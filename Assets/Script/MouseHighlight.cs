using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHighlight : MonoBehaviour
{
    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    private Material original;
    private Material highlighted;
    //private List<Material> m_list = new List<Material>();

    public bool onHighlight = false;
    private System.Action HighlightChanged;

    public bool highlight
    {
        get => onHighlight;

        set
        {
            onHighlight = value;
            OnHighlightChanged();
        }
    }

    protected virtual void OnHighlightChanged() => HighlightChanged?.Invoke();

    // Start is called before the first frame update
    void Start()
    {
        HighlightChanged += () => { Select(); };
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
        highlight = true;
    }

    // ...the red fades out to cyan as the mouse is held over...
    void OnMouseOver()
    {
        highlight = true;
    }

    // ...and the mesh finally turns white when the mouse moves away.
    void OnMouseExit()
    {
        highlight = false;
    }

    public void Select()
    {
        if (highlight) m_Renderer.material = highlighted;
        else m_Renderer.material = original;
    }
}
