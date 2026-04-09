using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 90f;
    public static readonly string moveAxis = "Vertical";
    public static readonly string turnAxis = "Horizontal";

    private Vector3 moveInput;
    void Start()
    {

    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        moveInput = new Vector3(x, 0, z).normalized;
        transform.position += transform.TransformDirection(moveInput) * moveSpeed * Time.deltaTime; // 로컬 방향으로 이동

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -rotateSpeed * Time.deltaTime, 0);

        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0); ;
        }
    }
}
