using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIHandler : DimController
{
    private IfcModel ifcModel;
    private DamageModel damageModel;

    public event DimBuilt OnDimBuilt;
    public delegate void DimBuilt (DamageModel _DamageInstance);

    public GUIHandler(CoreApplication app, IfcModel ifcModel) : base(app)
    {
        this.ifcModel = ifcModel;
    }

    public GUIHandler(CoreApplication app, DamageModel damageModel) : base(app)
    {
        this.damageModel = damageModel;
    }

    public void setView(DimView UIView)
    {
        UnityEngine.Debug.Log("GUI View is set.");
        this.view = UIView;
        if (this.ifcModel != null) (this.view as DamageGUI).initialise(this.ifcModel);
        if (this.damageModel != null) (this.view as DamageGUI).initialise(this.damageModel);
    }

    public override void notify(string message, params object[] parameters)
    {
        UnityEngine.Debug.Log(message);

        switch (message)
        {
            case DimNotification.AbortDim:
                returnViewer(parameters[0] as GameObject);
                break;

            case DimNotification.UnFreezeScreen:
                this.app.ResumeViewControl();
                break;

            case DimNotification.LoadStlFile:
                LoadSTLFile(parameters[0] as string);
                break;

            case DimNotification.FinishEditDim:
                finishEditDIM(parameters[0] as DamageModel);
                break;
        }
    }
    
    void LoadSTLFile(string filepath)
    {
        GameObject ImportObject = new GameObject(System.Guid.NewGuid().ToString("D"));
        DataExternalDocument dataExternalDocument = ExternalDocumentLoaderFactory.Create(app, filepath) as STLDocumentData;
        STLDataVisualization _STLDataVisualization = DimView.Create(ImportObject, "STLDataVisualization", app, this) as STLDataVisualization;

        GameObject visualParent = _STLDataVisualization.GenerateGameObject(dataExternalDocument);

        (this.view as DamageGUI).FeedIn3DImage(visualParent);

        this.app.EndProcess(ImportObject);
    }

    void finishEditDIM(DamageModel _DamageInstance)
    {
        OnDimBuilt(_DamageInstance);
        this.app.DestroyGUIView();
        this.app.Notify(controller: this, message: DimNotification.UnFreezeScreen, parameters: null);
    }

    void returnViewer(GameObject _DamageGUI)
    {
        this.app.EndProcess(_DamageGUI);
        this.app.Notify(controller: this, message: DimNotification.UnFreezeScreen, parameters: null);
    }
}
