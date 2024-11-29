using UnityEngine;
using UnityEngine.UI;

public class CaptureableObject : MonoBehaviour
{
    public TeamObject teamObject; // Ссылка на компонент TeamObject
    public float captureTime = 5f; // Время для полного захвата
    public Image backgroundImage; // Изображение фона (белый круг)
    public Image fillImage; // Изображение для заполнения прогресса захвата
    public GameObject captureUI; // UI для захвата, отображаемое только при незахваченном объекте
    public GameObject captureVisual; // Визуализация захвата (круг)

    private Color neutralColor = Color.white; // Цвет нейтрального объекта
    private Renderer captureVisualRenderer; // Рендерер круга для визуализации захвата
    private float captureProgress = 0f; // Текущий прогресс захвата
    private bool isCaptured = false; // Флаг, указывает на захват объекта
    private TeamObject.Team capturingTeam = TeamObject.Team.None; // Команда, которая пытается захватить объект

    private void Start()
    {
        if (teamObject == null)
        {
            teamObject = GetComponent<TeamObject>(); // Автоматическое получение TeamObject, если оно не назначено вручную
        }

        SetTeamColor(); // Устанавливаем цвет объекта при старте
        ResetCaptureVisual(); // Устанавливаем начальное состояние визуализации

        captureUI.SetActive(true); // UI захвата активен только для незахваченных объектов
        captureVisual.SetActive(true); // Визуализация активна в начале
        SetCaptureVisualColor(neutralColor); // Начальный цвет захватного круга нейтральный
    }

    private void OnTriggerStay(Collider other)
    {
        // Проверяем, является ли объект танком и принадлежит ли он команде
        Tank tank = other.GetComponent<Tank>();
        if (tank != null && !isCaptured && tank.team != TeamObject.Team.None)
        {
            // Если объект нейтральный или захватывается командой
            if (capturingTeam == TeamObject.Team.None || capturingTeam == tank.team)
            {
                capturingTeam = tank.team;
                captureProgress += Time.deltaTime; // Увеличиваем прогресс захвата

                // Обновляем визуализацию захвата
                UpdateCaptureVisual(captureProgress / captureTime);
                SetCaptureVisualColor(GetColorByTeam(capturingTeam)); // Изменяем цвет круга в зависимости от команды

                // Если захват завершен
                if (captureProgress >= captureTime)
                {
                    CaptureComplete(tank.team); // Захват завершен
                }
            }
            else
            {
                // Если другая команда начала захват, сбрасываем прогресс
                ResetCapture();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Если танк покинул область захвата
        Tank tank = other.GetComponent<Tank>();
        if (tank != null && tank.team == capturingTeam)
        {
            capturingTeam = TeamObject.Team.None;
        }
    }

    private void ResetCapture()
    {
        capturingTeam = TeamObject.Team.None;
        captureProgress = 0f;
        ResetCaptureVisual(); // Сбрасываем визуализацию захвата
        SetCaptureVisualColor(neutralColor); // Сбрасываем цвет на нейтральный
    }

    private void CaptureComplete(TeamObject.Team team)
    {
        isCaptured = true;
        captureUI.SetActive(false); // Скрываем UI после захвата
        captureVisual.SetActive(false); // Выключаем визуализацию захвата
        teamObject.team = team; // Устанавливаем команду захватившего объекта
        SetTeamColor(); // Меняем цвет объекта в зависимости от команды
    }

    private void SetTeamColor()
    {
        if (!isCaptured)
        {
            // Если объект еще не захвачен, он нейтрального цвета
            foreach (Renderer rend in teamObject.GetComponentsInChildren<Renderer>())
            {
                rend.material.color = neutralColor;
            }
        }
        else
        {
            // Устанавливаем цвет объекта в зависимости от команды
            teamObject.SetTeamColor();
        }
    }

    private void SetCaptureVisualColor(Color color)
    {
        // Устанавливаем цвет визуализации захвата (круг)
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }

    private Color GetColorByTeam(TeamObject.Team team)
    {
        // Получение цвета по команде (можно изменить для разных команд)
        switch (team)
        {
            case TeamObject.Team.Red:
                return Color.red;
            case TeamObject.Team.Blue:
                return Color.blue;
            case TeamObject.Team.Green:
                return Color.green;
            case TeamObject.Team.Yellow:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    private void UpdateCaptureVisual(float progress)
    {
        // Обновляем полосу захвата
        fillImage.fillAmount = Mathf.Clamp01(progress);
    }

    private void ResetCaptureVisual()
    {
        // Сбрасываем состояние визуализации
        fillImage.fillAmount = 0f;
    }
}

