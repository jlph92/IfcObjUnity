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

            MeshFilter m = root.GetComponent<MeshFilter>();

            Mesh[] mesh = Importer.Import(filePath);

            m.mesh = mesh[0];
        }
    }
}
