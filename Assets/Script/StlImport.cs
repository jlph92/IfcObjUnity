using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;
using System.IO;
using SFB;

public class StlImport : MonoBehaviour
{
    public string filePath;
    public float alpha = 0.5f;
    private Color default_color;

    [EasyButtons.Button]
    private void importSTL()
    {
        var extensions = new[] {
            new ExtensionFilter("STL files", "stl"),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open Settings File", "", extensions, false);
        filePath = path[0];
        if (filePath.Length != 0)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = Path.GetFileNameWithoutExtension(filePath);
            default_color = root.GetComponent<MeshRenderer>().material.color;
            root.GetComponent<MeshRenderer>().material.color = Color.red;
            if (root.GetComponent<BoxCollider>()) Destroy(root.GetComponent<BoxCollider>());
            root.AddComponent<MeshCollider>();

            MeshFilter m = root.GetComponent<MeshFilter>();

            Mesh[] mesh = Importer.Import(filePath);

            m.mesh = mesh[0];

            root.transform.Rotate(0.0f, -90.0f, 90.0f, Space.Self);
            root.transform.localScale = new Vector3(1.0f, -1.0f, 1.0f);
            root.layer = 9;

            cloneForShow(root);
            root.AddComponent<MouseHighlight>();
        }
        
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

    void OnGUI()
    {
        if (GUI.Button(new Rect(120, 10, 150, 30), "Load damage file"))
        {
            importSTL();
        }
    }
}