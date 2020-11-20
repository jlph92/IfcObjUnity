using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHighlight : MonoBehaviour
{
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    // Start is called before the first frame update
    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponent<MeshRenderer>();
    }

    // The mesh goes red when the mouse is over it...
    void OnMouseEnter()
    {
        m_Renderer.material.color = Color.red;
    }

    // ...the red fades out to cyan as the mouse is held over...
    void OnMouseOver()
    {
        m_Renderer.material.color -= new Color(0.1F, 0, 0) * Time.deltaTime;
    }

    // ...and the mesh finally turns white when the mouse moves away.
    void OnMouseExit()
    {
        m_Renderer.material.color = Color.white;
    }
}
