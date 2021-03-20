using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STLDataVisualization : DimView, IExternalDataVisualization
{
    GameObject visualItem;
    Vector3 position = Vector3.zero;
    Quaternion rotation = Quaternion.identity;

    private DataExternalDocument _externalDocumentData;
    private float alpha = 0.5f;

    public DataExternalDocument externalDocumentData
    {
        get => _externalDocumentData;
        set => _externalDocumentData = value;
    }

    public STLDataVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    public GameObject GenerateGameObject(DataExternalDocument externalDocumentData)
    {
        visualItem = createMeshes(externalDocumentData);

        Quaternion LocalMatrix = (externalDocumentData as STLDocumentData).getLocalTransformation();
        Color stlColor = (externalDocumentData as STLDocumentData).getStlColor();

        visualItem.transform.localPosition = position;
        visualItem.transform.localRotation = rotation;

        return visualItem;
    }

    private GameObject createMeshes(DataExternalDocument externalDocumentData)
    {
        GameObject root = new GameObject(System.Guid.NewGuid().ToString("D"));
        root.layer = 9;
        root.transform.parent = transform;
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        Color stlColor = (externalDocumentData as STLDocumentData).getStlColor();

        foreach (Mesh mesh in getMeshes(externalDocumentData))
        {
            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.transform.parent = root.transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.name = System.Guid.NewGuid().ToString("D");
            child.layer = 9;

            child.GetComponent<MeshRenderer>().material.color = stlColor;
            setTransparent(child.GetComponent<MeshRenderer>());
            if (child.GetComponent<BoxCollider>()) Destroy(child.GetComponent<BoxCollider>());

            MeshFilter m = child.GetComponent<MeshFilter>();
            m.mesh = mesh;
            m.mesh.RecalculateNormals();
        }

        return root;
    }

    void setTransparent(MeshRenderer m_renderer)
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

    public void updatePosition(Vector3 position)
    {
        this.position = position;
        if (visualItem != null) visualItem.transform.localPosition = position;
    }

    public void updateRotation(Quaternion rotation)
    {
        this.rotation = rotation;
        if (visualItem != null) visualItem.transform.localRotation = rotation;
    }

    public void RefreshView()
    {
        Destroy(visualItem);
        visualItem = createMeshes(externalDocumentData);
    }

    public static Mesh[] getMeshes(DataExternalDocument externalDocumentData)
    {
        string pathToFile = externalDocumentData.getReferenceFile();
        Parabox.Stl.Unit unit = (externalDocumentData as STLDocumentData).getLengthUnit();

        Mesh[] meshes = Parabox.Stl.Importer.Import(pathToFile, unit: unit);

        return meshes;
    }
}
