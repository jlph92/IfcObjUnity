﻿using UnityEngine;

public interface IDimView
{
    void RefreshView();
}

public class DimView : MonoBehaviour, CoreElement, IDimView
{
    public CoreApplication app { get; set; }
    public DimController controller { get; protected set; }

    public DimView(CoreApplication app, DimController controller)
    {
        this.app = app;
        this.controller = controller;
    }

    public void RefreshView()
    {

    }

    public static DimView Create(GameObject gmObj, string componentType, CoreApplication app, DimController controller)
    {
        DimView _DimView = gmObj.AddComponent(System.Type.GetType(componentType)) as DimView;
        _DimView.app = app;
        _DimView.controller = controller;

        return _DimView;
    }

    public static DimView InsertGUI(GameObject gmObj, CoreApplication app, DimController controller)
    {
        GameObject gui_Creator = Instantiate(Resources.Load<GameObject>("Prefabs/GUI_Creator"), gmObj.transform);
        DimView _DimView = gui_Creator.GetComponent<DamageGUI>();
        _DimView.app = app;
        _DimView.controller = controller;

        return _DimView;
    }
}
