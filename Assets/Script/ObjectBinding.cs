using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;
using System.Linq;

public class ObjectBinding : DimModel
{
    private Dictionary<IXbimViewModel, GameObject> relatedObjects = new Dictionary<IXbimViewModel, GameObject>();

    public IEnumerable attachedObjects
    {
        get
        {
            return relatedObjects;
        }
    }

    public GameObject GetValue(IXbimViewModel xm)
    {
        try
        {
            GameObject go = relatedObjects[xm];
            return go;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public IXbimViewModel GetView(GameObject go)
    {
        IXbimViewModel xm;
        try
        {
            xm = relatedObjects.FirstOrDefault(x => x.Value == go).Key;
            return xm;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool select(IXbimViewModel xm)
    {
        //Debug.Log(String.Format("{0}: {1}", xm.EntityLabel, xm.Name));
        var go = GetValue(xm);

        if (go != null)
        {
            go.GetComponent<MouseHighlight>().highlight = true;
            return true;
        }
        else return false; 
    }

    public void damageSelect(IXbimViewModel xm)
    {
        //Debug.Log(String.Format("{0}: {1}", xm.EntityLabel, xm.Name));
        var go = GetValue(xm);

        if (go != null)
        {
            //go.GetComponent<MouseHighlight>().damage = true;
            var go_duplicate = GameObject.Find(go.name + "(Duplicate)");
            go_duplicate.GetComponent<MouseHighlight>().damage = true;
            //Debug.Log(String.Format("{0}: {1}", go.name, "Set Damage"));
        } 
        else return;
    }

    public void unselect(IXbimViewModel xm)
    {
        //Debug.Log(String.Format("{0}: {1}", xm.EntityLabel, xm.Name));
        var go = GetValue(xm);

        if (go != null) go.GetComponent<MouseHighlight>().highlight = false;
        else return;
    }

    public void Register(IXbimViewModel IfcModel)
    {
        GameObject goElement = null;
        goElement = GameObject.Find(checkGuid(IfcModel));
        if (goElement != null)
        {
            goElement.AddComponent<MeshCollider>();
            goElement.layer = 8;
            goElement.AddComponent<MouseHighlight>();
            relatedObjects.Add(IfcModel, goElement);
        }
    }

    private string checkGuid(IXbimViewModel IfcModel)
    {
        return String.Format("id-{0}", IfcModel.EntityLabel);
    }
}
