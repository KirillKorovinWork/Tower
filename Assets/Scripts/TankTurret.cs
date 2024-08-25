using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TankTurret : MonoBehaviour
{
    public GameObject turret; // Ссылка на башню танка
    public Transform defaultTurretPosition; // Положение башни по умолчанию
    public GameObject missilePrefab; // Префаб ракеты
    public Transform missileSpawnPoint; // Точка появления ракеты
    public float rotationSpeed = 5f;
    public float fireRate = 1.5f; // Скорость стрельбы
    public float missileSpeed = 20f; // Скорость ракеты
    public Animator turretAnimator; // Аниматор для управления анимацией башни

    private Transform target; // Текущая цель
    private Quaternion lookRotation;
    private float fireCooldown = 0f;
    public Tank tank;
    public bool tagetDetected = false;

    private void Start()
    {
        tank = GetComponentInParent<Tank>();
        //Debug.Log(tank.team);
    }

    private void Update()
    {
        if (!tank.destroyed) 
        {
            if (target != null)
            {
                //if (turretAnimator != null)
                //{
                //    // Включаем анимацию наведения башни на цель
                //    turretAnimator.SetBool("isAiming", true);
                //    turretAnimator.SetLookAtPosition(target.position);
                //}
                //else
                //{
                //    RotateTurretTowardsTarget();
                //}

                RotateTurretTowardsTarget();

                if (fireCooldown <= 0f)
                {
                    FireMissile();
                    fireCooldown = fireRate; // Запускаем перезарядку
                }
            }
            else
            {
                //if (turretAnimator != null)
                //{
                //    // Возвращаем башню в исходное положение
                //    turretAnimator.SetBool("isAiming", false);
                //}
                //else
                //{
                //    RotateTurretToDefault();
                //}
                RotateTurretToDefault();
            }

            fireCooldown -= Time.deltaTime;
        }
        else 
        {
            turret.transform.rotation = Quaternion.Euler(30, turret.transform.rotation.y, turret.transform.rotation.z);
        }
        
    }

    private void RotateTurretTowardsTarget()
    {
        turret.transform.LookAt(target.position);
        //Vector3 direction = (target.position - turret.transform.position).normalized;
        //Quaternion lookRotation = Quaternion.LookRotation(direction);
        //turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void RotateTurretToDefault()
    {
        //Debug.Log(tank.transform.localRotation.y);
        Vector3 rotationVector = new Vector3(0,tank.transform.eulerAngles.y,0);
        turret.transform.rotation = Quaternion.Euler(rotationVector);
        //lookRotation = Quaternion.Euler(rotationVector);
        //turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void FireMissile()
    {
        GameObject missile = Instantiate(missilePrefab, missileSpawnPoint.position, missileSpawnPoint.rotation);
        Missile missileScript = missile.GetComponent<Missile>();
        missileScript.SetTarget(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tank") && other.GetComponent<Tank>().team != tank.team && other.GetComponent<Tank>().destroyed == false)
        {
            target = other.transform;
            tagetDetected = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == target)
        {
            target = null;
            tagetDetected = false;
        }
    }
}
