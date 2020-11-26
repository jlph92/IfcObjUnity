using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;
using System.IO;
using SFB;

public class StlImport : MonoBehaviour
{
    public string filePath;

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
            root.GetComponent<MeshRenderer>().material.color = Color.red;

            MeshFilter m = root.GetComponent<MeshFilter>();

            Mesh[] mesh = Importer.Import(filePath);

            m.mesh = mesh[0];

            root.transform.Rotate(0.0f, -90.0f, 90.0f, Space.Self);
            root.transform.localScale = new Vector3(1.0f, -1.0f, 1.0f);
            root.layer = 9;
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(120, 10, 150, 30), "Load damage file"))
        {
            importSTL();
        }
    }
}