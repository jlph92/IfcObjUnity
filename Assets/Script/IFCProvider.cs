using System.Collections.Generic;
using UnityEngine;
using Xbim.Common;
using Xbim.Ifc.ViewModels;

public class IFCProvider : DimController
{
    GameObject IFCGameObject = new GameObject("Ifc_Interface");


    /// <summary>
    /// View controlled by this controller
    /// </summary>
    #region View Elements
    // View for Ifc Tree
    DimView TreeView;
    DimView ifcPropertyView;

    // View for Damage Tree
    DimView damageView;
    DimView damagePropertyView;

    // View for the local element
    DimView localView;

    bool is_ViewSet
    {
        get
        {
            bool condition = (TreeView != null && ifcPropertyView != null && damageView != null && damagePropertyView != null && localView != null);
            return condition;
        }
    }

    void checkView()
    {
        if (is_ViewSet)
        {
            this.app.Notify(controller: this, message: DimNotification.TreeSet, parameters: null);
        } 
    }
    #endregion

    string filePath;

    // Two vital models in the interface
    IfcModel ifcModel;
    DamageViewModel damageViewModel;

    // Storing GameObject visualItems
    GameObject visualParent;
    List<GameObject> visualItems = new List<GameObject>();

    public IFCProvider(CoreApplication app, string filePath, DimView localView) : base(app)
    {
        this.filePath = filePath;
        this.localView = localView;

        DimView.Create(IFCGameObject, "IFCTreeVisualization", app, this);
        DimView.Create(IFCGameObject, "IfcPropertyVisualization", app, this);

        DimView.Create(IFCGameObject, "DamageTreeVisualization", app, this);
        DimView.Create(IFCGameObject, "DamagePropertyVisualization", app, this);

        this.app.Notify(controller: this, message: DimNotification.LoadIFCFile, parameters: null);
    }

    public void setTreeView(DimView TreeView)
    {
        UnityEngine.Debug.Log("Tree View is set.");
        this.TreeView = TreeView;
        checkView();
    }

    public void setPropertyView(DimView propertyView)
    {
        this.ifcPropertyView = propertyView;
        checkView();
    }

    public void setDamageView(DimView damageView)
    {
        this.damageView = damageView;
        checkView();
    }

    public void setDamagePropertyView(DimView propertyView)
    {
        this.damagePropertyView = propertyView;
        checkView();
    }

    public override void notify(string message, params object[] parameters)
    {
        UnityEngine.Debug.Log(message);

        switch (message)
        {
            case DimNotification.LoadIFCFile:
                LoadIfcFile();
                break;

            case DimNotification.IfcLoaded:
                updateIfcTreeView(parameters[0] as IfcModel);
                break;

            case DimNotification.DamageLoaded:
                updateDamageTreeView(parameters[0] as DamageViewModel);
                break;

            case DimNotification.TreeSet:
                LoadIfcData();
                break;

            case DimNotification.DamageTreeSet:
                LoadDamageData();
                break;

            case DimNotification.LoadObjFile:
                LoadObjFile(parameters[0] as System.String);
                break;

            case DimNotification.FinishLoadIFC2Obj:
                finishLoadIfc2Obj(parameters[0] as OBJDataVisualization);
                break;

            case DimNotification.LoadIFCProperty:
                loadIfcProperties(parameters[0] as IfcModel);
                break;

            case DimNotification.LoadDamageProperty:
                loadDamageProperties(parameters[0] as DamageModel);
                break;

            case DimNotification.ShowAnnotateButton:
                showAnnotateButton();
                break;

            case DimNotification.HideAnnotateButton:
                hideAnnotateButton();
                break;

            case DimNotification.ShowEditButton:
                showEditButton();
                break;

            case DimNotification.HideEditButton:
                hideEditButton();
                break;

            case DimNotification.AddDim:
                manualAddDamage(parameters[0] as IfcModel);
                break;

            case DimNotification.EditDim:
                EditDamage(parameters[0] as DamageModel);
                break;

            case DimNotification.FreezeScreen:
                this.app.PauseViewControl();
                break;

            case DimNotification.OverwriteIFCFile:
                writeIfc();
                break;
        }
    }

    private void LoadIfcFile()
    {
        // Convert Ifc into Obj file
        string outputFile_obj = extractIFC3DModel(filePath);

        // Load Obj file
        if (System.IO.File.Exists(outputFile_obj))
            this.app.Notify(controller: this, message: DimNotification.LoadObjFile, parameters: outputFile_obj);
        else
        {
            UnityEngine.Debug.Log("Converting Obj file fails!");
        }
    }

