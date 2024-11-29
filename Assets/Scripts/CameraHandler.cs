using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public GameObject cameraFly;
    public GameObject cameraPlayer;
    private bool isCameraActive = true;
    private void Start()
    {
        cameraFly.SetActive(true);
        cameraPlayer.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    private void SwitchCamera()
    {
        isCameraActive = !isCameraActive;

        cameraFly.SetActive(isCameraActive);
        cameraPlayer.SetActive(!isCameraActive);
    }
}
