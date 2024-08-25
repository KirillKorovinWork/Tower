using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 35f;
    public float rotationSpeed = 2f;
    public float lifetime = 5f;

    public GameObject explosionPrefab;
    private Transform target;
    

    private void Start()
    {
        Destroy(gameObject, lifetime); // ���������� ������ ����� ������������ �����
    }

    private void Update()
    {
        if (target == null)
        {
            // ���� ���� ��������, ������ ����� �� ������
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }
        transform.LookAt(target.position);
        // ��������� �� ����
        //Vector3 direction = (target.position - transform.position);
        //Quaternion lookRotation = Quaternion.LookRotation(direction);
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        // �������� ������ ������
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2.0f);
            // ����� ����� �������� ������ ������ ������, ����������� � �.�.
            other.gameObject.GetComponent<Tank>().Health(Random.Range(1,21));
            Destroy(gameObject);
        }
    }
}
