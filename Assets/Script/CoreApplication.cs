using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreApplication : MonoBehaviour
{
    public IDimModel model;
    public IDimView view;
    public IDimController controller;

    void Start()
    {
        DimView.Create(gameObject, "DimLocalDataVisualization", this, new DimLocalDataController(this, gameObject));
    }

    public void Notify(DimController controller, string message, params object[] parameters)
    {
        Debug.LogFormat("Controller {0} notify.", controller.ControllerID);
        controller.notify(message, parameters);
    }
}
