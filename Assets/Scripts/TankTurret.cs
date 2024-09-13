using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TankTurret : MonoBehaviour
{
    public GameObject turret;
    public Transform defaultTurretPosition;
    public GameObject missilePrefab;
    public Transform missileSpawnPoint;
    public float rotationSpeed = 5f;
    public float fireRate = 1.5f;
    public float missileSpeed = 20f;

    private Transform target;
    private Quaternion lookRotation;
    private float fireCooldown = 0f;
    public Tank tank;
    public bool tagetDetected = false;

    private void Start()
    {
        tank = GetComponentInParent<Tank>();
    }

    private void Update()
    {
        if (!tank.destroyed) 
        {
            if (target != null)
            {
                RotateTurretTowardsTarget();

                if (fireCooldown <= 0f)
                {
                    FireMissile();
                    fireCooldown = fireRate;
                }
            }
            else
            {
                RotateTurretToDefault();
            }

            fireCooldown -= Time.deltaTime;
        }
        else 
        {
            turret.transform.rotation = Quaternion.Euler(30, turret.transform.eulerAngles.y, turret.transform.eulerAngles.z);
        }
        
    }

    private void RotateTurretTowardsTarget()
    {
        turret.transform.LookAt(target.position);
    }

    private void RotateTurretToDefault()
    {
        Vector3 rotationVector = new Vector3(0,tank.transform.eulerAngles.y,0);
        turret.transform.rotation = Quaternion.Euler(rotationVector);
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
