using UnityEngine;
using TMPro;

public class Tank : TeamObject
{
    public TextMeshProUGUI tankHealthText;
    public float speed = 5f;
    public float speedLessHP = 2f;
    public float rotationSpeed = 180f;
    public int minCubesToCapture = 5;
    public int maxCubesToCapture = 10;
    public int rayLenght = 2;

    private int capturedCubes = 0;
    private int cubesToCapture;
    private bool isMoving = true;
    
    void Start()
    {
        SetTeamColor();
        cubesToCapture = Random.Range(minCubesToCapture, maxCubesToCapture + 1);
    }

    void Update()
    {
        if (isMoving && !destroyed)
        {
            MoveForward();
            DetectObstacle();

            tankHealthText.text = health.ToString(); 
        }

        if (destroyed) 
        {
            tankHealthText.text = "X";
        }
    }

    private void MoveForward()
    {
        if (health < 50) 
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else 
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    private void DetectObstacle()
    {
        RaycastHit hit;
        Vector3 forward = transform.forward;

        // Визуализация луча
        Debug.DrawRay(transform.position, forward * rayLenght, Color.green);

        if (Physics.Raycast(transform.position, transform.forward, out hit, rayLenght))
        {
            if ((hit.collider.CompareTag("Cube") && hit.collider.GetComponent<Cube>().team != team))
            {
                TurnAround();
            }

            if (hit.collider.CompareTag("Wall")) 
            {
                TurnAround();
            }
        }
        // Луч под углом 45 градусов вправо
        RaycastHit hitRight;
        Vector3 rightDirection = Quaternion.Euler(0, 45f, 0) * transform.forward;
        Debug.DrawRay(transform.position, rightDirection * rayLenght, Color.green);
        if (Physics.Raycast(transform.position, rightDirection, out hitRight, rayLenght))
        {

            if (hitRight.collider.CompareTag("Cube") && hitRight.collider.GetComponent<Cube>().team != team)
            {
                TurnAround();

            }
            if (hitRight.collider.CompareTag("Wall"))
            {
                TurnAround();
                return;
            }
        }

        // Луч под углом 45 градусов влево
        RaycastHit hitLeft;
        Vector3 leftDirection = Quaternion.Euler(0, -45f, 0) * transform.forward;
        Debug.DrawRay(transform.position, leftDirection * rayLenght, Color.green);
        if (Physics.Raycast(transform.position, leftDirection, out hitLeft, rayLenght))
        {

            if (hitLeft.collider.CompareTag("Cube") && hitLeft.collider.GetComponent<Cube>().team != team)
            {
                TurnAround();
            }
            if (hitLeft.collider.CompareTag("Wall"))
            {
                TurnAround();
                return;
            }
        }
    }

    private void TurnAround()
    {
        isMoving = false;
        // Вращение на 180 градусов в произвольную сторону
        float rotationAngle = Random.Range(0, 2) == 0 ? Random.Range(110f,250f) : Random.Range(-110f, -250f);
        transform.Rotate(Vector3.up, rotationAngle);
        isMoving = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube") && !destroyed)
        {
            Cube cube = other.GetComponent<Cube>();
            if (cube != null && cube.team != team)
            {
                capturedCubes++;
                //cube.team = team;
                //cube.SetTeamColor();
                //CubeManager.Instance.IncreaseTeamCount(team);

                if (capturedCubes >= cubesToCapture)
                {
                    TurnAround();
                    capturedCubes = 0;
                    cubesToCapture = Random.Range(minCubesToCapture, maxCubesToCapture + 1);
                }
            }
        }
    }
}

