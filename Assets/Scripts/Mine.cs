using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public GameObject explosionEffectPrefab; // Префаб эффекта взрыва
    public GameObject explosionSound;
    public float explosionRadius = 5f; // Радиус взрыва
    public float maxDamage = 50f; // Максимальный урон
    public float minDamage = 10f; // Минимальный урон

    private bool isExploded = false; // Флаг для предотвращения повторного взрыва
    private Collider explosionCollider; // Коллайдер взрывной волны

    void Start()
    {
        // Получаем второй коллайдер (взрывной волны)
        explosionCollider = GetComponent<Collider>();
        //explosionCollider.enabled = false; // Отключаем его до взрыва
    }

    // Когда танк входит в триггер, мина детонирует
    private void OnTriggerEnter(Collider other)
    {
        if (!isExploded && other.CompareTag("Tank"))
        {
            Detonate();
        }
    }

    private void Detonate()
    {
        GameObject explosionSFX = Instantiate(explosionSound, transform.position, Quaternion.identity);
        Destroy(explosionSFX, 2.0f);

        isExploded = true;

        // Создаём эффект взрыва
        var explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // Активируем взрывной коллайдер
        explosionCollider.enabled = true;

        // Наносим урон всем объектам в радиусе взрыва
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag("Tank"))
            {
                Tank tank = nearbyObject.GetComponent<Tank>();
                if (tank != null)
                {
                    float distance = Vector3.Distance(transform.position, tank.transform.position);
                    float damage = CalculateDamage(distance);
                    tank.Health((int)damage);
                }
            }
        }
        Destroy(explosion,2f);
        // Уничтожаем мину после взрыва
        Destroy(gameObject, 0.1f);
    }

    // Метод для расчета урона в зависимости от расстояния до центра взрыва
    private float CalculateDamage(float distance)
    {
        // Чем ближе к центру взрыва, тем выше урон
        float damage = maxDamage * (1 - (distance / explosionRadius));

        // Гарантируем, что урон не будет меньше минимального значения
        return Mathf.Clamp(damage, minDamage, maxDamage);
    }
}
