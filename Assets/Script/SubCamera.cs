using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCamera : MonoBehaviour
{
    public Color color;
    private Camera m_MainCamera;
    private Camera cam;

    void Start()
    {
        //This gets the Main Camera from the Scene
        m_MainCamera = Camera.main;
        cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = color;
    }

    void Update()
    {
        if (m_MainCamera.transform.hasChanged)
        {
            transform.position = m_MainCamera.transform.position;
            transform.rotation = m_MainCamera.transform.rotation;
        }
    }

}
