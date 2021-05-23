﻿using UnityEngine;

public class DimLocalDataController : DimController
{
    GameObject ctrlGameObject;
    string filePath;
    private IFCProvider ifcProvider;

    public DimLocalDataController(CoreApplication app, GameObject ctrlGameObject) : base(app)
    {
        UnityEngine.Debug.LogFormat("DimLocalDataController {0} created.", ControllerID);
        this.ctrlGameObject = ctrlGameObject;
    }

    public void setView(DimLocalDataVisualization UIView)
    {
        UnityEngine.Debug.Log("View is set.");
        this.view = UIView;
    }

    public override void notify(string message, params object[] parameters)
    {
        UnityEngine.Debug.Log(message);

        switch (message)
        {
            case DimNotification.CreateIFCInterface:
                CreateIfcInterface(parameters[0] as System.String);
                break;

            case DimNotification.WriteIFCFile:
                writeIfcfile();
                break;
        }
    }

    private void showAnotation()
    {
        (view as DimLocalDataVisualization).ActiveAnnotate();
    }

    private void hideAnotation()
    {
        (view as DimLocalDataVisualization).deActiveAnnotate();
    }

    void CreateIfcInterface(string filePath)
    {
        ifcProvider = new IFCProvider(app, filePath, this.view);
    }

    void writeIfcfile()
    {
        this.app.Notify(controller: ifcProvider, message: DimNotification.OverwriteIFCFile, parameters: null);
    }

}