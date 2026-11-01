using UnityEngine;

// 카메라가 공 뒤쪽 위에서 부드럽게 따라가요.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 9f, -10f);

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position + offset, 6f * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}
