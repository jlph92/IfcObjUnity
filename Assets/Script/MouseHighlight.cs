using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHighlight : MonoBehaviour
{
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    public Material original;
    public Material highlighted;

    // Start is called before the first frame update
    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponent<MeshRenderer>();

        original = m_Renderer.material;
        if (Resources.Load<Material>("Materials/Highlighted") == null) 
            Debug.Log("No Path found");
        else
            highlighted = Resources.Load<Material>("Materials/Highlighted");

    }

    // The mesh goes red when the mouse is over it...
    void OnMouseEnter()
    {
        Debug.Log("Entered");
        m_Renderer.material = highlighted;
    }

    // ...the red fades out to cyan as the mouse is held over...
    void OnMouseOver()
    {
        Debug.Log("Overed");
        m_Renderer.material.color -= new Color(0.1F, 0, 0) * Time.deltaTime;
    }

    // ...and the mesh finally turns white when the mouse moves away.
    void OnMouseExit()
    {
        Debug.Log("Exited");
        m_Renderer.material = original;
    }
}
