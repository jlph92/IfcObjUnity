using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float scale = 1.0f;
    private Bounds bound;
    private bool set = false;
    private Camera cam;

    float distance = 2.0f;
    float xSpeed = 10.0f;
    float ySpeed = 10.0f;
    float yMinLimit = -90f;
    float yMaxLimit = 90f;
    float distanceMin = 10f;
    float distanceMax = 10f;
    float smoothTime = 5.0f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    float velocityX = 0.0f;
    float velocityY = 0.0f;

    float t_velocityX = 0.0f;
    float t_velocityY = 0.0f;
    float posYAxis = 0.0f;
    float posXAxis = 0.0f;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;

        Vector3 angles = transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;

    }

    public void setModel(Bounds bound)
    {
        this.bound = bound;
        //Debug.Log(System.String.Format(" X: {0} Y:{1} Z:{2}",this.bound.size.x, this.bound.size.y, this.bound.size.z));
        this.set = true;
        scale = this.bound.size.y * 3.0f;
    }

    void Update()
    {
        if (set)
        {
            CameraFocus();

            // Rotation
            if (Input.GetMouseButton(1))
            {
                velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * 0.02f;
                velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
            }
            rotationYAxis += velocityX;
            rotationXAxis -= velocityY;
            rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
            Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
            Quaternion rotation = toRotation;

            transform.rotation = rotation;
            velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
            velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);


            // Translation
            if (Input.GetMouseButton(2))
            {
                t_velocityX += xSpeed * Input.GetAxis("Mouse X") * 0.01f;
                t_velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.01f;
            }
            posXAxis -= t_velocityX;
            posYAxis -= t_velocityY;
            Vector3 toPosition = new Vector3(posXAxis, posYAxis, 0.0f);
            transform.Translate(toPosition);

            t_velocityX = Mathf.Lerp(t_velocityX, 0, Time.deltaTime * smoothTime);
            t_velocityY = Mathf.Lerp(t_velocityY, 0, Time.deltaTime * smoothTime);
        }
    }

    void CameraFocus()
    {
        Vector3 pointOnside = bound.center - transform.forward;
        float aspect = (float)Screen.width / (float)Screen.height;
        scale += Input.mouseScrollDelta.y * 0.4f;
        float maxDistance = (scale / Mathf.Tan(Mathf.Deg2Rad * (cam.fieldOfView / aspect)));
        transform.position = Vector3.MoveTowards(pointOnside, bound.center, -maxDistance);
        transform.LookAt(bound.center);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

}
