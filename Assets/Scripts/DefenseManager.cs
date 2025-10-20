using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class DefenseManager : MonoBehaviour
{
    private List<Tank> defendingTanks = new List<Tank>();
    private Transform friendlyHangar;
    private TeamObject teamObject;
    private List<Tank> attackingEnemies = new List<Tank>();

    public float defenseRadius = 5f; // Радиус защиты вокруг ангара
    public float repositionInterval = 10f; // Время перед перераспределением
    public LayerMask navMeshLayer; // Слой для поиска на NavMesh

    void Start()
    {
        teamObject = GetComponent<TeamObject>();
        UpdateFriendlyHangar();
    }

    private void UpdateFriendlyHangar()
    {
        TankHangar[] allHangars = FindObjectsOfType<TankHangar>();
        foreach (var hangar in allHangars)
        {
            if (hangar.team == teamObject.team && !hangar.destroyed)
            {
                friendlyHangar = hangar.transform;
                return;
            }
        }

        friendlyHangar = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        Tank enemyTank = other.GetComponent<Tank>();

        if (enemyTank != null && enemyTank.team != TeamObject.Team.None && enemyTank.team != teamObject.team)
        {
            if (!attackingEnemies.Contains(enemyTank))
            {
                attackingEnemies.Add(enemyTank);
                CallForDefense();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Tank enemyTank = other.GetComponent<Tank>();

        if (enemyTank != null && attackingEnemies.Contains(enemyTank))
        {
            attackingEnemies.Remove(enemyTank);
        }
    }

    public void CallForDefense()
    {
        UpdateFriendlyHangar();

        if (friendlyHangar == null)
        {
            Debug.LogWarning("DefenseManager: No friendly hangar found! Defense canceled.");
            return;
        }

        defendingTanks.Clear();
        foreach (Tank tank in FindObjectsOfType<Tank>())
        {
            if (!tank.destroyed && tank.team == teamObject.team)
            {
                defendingTanks.Add(tank);
            }
        }

        if (defendingTanks.Count > 0)
        {
            AssignDefensePositions();
            Debug.Log($"DefenseManager: Assigned {defendingTanks.Count} tanks to defend {friendlyHangar.name}");
        }
        else
        {
            Debug.LogWarning("DefenseManager: No available tanks for defense!");
        }
    }

    private void AssignDefensePositions()
    {
        for (int i = 0; i < defendingTanks.Count; i++)
        {
            Tank tank = defendingTanks[i];
            Vector3 defensePosition = GetValidDefensePosition(i);
            tank.SetState(Tank.TankState.Defending, friendlyHangar, 2.5f);
            tank.SetDefensivePosition(defensePosition);
        }
    }

    private Vector3 GetValidDefensePosition(int index)
    {
        float angle = (360f / defendingTanks.Count) * index;
        float radian = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian)) * defenseRadius;
        Vector3 desiredPosition = friendlyHangar.position + offset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredPosition, out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position; // Если точка валидна, берем ее
        }
        else
        {
            // Если не нашли, ищем ближе к ангару
            for (float r = defenseRadius; r > 1f; r -= 1f)
            {
                Vector3 adjustedOffset = new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian)) * r;
                Vector3 adjustedPosition = friendlyHangar.position + adjustedOffset;

                if (NavMesh.SamplePosition(adjustedPosition, out hit, 2f, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }

        return friendlyHangar.position; // В крайнем случае остается около ангара
    }

    private void Update()
    {
        if (attackingEnemies.Count > 0)
        {
            foreach (Tank tank in defendingTanks)
            {
                if (!tank.destroyed)
                {
                    Tank nearestEnemy = GetClosestAttacker(tank.transform.position);
                    if (nearestEnemy != null)
                    {
                        tank.SetState(Tank.TankState.Attacking, nearestEnemy.transform, 1.5f);
                    }
                }
            }
        }
    }

    private Tank GetClosestAttacker(Vector3 position)
    {
        Tank closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Tank enemy in attackingEnemies)
        {
            if (enemy != null && !enemy.destroyed)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }
}
