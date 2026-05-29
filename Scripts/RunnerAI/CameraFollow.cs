using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float xOffset = 5f;

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x + xOffset, transform.position.y, transform.position.z);
        }
    }
}
