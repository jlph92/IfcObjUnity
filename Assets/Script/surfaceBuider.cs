using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class surfaceBuider : MonoBehaviour
{
    public GameObject plane;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                // Find the line from the gun to the point that was clicked.
                Vector3 position = hit.point;
                Vector3 normal = hit.normal;
                

                plane.transform.position = position;
                plane.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            }
        }
    }
}
