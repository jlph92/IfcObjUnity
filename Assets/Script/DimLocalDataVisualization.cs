using UnityEngine;
using UnityEngine.UI;
using SFB;

public class DimLocalDataVisualization : DimView, ILocalDataVisualization
{
    private GameObject LoadButton;
    private GameObject AnnotateButton;
    private GameObject EditButton;

    public DimLocalDataVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {
        
    }

    void Awake()
    {
        AnnotateButton = GameObject.Find("/UI/Annotate");
        LoadButton = GameObject.Find("/UI/LoadIFC");
        EditButton = GameObject.Find("/UI/Edit");

        LoadButton.SetActive(true);
        AnnotateButton.SetActive(false);
        EditButton.SetActive(false);
    }

    void Start()
    {
        (controller as DimLocalDataController).setView(this);

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
                this.app.Notify(controller: controller, message: DimNotification.CreateIFCInterface, parameters: filePath);
            }
        }
    }

    public void deActiveLoad()
    {
        Debug.Log("Kill Load switch.");
        LoadButton.SetActive(false);
    }

    public void ActiveAnnotate()
    {
        AnnotateButton.SetActive(true);
    }

    public void deActiveAnnotate()
    {
        AnnotateButton.SetActive(false);
    }

    public void ActiveEdit()
    {
        EditButton.SetActive(true);
    }

    public void deActiveEdit()
    {
        EditButton.SetActive(false);
    }

    public void GenerateGameObject()
    {

    }
}
