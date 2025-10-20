using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Tank : TeamObject
{
    // Для совместимости с менеджерами
    public enum TankState { Exploring, Attacking, Defending }

    // FSM состояния
    private enum MainState { Exploring, Attacking, Defending }
    private enum ExploringSubState { Normal, Stuck, IdleWander }
    private enum AttackingSubState { Aggressive, Retreating, Flanking }
    private enum DefendingSubState { HoldPosition, PursueEnemy }

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI stateText;
    public Camera tankCamera;

    [Header("NavMesh")]
    public float speed = 5f;
    public float stoppingDistance = 1.5f;
    public float attackStopDistance = 2.5f;

    [Header("AI")]
    public float lowHealthPercent = 0.3f;
    public float highHealthPercent = 0.7f;
    public float stuckDistanceThreshold = 0.5f;
    public float stuckDistanceDuration = 2f;
    public float explorationStep = 5f;
    public float maxExplorationRadius = 30f;
    public float idleWanderTime = 3f; // Сколько секунд стоять без дела, прежде чем разбежаться

    // FSM State
    private MainState currentMainState = MainState.Exploring;
    private ExploringSubState exploringSubState = ExploringSubState.Normal;
    private AttackingSubState attackingSubState = AttackingSubState.Aggressive;
    private DefendingSubState defendingSubState = DefendingSubState.HoldPosition;

    // Internal
    private NavMeshAgent navMeshAgent;
    private Turret turret;
    private Transform target;
    private Vector3 explorationCenter = Vector3.zero;
    private float explorationRadius = 5f;
    private float stuckDistanceTime = 0f;
    private float lastDistanceToTarget = 0f;
    private bool isSinking = false;
    private float noMoveTime = 0f;
    private Vector3 lastPosition = Vector3.zero;

    // Для разделения целей между танками
    private static Dictionary<int, int> cubeAssignments = new Dictionary<int, int>();
    private const int maxTanksPerCube = 1;

    void Start()
    {
        SetTeamColor();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.autoBraking = true;
        turret = GetComponentInChildren<Turret>();
        ShowState();
        CameraManager cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager != null) cameraManager.AddTankCamera(tankCamera);
        StartCoroutine(StateHandler());
    }

    void Update()
    {
        UpdateHealthUI();
        if (destroyed && !isSinking)
            StartCoroutine(SinkAndDestroy());

        // Проверка stuck по расстоянию до цели
        if (currentMainState == MainState.Exploring && target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (Mathf.Abs(distance - lastDistanceToTarget) < stuckDistanceThreshold)
            {
                stuckDistanceTime += Time.deltaTime;
                if (stuckDistanceTime > stuckDistanceDuration)
                    exploringSubState = ExploringSubState.Stuck;
            }
            else
                stuckDistanceTime = 0;
            lastDistanceToTarget = distance;
        }

        // Проверка — если не двигается по позиции
        if (currentMainState == MainState.Exploring)
        {
            if (Vector3.Distance(transform.position, lastPosition) < 0.05f)
                noMoveTime += Time.deltaTime;
            else
                noMoveTime = 0f;

            lastPosition = transform.position;

            if (noMoveTime > idleWanderTime)
            {
                exploringSubState = ExploringSubState.IdleWander;
                noMoveTime = 0f;
            }
        }
    }

    private IEnumerator StateHandler()
    {
        while (!destroyed)
        {
            switch (currentMainState)
            {
                case MainState.Exploring: ExploringFSM(); break;
                case MainState.Attacking: AttackingFSM(); break;
                case MainState.Defending: DefendingFSM(); break;
            }
            ShowState();
            yield return new WaitForSeconds(0.5f);
        }
    }

    #region FSM LOGIC

    void ExploringFSM()
    {
        switch (exploringSubState)
        {
            case ExploringSubState.Normal:
                if (explorationCenter == Vector3.zero) SetExplorationCenter();
                if (target == null || navMeshAgent.remainingDistance <= stoppingDistance)
                    SetExplorationTarget();
                // Если рядом враг — переход в атаку
                var enemy = FindClosestEnemy();
                if (enemy != null && Vector3.Distance(transform.position, enemy.position) < 10f)
                    SetState(MainState.Attacking, enemy);
                break;
            case ExploringSubState.Stuck:
                SetExplorationTarget();
                exploringSubState = ExploringSubState.Normal;
                stuckDistanceTime = 0;
                break;
            case ExploringSubState.IdleWander:
                // Делаем небольшой разбег (wander) в случайную достижимую точку поблизости
                for (int i = 0; i < 10; i++)
                {
                    Vector2 randomPoint = Random.insideUnitCircle * 5f;
                    Vector3 wanderPosition = transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);
                    if (IsReachable(wanderPosition))
                    {
                        navMeshAgent.SetDestination(wanderPosition);
                        Debug.Log($"{gameObject.name}: Wandering due to idle");
                        break;
                    }
                }
                exploringSubState = ExploringSubState.Normal;
                break;
        }
    }

    void AttackingFSM()
    {
        // Проверка достижимости цели
        if (target == null || !IsReachable(target.position))
        {
            // Нет цели или она недостижима — идём исследовать дальше!
            SetState(MainState.Exploring, null);
            SetExplorationTarget();
            return;
        }

        Tank enemyTank = target.GetComponent<Tank>();
        if (enemyTank == null || enemyTank.destroyed)
        {
            SetState(MainState.Exploring, null);
            SetExplorationTarget();
            return;
        }

        float myPercent = Mathf.Clamp01((float)health / 100f);
        float enemyPercent = (enemyTank.health > 0) ? Mathf.Clamp01((float)enemyTank.health / 100f) : 0f;
        float distance = Vector3.Distance(transform.position, target.position);

        if (myPercent < lowHealthPercent)
            attackingSubState = AttackingSubState.Retreating;
        else if (myPercent > highHealthPercent && enemyPercent < lowHealthPercent)
            attackingSubState = AttackingSubState.Aggressive;
        else if (enemyPercent > myPercent * 1.3f)
            attackingSubState = AttackingSubState.Flanking;
        else
            attackingSubState = AttackingSubState.Aggressive;

        switch (attackingSubState)
        {
            case AttackingSubState.Aggressive:
                navMeshAgent.isStopped = false;
                if (distance > attackStopDistance)
                    navMeshAgent.SetDestination(target.position);
                else
                    navMeshAgent.isStopped = true;
                break;

            case AttackingSubState.Retreating:
                var basePos = FindNearestFriendlyHangar();
                if (basePos != null && IsReachable(basePos.position))
                {
                    navMeshAgent.SetDestination(basePos.position);
                    navMeshAgent.isStopped = false;
                }
                else
                {
                    SetState(MainState.Exploring, null);
                    SetExplorationTarget();
                }
                break;

            case AttackingSubState.Flanking:
                Vector3 dir = (transform.position - enemyTank.transform.position).normalized;
                Vector3 flankPos = transform.position + Quaternion.Euler(0, 90, 0) * dir * 7f;
                if (IsReachable(flankPos))
                {
                    navMeshAgent.SetDestination(flankPos);
                    navMeshAgent.isStopped = false;
                }
                else
                {
                    SetState(MainState.Exploring, null);
                    SetExplorationTarget();
                }
                break;
        }
    }

    void DefendingFSM()
    {
        switch (defendingSubState)
        {
            case DefendingSubState.HoldPosition:
                var friendly = FindNearestFriendlyHangar();
                if (friendly != null && IsReachable(friendly.position))
                {
                    navMeshAgent.SetDestination(friendly.position);
                    navMeshAgent.isStopped = false;
                }
                var enemy = FindClosestEnemy();
                if (enemy != null && Vector3.Distance(transform.position, enemy.position) < 12f)
                {
                    defendingSubState = DefendingSubState.PursueEnemy;
                    target = enemy;
                }
                break;
            case DefendingSubState.PursueEnemy:
                if (target != null && IsReachable(target.position))
                {
                    navMeshAgent.SetDestination(target.position);
                    navMeshAgent.isStopped = false;
                    if (Vector3.Distance(transform.position, target.position) > 15f)
                        defendingSubState = DefendingSubState.HoldPosition;
                }
                else
                    defendingSubState = DefendingSubState.HoldPosition;
                break;
        }
    }

    #endregion

    #region STATE SETTERS

    // Для FSM
    private void SetState(MainState mainState, Transform newTarget = null)
    {
        currentMainState = mainState;
        target = newTarget;
        if (mainState == MainState.Exploring)
            exploringSubState = ExploringSubState.Normal;
        if (mainState == MainState.Attacking)
            attackingSubState = AttackingSubState.Aggressive;
        if (mainState == MainState.Defending)
            defendingSubState = DefendingSubState.HoldPosition;
    }

    // Для совместимости со старыми менеджерами
    public void SetState(TankState newState, Transform newTarget, float stopDist = 0f)
    {
        switch (newState)
        {
            case TankState.Exploring: SetState(MainState.Exploring, newTarget); break;
            case TankState.Attacking: SetState(MainState.Attacking, newTarget); break;
            case TankState.Defending: SetState(MainState.Defending, newTarget); break;
        }
        if (navMeshAgent != null && !destroyed && newTarget != null)
        {
            navMeshAgent.SetDestination(newTarget.position);
            navMeshAgent.stoppingDistance = stopDist;
            navMeshAgent.isStopped = false;
        }
    }

    // Для совместимости: перемещение на оборонительную позицию
    public void SetDefensivePosition(Vector3 position)
    {
        if (navMeshAgent != null && !destroyed)
        {
            navMeshAgent.SetDestination(position);
            navMeshAgent.stoppingDistance = 1.5f;
            navMeshAgent.isStopped = false;
        }
    }

    #endregion

    #region HELPERS

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = destroyed ? "X" : health.ToString();
    }

    private void ShowState()
    {
        if (stateText == null) return;
        string s = currentMainState.ToString();
        switch (currentMainState)
        {
            case MainState.Exploring: s += $" ({exploringSubState})"; break;
            case MainState.Attacking: s += $" ({attackingSubState})"; break;
            case MainState.Defending: s += $" ({defendingSubState})"; break;
        }
        stateText.text = s;
    }

    private void SetExplorationCenter()
    {
        TankHangar[] hangars = FindObjectsOfType<TankHangar>();
        float minDist = Mathf.Infinity;
        Transform closest = null;
        foreach (var hangar in hangars)
        {
            if (hangar.team == team && !hangar.destroyed)
            {
                float dist = Vector3.Distance(transform.position, hangar.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hangar.transform;
                }
            }
        }
        if (closest != null)
            explorationCenter = closest.position;
    }

    private void OnDestroy()
    {
        // Если умирал или менял цель, снимаем "назначение"
        if (target != null && target.CompareTag("Cube"))
        {
            int id = target.gameObject.GetInstanceID();
            if (cubeAssignments.ContainsKey(id))
            {
                cubeAssignments[id] = Mathf.Max(0, cubeAssignments[id] - 1);
                if (cubeAssignments[id] == 0)
                    cubeAssignments.Remove(id);
            }
        }
    }

    private void SetExplorationTarget()
    {
        // Снимаем старое назначение
        if (target != null && target.CompareTag("Cube"))
        {
            int id = target.gameObject.GetInstanceID();
            if (cubeAssignments.ContainsKey(id))
            {
                cubeAssignments[id] = Mathf.Max(0, cubeAssignments[id] - 1);
                if (cubeAssignments[id] == 0)
                    cubeAssignments.Remove(id);
            }
        }

        Transform explorationTarget = FindBestExplorationTarget();
        if (explorationTarget != null)
        {
            target = explorationTarget;
            navMeshAgent.SetDestination(target.position);

            // Обновляем новый assignment
            int id = target.gameObject.GetInstanceID();
            if (!cubeAssignments.ContainsKey(id))
                cubeAssignments[id] = 0;
            cubeAssignments[id]++;
        }
        else
        {
            // fallback: random nearby position
            if (explorationRadius < maxExplorationRadius)
                explorationRadius += explorationStep;
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 randomPoint = Random.insideUnitCircle * explorationRadius;
                    Vector3 newTargetPosition = explorationCenter + new Vector3(randomPoint.x, 0, randomPoint.y);
                    if (IsReachable(newTargetPosition))
                    {
                        navMeshAgent.SetDestination(newTargetPosition);
                        break;
                    }
                }
            }
        }
    }

    private bool IsReachable(Vector3 point)
    {
        NavMeshPath path = new NavMeshPath();
        if (navMeshAgent.CalculatePath(point, path))
            return path.status == NavMeshPathStatus.PathComplete;
        return false;
    }

    private Transform FindBestExplorationTarget()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        Transform bestCube = null;
        float minDist = Mathf.Infinity;

        // 1. Ищем ближайший достижимый незахваченный (нейтральный) и не занятый
        foreach (var cube in cubes)
        {
            Cube c = cube.GetComponent<Cube>();
            int id = cube.GetInstanceID();
            int assigned = cubeAssignments.ContainsKey(id) ? cubeAssignments[id] : 0;
            if (c != null && c.team == Team.None && assigned < maxTanksPerCube && IsReachable(cube.transform.position))
            {
                float dist = Vector3.Distance(transform.position, cube.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestCube = cube.transform;
                }
            }
        }

        // 2. Если нейтральных нет — ищем ближайший достижимый вражеский и не занятый
        if (bestCube == null)
        {
            foreach (var cube in cubes)
            {
                Cube c = cube.GetComponent<Cube>();
                int id = cube.GetInstanceID();
                int assigned = cubeAssignments.ContainsKey(id) ? cubeAssignments[id] : 0;
                if (c != null && c.team != team && assigned < maxTanksPerCube && IsReachable(cube.transform.position))
                {
                    float dist = Vector3.Distance(transform.position, cube.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestCube = cube.transform;
                    }
                }
            }
        }
        return bestCube;
    }

    private Transform FindClosestEnemy()
    {
        Tank[] allTanks = FindObjectsOfType<Tank>();
        float minDist = Mathf.Infinity;
        Transform closest = null;
        foreach (var t in allTanks)
        {
            if (t.team != team && !t.destroyed)
            {
                float d = Vector3.Distance(transform.position, t.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = t.transform;
                }
            }
        }
        return closest;
    }

    private Transform FindNearestFriendlyHangar()
    {
        TankHangar[] hangars = FindObjectsOfType<TankHangar>();
        float minDist = Mathf.Infinity;
        Transform closest = null;
        foreach (var hangar in hangars)
        {
            if (hangar.team == team && !hangar.destroyed)
            {
                float dist = Vector3.Distance(transform.position, hangar.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hangar.transform;
                }
            }
        }
        return closest;
    }

    private IEnumerator SinkAndDestroy()
    {
        isSinking = true;
        navMeshAgent.enabled = false;
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
    #endregion
}
