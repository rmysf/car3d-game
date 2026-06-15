using UnityEngine;

public class MobileCameraLook : MonoBehaviour
{
    public Transform target;

    public float distance = 5f;
    public float sensitivity = 0.2f;

    private float yaw = 0f;
    private float pitch = 20f;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.position.x > Screen.width * 0.3f)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    yaw += touch.deltaPosition.x * sensitivity;
                    pitch -= touch.deltaPosition.y * sensitivity;

                    pitch = Mathf.Clamp(pitch, 10f, 80f);
                }
            }
        }
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 position = target.position - (rotation * Vector3.forward * distance);

        transform.position = position;

        transform.LookAt(target);
    }
}