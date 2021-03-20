using UnityEngine;
using UnityEngine.UI;
using SFB;

public class DimLocalDataVisualization : DimView, ILocalDataVisualization
{
    private GameObject LoadButton;

    public DimLocalDataVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {
        
    }

    void Start()
    {
        (controller as DimLocalDataController).setView(this);

        LoadButton = GameObject.Find("LoadIFC");
        LoadButton.SetActive(true);
        Button ifcLoadBtn = LoadButton.GetComponent<Button>();
        ifcLoadBtn.onClick.AddListener(openFile);
    }

    private void openFile()
    {
        var extensions = new[] {
            new ExtensionFilter("IFC files", "ifc"),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open Ifc File", "", extensions, false);
        if (path != null && path.Length > 0)
        {
            string filePath = path[0];

            if (filePath.Length != 0)
            {
                this.app.Notify(controller: controller, message: DimNotification.LoadIFCFile, parameters: filePath);
            }
        }
    }

    public void completeLoad()
    {
        Debug.Log("Kill Load switch.");
        LoadButton.SetActive(false);
    }

    public void GenerateGameObject()
    {

    }
}
