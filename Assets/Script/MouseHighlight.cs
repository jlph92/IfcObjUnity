using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHighlight : MonoBehaviour
{
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    private Material original;
    public Material highlighted;
    //private List<Material> m_list = new List<Material>();

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

        //m_list.Add(original);
        //m_list.Add(highlighted);
    }

    // The mesh goes red when the mouse is over it...
    void OnMouseEnter()
    {
        //Debug.Log("Entered");
        //if (m_list.Count > 0) m_Renderer.materials = m_list.ToArray();
        m_Renderer.material = highlighted;
        //FindObjectOfType<SelectHandler>().onSelect(this.gameObject);
    }

    // ...the red fades out to cyan as the mouse is held over...
    void OnMouseOver()
    {
        //Debug.Log("Overed");
        //if (m_list.Count > 0) m_Renderer.materials = m_list.ToArray();
        m_Renderer.material.color -= new Color(0.1F, 0, 0) * Time.deltaTime;
    }

    // ...and the mesh finally turns white when the mouse moves away.
    void OnMouseExit()
    {
        //Debug.Log("Exited");
        //Material[] m_original = { original };
        m_Renderer.material = original;
    }
}
