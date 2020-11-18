using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Parabox.Stl;
using System.IO;

public class StlImport : MonoBehaviour
{
    public string filePath;

    [EasyButtons.Button]
    private void importSTL()
    {
        filePath = EditorUtility.OpenFilePanel("Open with stl", "", "stl");
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
