using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        if (Camera.main != null) camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (camTransform != null)
        {
            // Move background with camera but keep it slightly behind
            // Syncing with LateUpdate (where CameraFollow usually runs) avoids jitter
            transform.position = new Vector3(camTransform.position.x, camTransform.position.y, 10);
        }
    }
}
