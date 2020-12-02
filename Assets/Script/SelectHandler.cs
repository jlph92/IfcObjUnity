using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectHandler : MonoBehaviour
{
    public GameObject selected;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onSelect(GameObject selected)
    {
        this.selected = selected;
        FindObjectOfType<IfcInteract>().setProduct(getGuid());
    }

    private string getGuid()
    {
        if (selected != null) return selected.GetComponent<IFCData>().STEPId;
        else return "NIL";
    }

    void OnGUI()
    {
        if (!getGuid().Equals("NIL"))
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;

            var _properties = FindObjectOfType<IfcInteract>().Properties;

            int height = 80;

            foreach (var _property in _properties)
            {
                String s = String.Format("{0}: {1}", _property.Name, _property.Value);
                GUI.Label(new Rect(10, height, 100, 20), s, style);
                height += 20;
            }
        }
    }
}
