using UnityEngine;

public class DimLocalDataController : DimController
{
    GameObject ctrlGameObject;
    string filePath;

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
            case DimNotification.LoadIFCFile:
                LoadIfcFile(parameters[0] as System.String);
                break;

            case DimNotification.LoadObjFile:
                LoadObjFile(parameters[0] as System.String);
                break;
            case DimNotification.FinishLoadObjFile:
                finishLoad(parameters[0] as OBJDataVisualization);
                break;
            case DimNotification.CreateIfcTree:
                createIfcTree();
                break;
        }
    }

    private void LoadIfcFile(string filePath)
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

        this.filePath = filePath;
        UnityEngine.Debug.LogFormat(outputFile_obj);

        if (System.IO.File.Exists(outputFile_obj))
            this.app.Notify(controller: this, message: DimNotification.LoadObjFile, parameters: outputFile_obj);
        else
        {
            UnityEngine.Debug.Log("Converting Obj file fails!");
        }
    }

    private void LoadObjFile(string filePath)
    {
        DataExternalDocument dataExternalDocument = ExternalDocumentLoaderFactory.Create(app, filePath) as OBJDocumentData;
        OBJDataVisualization _OBJDataVisualization = DimView.Create(ctrlGameObject, "OBJDataVisualization", app, this) as OBJDataVisualization;
        _OBJDataVisualization.GenerateGameObject(dataExternalDocument);
    }

    private void finishLoad(OBJDataVisualization _OBJDataVisualization)
    {
        UnityEngine.Debug.Log("Obj file adjust view.");
        _OBJDataVisualization.adjustView();

        if (view != null) (view as DimLocalDataVisualization).completeLoad();
        else UnityEngine.Debug.Log("View is null.");

        this.app.Notify(controller: this, message: DimNotification.CreateIfcTree);
    }

    private void createIfcTree()
    {
        DimView.Create(ctrlGameObject, "IFCTreeVisualization", app, new IFCProvider(app, this.filePath));
    }
}
