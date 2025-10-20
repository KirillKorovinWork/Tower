using UnityEngine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    public Camera rotatingCamera; // Камера, которая вращается по кругу
    public List<Camera> tankCameras = new List<Camera>(); // Список камер танков
    public float switchInterval = 10f; // Интервал переключения между камерами

    private float switchTimer = 0f;
    private int currentCameraIndex = 0; // Индекс текущей камеры
    private Camera currentCamera; // Текущая активная камера

    void Start()
    {
        // Проверяем, чтобы вращающаяся камера была активна вначале
        if (rotatingCamera != null)
        {
            ActivateCamera(rotatingCamera);
        }
    }

    void Update()
    {
        // Обновляем таймер переключения
        switchTimer += Time.deltaTime;

        // Если таймер превышает интервал, переключаем камеры
        if (switchTimer >= switchInterval)
        {
            switchTimer = 0f;
            SwitchCamera();
        }
        if(currentCamera != rotatingCamera) 
        {
            if (currentCamera.GetComponentInParent<Tank>().destroyed == true)
            {
                SwitchCamera();
            }
        }
    }

    private void SwitchCamera()
    {
        if (currentCamera == rotatingCamera)
        {
            // Переключаемся на камеру первого танка (или любого другого танка из списка)
            Camera tankCamera = GetAnyTankCamera();
            if (tankCamera != null)
            {
                ActivateCamera(tankCamera);
            }
        }
        else
        {
            // Переключаемся обратно на вращающуюся камеру
            ActivateCamera(rotatingCamera);
        }
    }

    private Camera GetAnyTankCamera()
    {
        // Просто возвращаем первую камеру из списка танков
        if (tankCameras.Count > 0)
        {
            return tankCameras[Random.Range(0, tankCameras.Count)];
        }

        return null; // Если камер танков нет
    }

    private void ActivateCamera(Camera cameraToActivate)
    {
        // Деактивируем текущую камеру
        if (currentCamera != null)
        {
            currentCamera.gameObject.SetActive(false);
        }

        // Активируем новую камеру
        currentCamera = cameraToActivate;
        currentCamera.gameObject.SetActive(true);
    }

    // Метод для добавления камеры танка
    public void AddTankCamera(Camera newTankCamera)
    {
        if (newTankCamera != null && !tankCameras.Contains(newTankCamera))
        {
            tankCameras.Add(newTankCamera);
        }
    }

    // Метод для удаления камеры танка (например, когда танк уничтожен)
    public void RemoveTankCamera(Camera tankCamera)
    {
        if (tankCameras.Contains(tankCamera))
        {
            tankCameras.Remove(tankCamera);
        }
    }
}
