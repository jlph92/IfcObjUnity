using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHighlight : MonoBehaviour
{
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    private Material original;
    private Material highlighted;
    private Material Damaged;
    private IfcModel ifcModel;

    // Triger signal for selection
    private bool selected = false;
    private bool damaged = false;

    public bool highlight
    {
        get => selected;

        set
        {
            selected = value;
            Select();
        }
    }

    public bool damage
    {
        get => damaged;

        set
        {
            damaged = value;
            DamageSelect();
        }
    }

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

        if (Resources.Load<Material>("Materials/Damage") == null)
            Debug.Log("No Path found");
        else
            Damaged = Resources.Load<Material>("Materials/Damage");

        gameObject.AddComponent<MeshCollider>();
    }

    public void setIfcModel(IfcModel ifcModel)
    {
        this.ifcModel = ifcModel;
    }

    void OnMouseUpAsButton()
    {
        if (!selected) ifcModel.OnSelected(System.EventArgs.Empty);
    }

    //// ...highlight as the mouse is held over...
    //void OnMouseOver()
    //{
    //    m_Renderer.material = highlighted;
    //}

    //// ...and the mesh finally turns original when the mouse moves away.
    //void OnMouseExit()
    //{
    //    m_Renderer.material = original;
    //}


    public void Select()
    {
        if (highlight) m_Renderer.material = highlighted;
        else m_Renderer.material = original;
    }

    public void DamageSelect()
    {
        if (damage) m_Renderer.material = Damaged;
        else m_Renderer.material = original;
    }
}
