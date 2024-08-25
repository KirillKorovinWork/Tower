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
        //Debug.Log($"Red: {redCubes}, Blue: {blueCubes}, Green: {greenCubes}, Yellow: {yellowCubes}");

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
        //Debug.Log($"Red: {redCubes}, Blue: {blueCubes}, Green: {greenCubes}, Yellow: {yellowCubes}");

        TextUpdate();
    }

    private void TextUpdate() 
    {
        redTeamText.text = "Team 1: " + redCubes.ToString();
        blueTeamText.text = "Team 2: " + blueCubes.ToString();
        greenTeamText.text = "Team 3: " + greenCubes.ToString();
        yellowTeamText.text = "Team 4: " + yellowCubes.ToString();
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
