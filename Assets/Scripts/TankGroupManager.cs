using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TankGroupManager : MonoBehaviour
{
    public TeamObject.Team playerTeam = TeamObject.Team.None;
    public float attackIntervalMin = 240f;
    public float attackIntervalMax = 600f;

    private List<Tank> availableTanks = new List<Tank>();
    private List<Tank> assignedTanks = new List<Tank>();
    private List<Transform> enemyHangars = new List<Transform>();
    private Transform friendlyHangar;

    void Start()
    {
        if (playerTeam == TeamObject.Team.None)
        {
            Debug.LogError("TankGroupManager: Team is not assigned!");
            return;
        }

        StartCoroutine(PlanAttackRoutine());
    }

    private IEnumerator PlanAttackRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);

            UpdateHangars();
            AssignTanksToObjective();
        }
    }

    private void UpdateHangars()
    {
        enemyHangars.Clear();
        friendlyHangar = null;

        TankHangar[] allHangars = FindObjectsOfType<TankHangar>();

        foreach (var hangar in allHangars)
        {
            if (hangar.team == playerTeam)
            {
                friendlyHangar = hangar.transform;
            }
            else if (!hangar.destroyed && !enemyHangars.Contains(hangar.transform))
            {
                enemyHangars.Add(hangar.transform);
            }
        }
    }

    private void AssignTanksToObjective()
    {
        availableTanks.Clear();
        foreach (Tank tank in FindObjectsOfType<Tank>())
        {
            if (!tank.destroyed && tank.team == playerTeam && !assignedTanks.Contains(tank))
            {
                availableTanks.Add(tank);
            }
        }

        if (availableTanks.Count >= 6 && enemyHangars.Count > 0)
        {
            CreateAttackGroup();
        }
    }

    private void CreateAttackGroup()
    {
        int groupSize = Random.Range(4, 6);
        List<Tank> attackGroup = new List<Tank>();

        for (int i = 0; i < groupSize && availableTanks.Count > 0; i++)
        {
            Tank selectedTank = availableTanks[0];
            availableTanks.RemoveAt(0);
            assignedTanks.Add(selectedTank);
            attackGroup.Add(selectedTank);
        }
    }
}
