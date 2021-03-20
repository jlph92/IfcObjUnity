using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AsImpL;

public class OBJDataVisualization : DimView, IExternalDataVisualization
{
    GameObject visualItem;
    private UnityEvent finish_loadEvent;

    private const float alpha = 0.5f;

    public OBJDataVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {
        
    }

    void finishLoading()
    {
        Debug.Log("OBJ Finish Loading.");
        this.app.Notify(controller: controller, message: DimNotification.FinishLoadObjFile, parameters: this);
    }

    void Start()
    {
        GetComponent<OutlineToolkit.EdgeDetect>().enabled = false;

        if (finish_loadEvent == null)
            finish_loadEvent = new UnityEvent();

        //Call finish loading with finish load event
        finish_loadEvent.AddListener(finishLoading);

        //Create finish Loading Object event
        FindObjectOfType<ObjectImporterUI>().addLoadEvent(finish_loadEvent);
    }

    public GameObject GenerateGameObject(DataExternalDocument externalDocumentData)
    {
        string pathToFile = externalDocumentData.getReferenceFile();
        visualItem = new GameObject(System.Guid.NewGuid().ToString("D"));

        MultiObjectImporter obj = FindObjectOfType<MultiObjectImporter>();
        if (obj != null)
        {
            obj.objectsList.Add(new ModelImportInfo(path: pathToFile));
            obj.parentTransform = visualItem.transform;
            obj.loadObj();
        }

        return visualItem;
    }

    public void adjustView()
    {
        GetComponent<OutlineToolkit.EdgeDetect>().enabled = true;
        checkBound();
        //GetComponent<IFCTreeView>().openFile(filePath);
        //GetComponent<Damage_TreeView>().openFile(filePath);
        damageView();
    }

    private void damageView()
    {
        GameObject damageModel = Instantiate(visualItem);
        damageModel.name = visualItem.name;
        damageModel.layer = 9;

        foreach (Transform child in visualItem.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = 8;  // change to the Actual layer. 
        }

        setDamageView(damageModel);
    }

    private void setDamageView(GameObject damageModel)
    {
        foreach (Transform child in damageModel.GetComponentsInChildren<Transform>())
        {
            child.name = child.name + "(Damage View)";
            child.gameObject.layer = 9;  // change to the Damage layer. 
        }

        foreach (Renderer m_renderer in damageModel.GetComponentsInChildren<MeshRenderer>())
        {
            Color color = m_renderer.material.color;
            color.a = alpha;

            Material m_material = new Material(Shader.Find("Standard"));
            m_material.SetFloat("_Mode", 2.0f);
            m_material.SetColor("_Color", color);
            m_material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_material.SetInt("_ZWrite", 0);
            m_material.DisableKeyword("_ALPHATEST_ON");
            m_material.EnableKeyword("_ALPHABLEND_ON");
            m_material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m_material.renderQueue = 3000;

            m_renderer.material = m_material;
        }
    }

    private void checkBound()
    {
        Bounds bounds = new Bounds(visualItem.transform.position, Vector3.zero);
        Renderer[] renderers = visualItem.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        FindObjectOfType<CameraControl>().setModel(bounds);
    }
}
