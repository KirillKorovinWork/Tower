using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManagement : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject team1Camera;
    public GameObject team2Camera;
    public GameObject team3Camera;
    public GameObject team4Camera;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            SetActiveCamera(mainCamera);
        }
        else if (Input.GetKey(KeyCode.Alpha1))
        {
            SetActiveCamera(team1Camera);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            SetActiveCamera(team2Camera);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            SetActiveCamera(team3Camera);
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            SetActiveCamera(team4Camera);
        }
    }

    private void SetActiveCamera(GameObject activeCamera)
    {
        mainCamera.SetActive(activeCamera == mainCamera);
        team1Camera.SetActive(activeCamera == team1Camera);
        team2Camera.SetActive(activeCamera == team2Camera);
        team3Camera.SetActive(activeCamera == team3Camera);
        team4Camera.SetActive(activeCamera == team4Camera);
    }
}
