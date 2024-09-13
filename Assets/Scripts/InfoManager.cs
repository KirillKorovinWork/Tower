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
        int remainingRedTanks = GetRemainingTanks(TeamObject.Team.Red);
        int remainingBlueTanks = GetRemainingTanks(TeamObject.Team.Blue);
        int remainingGreenTanks = GetRemainingTanks(TeamObject.Team.Green);
        int remainingYellowTanks = GetRemainingTanks(TeamObject.Team.Yellow);

        // Проверка, остался ли последний танк
        if ((remainingRedTanks > 0 && remainingBlueTanks == 0 && remainingGreenTanks == 0 && remainingYellowTanks == 0) ||
            (remainingBlueTanks > 0 && remainingRedTanks == 0 && remainingGreenTanks == 0 && remainingYellowTanks == 0) ||
            (remainingGreenTanks > 0 && remainingRedTanks == 0 && remainingBlueTanks == 0 && remainingYellowTanks == 0) ||
            (remainingYellowTanks > 0 && remainingRedTanks == 0 && remainingBlueTanks == 0 && remainingGreenTanks == 0))
        {
            EndGameWithVictory("The last tank standing wins!");
        }
    }

    private int GetRemainingTanks(TeamObject.Team team)
    {
        return FindObjectsOfType<Tank>().Count(tank => tank.team == team && !tank.destroyed);
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
    }

    
}
