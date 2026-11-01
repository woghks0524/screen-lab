using UnityEngine;

// 모을 별(노란 상자): 제자리에서 빙글빙글 돌아요. 닿으면 PlayerController가 꺼요.
public class Pickup : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(new Vector3(15f, 30f, 45f) * Time.deltaTime);
    }
}
