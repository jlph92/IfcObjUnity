using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCentreFocus : MonoBehaviour
{
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;

        // For Demo purpose, method focus is called in Start
        // Focus methods can be called after the Objects finish loaded into Scene
        focus();
    }

    public void focus()
    {
        // Define an Abstact Bound Volume
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        // Gather only active Object Renderer
        // Inactive means Invisible, not in consideration
        var renderers = FindObjectsOfType<Renderer>();

        // Wrap up the volume from the renderer
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        // Pass the bound to adapt camera position & direction
        CameraFocus(bounds);
    }

    void CameraFocus(Bounds bounds)
    {
        // Create Forward vector from Bound Center
        Vector3 pointOnside = bounds.center - transform.forward;

        // Extract Aspect Ratio 
        float aspect = (float)Screen.width / (float)Screen.height;

        // Define Zoom scale
        float scale = bounds.size.y * 3.0f;

        // Defien distant from camera to Object
        float maxDistance = (scale / Mathf.Tan(Mathf.Deg2Rad * (cam.fieldOfView / aspect)));

        // Assign camera position
        transform.position = Vector3.MoveTowards(pointOnside, bounds.center, -maxDistance);

        // Force Camera always look at centre of Object
        transform.LookAt(bounds.center);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
