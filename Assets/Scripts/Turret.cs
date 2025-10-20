using UnityEngine;

public class Turret : MonoBehaviour
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
    private TeamObject teamObject;
    private bool targetDetected = false;
    private AudioSource gunshotSound;

    private void Start()
    {
        teamObject = GetComponentInParent<TeamObject>();
        gunshotSound = gameObject.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!teamObject.destroyed && teamObject.team != TeamObject.Team.None)
        {
            if (target != null && !target.gameObject.GetComponent<TeamObject>().destroyed && teamObject.team != target.gameObject.GetComponent<TeamObject>().team)
            {
                RotateTurretTowardsTarget();

                if (fireCooldown <= 0f)
                {
                    if (gunshotSound.clip != null)
                    {
                        gunshotSound.Play();
                    }
                    FireMissile();
                    fireCooldown = fireRate;
                }
            }
            else
            {
                ClearTarget();
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
        Vector3 direction = (target.position - turret.transform.position).normalized;
        lookRotation = Quaternion.LookRotation(direction);
        turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void RotateTurretToDefault()
    {
        Vector3 rotationVector = new Vector3(0, teamObject.transform.eulerAngles.y, 0);
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
        TeamObject otherTeamObject = other.GetComponent<TeamObject>();

        if (otherTeamObject != null && otherTeamObject.team != teamObject.team && !otherTeamObject.destroyed && otherTeamObject.team != TeamObject.Team.None)
        {
            target = other.transform;
            targetDetected = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == target)
        {
            ClearTarget();
        }
    }

    private void ClearTarget()
    {
        target = null;
        targetDetected = false;
    }

    public bool HasTarget()
    {
        return target != null;
    }

    // **Добавленный метод для танка**
    public Transform GetTarget()
    {
        return target;
    }
}
