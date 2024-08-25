using UnityEngine;

public class Cube : TeamObject
{
    void Start()
    {
        // Устанавливаем цвет при запуске
        SetTeamColor();
    }

    // Когда танк сталкивается с кубом
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tank"))
        {
            Tank tank = other.GetComponent<Tank>();
            if (tank != null && tank.team != team)
            {
                // Уменьшаем счетчик для старой команды
                CubeManager.Instance.DecreaseTeamCount(team);

                // Меняем команду куба на команду танка
                team = tank.team;

                // Устанавливаем новый цвет
                SetTeamColor();

                // Увеличиваем счетчик для новой команды
                CubeManager.Instance.IncreaseTeamCount(team);
            }
        }
    }

}
