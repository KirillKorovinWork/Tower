using UnityEngine;

public class Cube : TeamObject
{
    void Start()
    {
        SetTeamColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tank"))
        {
            Tank tank = other.GetComponent<Tank>();
            if (tank != null && tank.team != team)
            {
                CubeManager.Instance.DecreaseTeamCount(team);
                team = tank.team;
                SetTeamColor();
                CubeManager.Instance.IncreaseTeamCount(team);
            }
        }
    }

}
