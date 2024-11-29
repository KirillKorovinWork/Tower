using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    public static InfoManager Instance;

    public float gameTime = 300f; // Время игры в секундах (например, 5 минут)
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI conditionText;
    public GameObject infoPanel;

    private float remainingTime;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        remainingTime = gameTime;
        StartCoroutine(DisplayConditions());
        StartCoroutine(GameTimer());
    }

    private IEnumerator DisplayConditions()
    {
        ShowConditions();
        yield return new WaitForSeconds(5f);
        CloseConditions();
    }

    void ShowConditions() 
    {
        conditionText.gameObject.SetActive(true);
        infoPanel.SetActive(true);
}

    void CloseConditions() 
    {
        conditionText.gameObject.SetActive(false);
        infoPanel.SetActive(false);
    }

    private IEnumerator GameTimer()
    {
        while (remainingTime > 0)
        {
            if (!gameEnded)
            {
                remainingTime -= Time.deltaTime;
                timerText.text = "Time: " + Mathf.CeilToInt(remainingTime).ToString();
                CheckVictoryConditions();
            }
            yield return null;
        }

        if (!gameEnded)
        {
            DetermineVictoryByPoints();
        }
    }

    private void CheckVictoryConditions()
    {
        int remainingRedObjects = GetRemainingObjects(TeamObject.Team.Red);
        int remainingBlueObjects = GetRemainingObjects(TeamObject.Team.Blue);
        int remainingGreenObjects = GetRemainingObjects(TeamObject.Team.Green);
        int remainingYellowObjects = GetRemainingObjects(TeamObject.Team.Yellow);

        // Проверка, остался ли последний танк
        if ((remainingRedObjects > 0 && remainingBlueObjects == 0 && remainingGreenObjects == 0 && remainingYellowObjects == 0) ||
            (remainingBlueObjects > 0 && remainingRedObjects == 0 && remainingGreenObjects == 0 && remainingYellowObjects == 0) ||
            (remainingGreenObjects > 0 && remainingRedObjects == 0 && remainingBlueObjects == 0 && remainingYellowObjects == 0) ||
            (remainingYellowObjects > 0 && remainingRedObjects == 0 && remainingBlueObjects == 0 && remainingGreenObjects == 0))
        {
            EndGameWithVictory("The last team standing wins!");
        }
    }

    private int GetRemainingObjects(TeamObject.Team team)
    {
        return FindObjectsOfType<TeamObject>().Count(obj =>
            obj.team == team &&
            !obj.destroyed &&
            !(obj is Cube)); // Исключаем объекты типа Cube
    }

    private void DetermineVictoryByPoints()
    {
        int redPoints = CubeManager.Instance.GetTeamCubeCount(TeamObject.Team.Red);
        int bluePoints = CubeManager.Instance.GetTeamCubeCount(TeamObject.Team.Blue);
        int greenPoints = CubeManager.Instance.GetTeamCubeCount(TeamObject.Team.Green);
        int yellowPoints = CubeManager.Instance.GetTeamCubeCount(TeamObject.Team.Yellow);

        int maxPoints = Mathf.Max(redPoints, bluePoints, greenPoints, yellowPoints);
        string winningTeam = "";

        if (redPoints == maxPoints) winningTeam = "Red Team";
        else if (bluePoints == maxPoints) winningTeam = "Blue Team";
        else if (greenPoints == maxPoints) winningTeam = "Green Team";
        else if (yellowPoints == maxPoints) winningTeam = "Yellow Team";

        EndGameWithVictory(winningTeam + " wins by points!");
    }

    private void EndGameWithVictory(string message)
    {
        gameEnded = true;
        victoryText.text = message;
        StartCoroutine(DisplayVictoryMessage());
    }

    private IEnumerator DisplayVictoryMessage()
    {
        infoPanel.gameObject.SetActive(true);
        victoryText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        infoPanel.gameObject.SetActive(false);
        victoryText.gameObject.SetActive(false);
    }
}
