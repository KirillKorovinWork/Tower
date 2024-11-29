using TMPro;
using UnityEngine;
public class Tower : TeamObject
{
    public TextMeshProUGUI healthText;
    void Start()
    {
        SetTeamColor();
    }

    private void Update()
    {
        if (!destroyed)
        {
            healthText.text = health.ToString();
        }

        if (destroyed)
        {
            healthText.text = "X";
        }
    }
}
