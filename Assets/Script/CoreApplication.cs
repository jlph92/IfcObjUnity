using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreApplication : MonoBehaviour
{
    public IDimModel model;
    public IDimView view;
    public IDimController controller;

    public GameObject UI_item;

    private CameraControl _cameraControl;

    void Start()
    {
        _cameraControl = GetComponent<CameraControl>();
        DimView.Create(gameObject, "DimLocalDataVisualization", this, new DimLocalDataController(this, gameObject));
    }

    public void Notify(DimController controller, string message, params object[] parameters)
    {
        Debug.LogFormat("Controller {0} notify.", controller.ControllerID);
        controller.notify(message, parameters);
    }

    public void PauseViewControl()
    {
        if (_cameraControl != null) _cameraControl.enabled = false;
    }

    public void ResumeViewControl()
    {
        if (_cameraControl != null) _cameraControl.enabled = true;
    }

    public void EndProcess(GameObject gmobj)
    {
        Debug.LogFormat("GameObject {0} is destroyed.", gmobj.name);
        Destroy(gmobj);
    }

    public void DestroyGUIView()
    {
        var _DimView  = UI_item.GetComponent<DamageGUI>();
        Destroy(_DimView);
    }

    public GameObject CreateObject(GameObject gmObj, Transform parentTransform)
    {
        return Instantiate(gmObj, parentTransform);
    }
}
