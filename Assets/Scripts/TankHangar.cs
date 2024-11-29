using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class TankHangar : TeamObject
{
    public TextMeshProUGUI healthText;

    [System.Serializable]
    public class TankSpawnData
    {
        public GameObject tankPrefab; // Префаб танка
        public float spawnChance;     // Шанс спавна
    }

    public List<TankSpawnData> tanks; // Список танков с шансами
    public Transform spawnPoint;     // Точка спавна танков
    public float spawnInterval = 5f; // Интервал между спавнами
    public int maxTanksToSpawn = 10; // Максимальное количество танков

    private float spawnTimer = 0f;  // Таймер для отслеживания времени
    private int spawnedTanks = 0;   // Счётчик уже созданных танков

    void Start()
    {
        SetTeamColor();
    }

    private void Update()
    {
        if (!destroyed)
        {
            // Если ангар принадлежит команде и ещё не достигнут лимит танков
            if (team != Team.None && spawnedTanks < maxTanksToSpawn)
            {
                spawnTimer += Time.deltaTime;

                // Когда таймер достигает интервала, спавним танк
                if (spawnTimer >= spawnInterval)
                {
                    SpawnTank();
                    spawnTimer = 0f; // Сбрасываем таймер после спавна
                }
            }

            healthText.text = health.ToString();
        }
        else
        {
            healthText.text = "X";
        }
    }

    private void SpawnTank()
    {
        GameObject selectedTank = GetRandomTank();
        if (selectedTank != null)
        {
            // Создаём новый танк
            GameObject newTank = Instantiate(selectedTank, spawnPoint.position, spawnPoint.rotation);

            // Смещаем танк по высоте на основе его "SpawnPoint"
            Transform spawnAnchor = newTank.transform.Find("SpawnPoint");
            if (spawnAnchor != null)
            {
                Vector3 offset = newTank.transform.position - spawnAnchor.position;
                newTank.transform.position += offset;
            }

            // Настраиваем команду для танка
            Tank tankComponent = newTank.GetComponent<Tank>();
            if (tankComponent != null)
            {
                tankComponent.team = this.team;
                tankComponent.SetTeamColor();
            }
            spawnedTanks++;
        }
    }

    private GameObject GetRandomTank()
    {
        float totalChance = 0;

        // Суммируем все шансы
        foreach (TankSpawnData tank in tanks)
        {
            totalChance += tank.spawnChance;
        }

        // Генерируем случайное число
        float randomValue = Random.Range(0, totalChance);
        float cumulativeChance = 0;

        // Ищем танк, соответствующий случайному числу
        foreach (TankSpawnData tank in tanks)
        {
            cumulativeChance += tank.spawnChance;
            if (randomValue <= cumulativeChance)
            {
                return tank.tankPrefab;
            }
        }

        return null; // Если список пуст или произошла ошибка
    }
}
