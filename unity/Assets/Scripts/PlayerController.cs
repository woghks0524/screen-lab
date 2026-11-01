using UnityEngine;

// 공(플레이어): 방향키·WASD 또는 화면을 누른 쪽으로 힘을 줘서 굴려요. 스페이스는 점프.
public class PlayerController : MonoBehaviour
{
    public float force = 14f;
    public float jump = 5.5f;
    Rigidbody rb;
    Vector3 start;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        start = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (Input.GetMouseButton(0))
        {
            // 화면 가운데에서 누른 곳 쪽으로 굴러가요 (휴대폰·태블릿용)
            Vector2 d = (Vector2)Input.mousePosition - new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (d.magnitude > 30f) { d.Normalize(); dir = new Vector3(d.x, 0f, d.y); }
        }
        rb.AddForce(Vector3.ClampMagnitude(dir, 1f) * force);
        if (transform.position.y < -6f) Respawn();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Physics.Raycast(transform.position, Vector3.down, 0.6f))
            rb.AddForce(Vector3.up * jump, ForceMode.Impulse);
    }

    void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = start;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Pickup>() == null) return;
        other.gameObject.SetActive(false);
        Hud.I.Add();
    }
}
