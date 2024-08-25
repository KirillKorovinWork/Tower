using UnityEngine;

public abstract class TeamObject : MonoBehaviour
{
    public int health = 100;
    public bool destroyed = false;

    public enum Team
    {
        None,
        Red,
        Blue,
        Green,
        Yellow
    }

    public Team team = Team.None; // Команда, к которой принадлежит объект

    [SerializeField] private Renderer[] teamRenderers; // Массив рендереров, для которых нужно менять цвет

    // Метод для изменения цвета в зависимости от команды
    public virtual void SetTeamColor()
    {
        Color teamColor = GetColorByTeam(team);

        foreach (Renderer rend in teamRenderers)
        {
            rend.material.color = teamColor;
        }
    }

    // Метод для получения цвета по команде
    protected Color GetColorByTeam(Team team)
    {
        switch (team)
        {
            case Team.Red:
                return Color.red;
            case Team.Blue:
                return Color.blue;
            case Team.Green:
                return Color.green;
            case Team.Yellow:
                return Color.yellow;
            default:
                return Color.white;
        }
    }


    public virtual void Health(int damage) 
    {
        health -= damage;
        if (health <= 0) 
        {
            destroyed = true;
        }
    }
}