    private string extractIFC3DModel(string filePath)
    {
        string file_directory = System.IO.Path.GetDirectoryName(filePath) + @"\Output\";
        string OutputFileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        string curentDir = System.IO.Directory.GetCurrentDirectory();

        if (!System.IO.Directory.Exists(file_directory)) System.IO.Directory.CreateDirectory(file_directory);

        startInfo.WorkingDirectory = curentDir;
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = false;
        startInfo.FileName = "cmd.exe";
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        string outputFile_obj = file_directory + OutputFileName + ".obj";
        string argument = System.String.Format("/c IfcConvert \"{0}\" \"{1}\" --use-element-numeric-ids", filePath, outputFile_obj);
        startInfo.Arguments = argument;

        try
        {
            using (System.Diagnostics.Process exeProcess = System.Diagnostics.Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }
        catch
        {
            UnityEngine.Debug.Log("Converting Error!");
        }

        return outputFile_obj;
    }

    private void LoadObjFile(string filePath)
    {
        DataExternalDocument dataExternalDocument = ExternalDocumentLoaderFactory.Create(app, filePath) as OBJDocumentData;
        OBJDataVisualization _OBJDataVisualization = DimView.Create(IFCGameObject, "OBJDataVisualization", app, this) as OBJDataVisualization;
        _OBJDataVisualization.is_IFC_Model = true;

        visualParent = _OBJDataVisualization.GenerateGameObject(dataExternalDocument);
    }

    private void storeVisualItems()
    {
        //Debug.LogFormat("GameObject visual: {0}, Child count: {1}", visualParent.name, visualParent.transform.childCount);
        foreach (Transform child in visualParent.transform)
        {
            LazyLoad(child.gameObject);
        }

        // Attach in visual items to Ifc Model
        IfcModel.attachGeometry(this.ifcModel, visualItems);
    }

    private void LazyLoad(GameObject visualItem)
    {
        if (visualItem.transform.childCount > 0)
        {
            foreach (Transform child in visualItem.transform)
            {
                // Debug.Log(child.gameObject.name);
                visualItems.Add(child.gameObject);
                LazyLoad(child.gameObject);
            }
        }
        else
        {
            return;
        }
    }

    // Adjust view of Loaded Obj
    private void finishLoadIfc2Obj(OBJDataVisualization _OBJDataVisualization)
    {
        UnityEngine.Debug.Log("Obj file adjust view.");
        _OBJDataVisualization.adjustView();
        hideLoadButton();
        showCommitButton();

        storeVisualItems();
    }

    // Reading IfcData
    void LoadIfcData()
    {
        UnityEngine.Debug.Log("Create Ifc Data Entity");
        IFCDataEntity _IFCDataEntity = new IFCDataEntity(app, this, filePath);
    }

    // Reading DamageData
    void LoadDamageData()
    {
        UnityEngine.Debug.Log("Create Damage Data Entity");
        DamageDataEntity _DamageDataEntity = new DamageDataEntity(app, this, filePath, this.ifcModel);
    }

    // Update information in Tree View
    void updateIfcTreeView(IfcModel ifcItem)
    {
        this.ifcModel = ifcItem;
        UnityEngine.Debug.Log("Update Ifc Tree!");

        // Update Ifc Tree with Ifc Model
        (TreeView as IFCTreeVisualization).insertIfcData2Tree(ifcItem);

        this.app.Notify(controller: this, message: DimNotification.DamageTreeSet, parameters: null);
    }

    // Update information in Tree View
    void updateDamageTreeView(DamageViewModel damageViewModel)
    {
        this.damageViewModel = damageViewModel;
        UnityEngine.Debug.Log("Update Damage Tree!");

        (damageView as DamageTreeVisualization).insertIfcData2Tree(damageViewModel);

        AddDamageLabel(damageViewModel);
    }

    void AddDamageLabel(DamageViewModel damageViewModel)
    {
        var _DamageModels = damageViewModel.DamageModels;

        foreach (var _DamageModel in _DamageModels)
        {
            LazyLoadModel(_DamageModel);
        }
    }

    void LazyLoadModel(DamageModel parentModel)
    {
        if (parentModel is DamageInstance)
        {
            if (parentModel.DefectLabelObject == null)
            {
                parentModel.DefectLabelObject = this.app.CreateObject(Resources.Load<GameObject>("Prefabs/ProxyLabel"), GameObject.Find("UI").transform);
            }   
        }

        foreach (var _DamageModel in parentModel.Children)
        {
            LazyLoadModel(_DamageModel);
        }
    }

    void loadIfcProperties(IfcModel ifcItem)
    {
        (ifcPropertyView as IfcPropertyVisualization).writeProperties(ifcItem);
    }

    void loadDamageProperties(DamageModel damageItem)
    {
        (damagePropertyView as DamagePropertyVisualization).writeProperties(damageItem);
    }

    // Add Damage manually 
    void manualAddDamage(IfcModel ifcModel)
    {
        this.app.Notify(controller: this, message: DimNotification.FreezeScreen, parameters: null);
        GUIHandler guiHandler = new GUIHandler(app, ifcModel);
        DimView.InsertGUI(app.UI_item, app, guiHandler);
        guiHandler.OnDimBuilt += InvokeDamge;
    }

    // Edit Damage manually 
    void EditDamage(DamageModel damageModel)
    {
        this.app.Notify(controller: this, message: DimNotification.FreezeScreen, parameters: null);
        GUIHandler guiHandler = new GUIHandler(app, damageModel);
        DimView.InsertGUI(app.UI_item, app, guiHandler);
        guiHandler.OnDimBuilt += InvokeDamge;
    }

    void writeIfc()
    {
        if(damageViewModel != null) damageViewModel.writeIfcFIle();
    }

    void InvokeDamge(DamageModel _DamageInstance)
    {
        this.damageViewModel.AddItem(_DamageInstance);
        updateDamageTreeView(this.damageViewModel);
    }

    void showAnnotateButton()
    {
        (localView as DimLocalDataVisualization).ActiveAnnotate();
    }

    void hideAnnotateButton()
    {
        (localView as DimLocalDataVisualization).deActiveAnnotate();
    }

    void showEditButton()
    {
        (localView as DimLocalDataVisualization).ActiveEdit();
    }

    void hideEditButton()
    {
        (localView as DimLocalDataVisualization).deActiveEdit();
    }

    void hideLoadButton()
    {
        (localView as DimLocalDataVisualization).deActiveLoad();
    }

    void showCommitButton()
    {
        (localView as DimLocalDataVisualization).ActiveCommit();
    }

    void hideCommitButton()
    {
        (localView as DimLocalDataVisualization).deActiveCommit();
    }
}
