using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;
using System.IO;
using SFB;

public class StlImport : MonoBehaviour
{
    public float alpha = 0.5f;
    //public Unit unit;
    public bool smooth = false;
    private Color default_color;
    private bool showOrigin = true;


    public void offOrigin()
    {
        showOrigin = false;
    }

    public void openSTL(string filePath, Unit unit)
    {
        GameObject root = new GameObject(Path.GetFileNameWithoutExtension(filePath));
        root.layer = 9;
        root.transform.parent = transform;
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        if (showOrigin) Instantiate(Resources.Load<GameObject>("Prefabs/Axis_Arrow"), root.transform);
        

        Mesh[] meshes = Importer.Import(filePath, smooth: smooth, unit: unit);

        foreach (Mesh mesh in meshes)
        {
            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.transform.parent = root.transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.name = "Element";
            child.layer = 9;

            //default_color = child.GetComponent<MeshRenderer>().material.color;
            child.GetComponent<MeshRenderer>().material.color = Color.red;
            setTransparent(child.GetComponent<MeshRenderer>());
            if (child.GetComponent<BoxCollider>()) Destroy(child.GetComponent<BoxCollider>());

            MeshFilter m = child.GetComponent<MeshFilter>();
            m.mesh = mesh;
            m.mesh.RecalculateNormals();
        }
    }

    private void importSTL()
    {
        Unit unit = Unit.Milimeter;

        var extensions = new[] {
            new ExtensionFilter("STL files", "stl"),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open Settings File", "", extensions, false);
        string filePath = path[0];
        if (filePath.Length != 0)
        {
            GameObject root = Instantiate(Resources.Load<GameObject>("Prefabs/Axis_Arrow"), Vector3.zero, Quaternion.identity) as GameObject;
            root.name = Path.GetFileNameWithoutExtension(filePath);

            Mesh[] meshes = Importer.Import(filePath, smooth: smooth, unit: unit);

            foreach (Mesh mesh in meshes)
            {
                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.transform.parent = root.transform;
                child.name = "Element";
                //default_color = child.GetComponent<MeshRenderer>().material.color;
                child.GetComponent<MeshRenderer>().material.color = Color.red;
                setTransparent(child.GetComponent<MeshRenderer>());
                if (child.GetComponent<BoxCollider>()) Destroy(child.GetComponent<BoxCollider>());

                MeshFilter m = child.GetComponent<MeshFilter>();
                m.mesh = mesh;
                m.mesh.RecalculateNormals();
            }

            root.layer = 9;

            //cloneForShow(root);
        }
        
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

    private void cloneForShow(GameObject root)
    {
        GameObject duplicate = Instantiate(root);
        duplicate.layer = 8;

        Renderer m_renderer = duplicate.GetComponent<MeshRenderer>();

        Color color = m_renderer.material.color;

        if (default_color != null)
        {
            color = default_color;
        }
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

    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(120, 10, 150, 30), "Load damage file"))
    //    {
    //        importSTL();
    //    }
    //}
}