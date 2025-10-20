using UnityEngine;
using TMPro;

public class CubeManager : MonoBehaviour
{
    public static CubeManager Instance;

    private int redCubes = 0;
    private int blueCubes = 0;
    private int greenCubes = 0;
    private int yellowCubes = 0;

    public TextMeshProUGUI redTeamText;
    public TextMeshProUGUI blueTeamText;
    public TextMeshProUGUI greenTeamText;
    public TextMeshProUGUI yellowTeamText;

    void Awake()
    {
        // Singleton для простоты доступа к менеджеру
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        TextUpdate();
    }

    public void IncreaseTeamCount(TeamObject.Team team)
    {
        switch (team)
        {
            case TeamObject.Team.Red:
                redCubes++;
                break;
            case TeamObject.Team.Blue:
                blueCubes++;
                break;
            case TeamObject.Team.Green:
                greenCubes++;
                break;
            case TeamObject.Team.Yellow:
                yellowCubes++;
                break;
        }
        TextUpdate();
    }

    public void DecreaseTeamCount(TeamObject.Team team)
    {
        switch (team)
        {
            case TeamObject.Team.Red:
                redCubes--;
                break;
            case TeamObject.Team.Blue:
                blueCubes--;
                break;
            case TeamObject.Team.Green:
                greenCubes--;
                break;
            case TeamObject.Team.Yellow:
                yellowCubes--;
                break;
        }
        TextUpdate();
    }

    private void TextUpdate()
    {
        // Определяем лидирующую команду
        int maxCubes = Mathf.Max(redCubes, blueCubes, greenCubes, yellowCubes);

        UpdateTeamText(redTeamText, redCubes, maxCubes);
        UpdateTeamText(blueTeamText, blueCubes, maxCubes);
        UpdateTeamText(greenTeamText, greenCubes, maxCubes);
        UpdateTeamText(yellowTeamText, yellowCubes, maxCubes);
    }

    private void UpdateTeamText(TextMeshProUGUI teamText, int teamCubes, int maxCubes)
    {
        teamText.text = $"Team: {teamCubes}";
        // Увеличиваем размер текста, если команда лидирует
        teamText.fontSize = (teamCubes == maxCubes && teamCubes > 0) ? 50 : 40;
    }

    public int GetTeamCubeCount(TeamObject.Team team)
    {
        switch (team)
        {
            case TeamObject.Team.Red:
                return redCubes;
            case TeamObject.Team.Blue:
                return blueCubes;
            case TeamObject.Team.Green:
                return greenCubes;
            case TeamObject.Team.Yellow:
                return yellowCubes;
            default:
                return 0;
        }
    }
}
