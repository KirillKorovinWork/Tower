using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 35f;
    public float rotationSpeed = 2f;
    public float lifetime = 5f;

    public GameObject explosionPrefab;
    public GameObject explosionSound;
    private Transform target;


    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    private void Update()
    {
        if (target == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }
        transform.LookAt(target.position);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target || other.CompareTag("Wall"))
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            GameObject explosionSFX = Instantiate(explosionSound, transform.position, Quaternion.identity);
            Destroy(explosion, 2.0f);
            Destroy(explosionSFX, 2.0f);
            if (other.transform == target) 
            {
                other.gameObject.GetComponent<Tank>().Health(Random.Range(1, 21));
            }
            Destroy(gameObject);
        }
    }
}
