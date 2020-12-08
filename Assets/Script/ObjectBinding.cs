using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;
using System.Linq;

public class ObjectBinding
{
    private Dictionary<IXbimViewModel, GameObject> relatedObjects = new Dictionary<IXbimViewModel, GameObject>();
    private IfcStore model;

    public IEnumerable attachedObjects
    {
        get
        {
            return relatedObjects;
        }
    }

    public void setModel(IfcStore model)
    {
        this.model = model;
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
        var id = IfcModel.EntityLabel;
        IIfcObjectDefinition specificObject = model.Instances.FirstOrDefault<IIfcObjectDefinition>(d => d.EntityLabel == id);
        Debug.Log(specificObject.GlobalId);
        return String.Format("{0}",specificObject.GlobalId);
    }
}
