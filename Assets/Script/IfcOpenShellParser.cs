using UnityEngine;
using Dummiesman;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Collections.Generic;


public class IfcOpenShellParser : MonoBehaviour
{
    public string filePath;
    private GameObject loadedOBJ;

    private void checkBound(GameObject model)
    {
        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        GetComponentInChildren<CameraControl>().setModel(bounds);
    }

    #region Object
    /// <summary>
    /// Load .obj file
    /// </summary>
    private void LoadOBJ(string file)
    {
        OBJLoader obj = new OBJLoader();
        loadedOBJ = obj.Load(file);
        if (loadedOBJ != null)
        {
            // turn -90 on the X-Axis (CAD/BIM uses Z up)
            loadedOBJ.transform.Rotate(-90, 0, 0);
        }
    }
    #endregion

    #region XML
    /// <summary>
    /// Load XML files
    /// </summary>
    private void LoadXML(string file)
    {
        var loadedXML = new XmlDocument();
        loadedXML.Load(file);

        // basepath
        string basePath = @"//ifc/decomposition";
        GameObject root = new GameObject();
        root.name = Path.GetFileNameWithoutExtension(filePath) + " (IFC)";
        root.transform.SetParent(transform.parent);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        foreach (XmlNode node in loadedXML.SelectNodes(basePath + "/IfcProject"))
            AddElements(node, root);

        GameObject ToBeDeleted = GameObject.Find(Path.GetFileNameWithoutExtension(filePath));
        //UnityEngine.Debug.Log(ToBeDeleted.name + "to be deleted");

        if (Application.isEditor)
            DestroyImmediate(ToBeDeleted);
        else
            Destroy(ToBeDeleted);

        GroupElements();
        checkBound(root);
    }

    private void AddElements(XmlNode node, GameObject parent)
    {
        if (node.Attributes.GetNamedItem("id") != null)
        {
            //if (node.Attributes.GetNamedItem("Name") != null)
            //{
            //    UnityEngine.Debug.Log(string.Format("{0} => {1}",
            //                        node.Attributes.GetNamedItem("id").Value,
            //                        node.Attributes.GetNamedItem("Name").Value)
            //    );
            //}
            //else
            //{
            //    UnityEngine.Debug.Log(node.Attributes.GetNamedItem("id").Value);
            //}

            // Search an existing GameObject with this name
            // This would apply only to elements which have
            // a geometric representation and which are
            // extracted from the 3D file.
            string searchPath = Path.GetFileNameWithoutExtension(filePath) + "/" +
                node.Attributes.GetNamedItem("id").Value;
            GameObject goElement = null;
            goElement = GameObject.Find(searchPath);
            if (goElement != null) 
            {
                goElement.AddComponent<MouseHighlight>();
                goElement.AddComponent<MeshCollider>();
            }

            // What if we can't find any? We need to create
            // a new empty object
            if (goElement == null)
                goElement = new GameObject();

            if (goElement != null)
            {
                // Set name from the IFC Name field
                if (node.Attributes.GetNamedItem("Name") != null)
                {
                    goElement.name = node.Attributes.GetNamedItem("Name").Value;
                }
                else if(node.Name.Contains("Building"))
                {
                    goElement.name = "Building";
                }
                else
                {
                    goElement.name = "Unkonown";
                }
                // Link the object to the parent we received
                if (parent != null)
                    goElement.transform.SetParent(parent.transform);

                // Add properties
                IFCData.AddProperties(node, goElement);

                // Go through children (recursively)
                foreach (XmlNode child in node.ChildNodes)
                    AddElements(child, goElement);
            }
        }// end if "id" attribute

    }

    private void GroupElements()
    {
        GameObject[] BuildingStoreys = GameObject.FindGameObjectsWithTag("IfcBuildingStorey");
        Dictionary<GameObject, Transform> GroupParents = new Dictionary<GameObject, Transform>();
        Dictionary<GameObject, Transform> GroupChildren = new Dictionary<GameObject, Transform>();

        foreach (GameObject BuildingStorey in BuildingStoreys)
        {
            Dictionary<string, GameObject> Groups = new Dictionary<string, GameObject>();

            foreach (Transform child in BuildingStorey.transform)
            {
                if (child.gameObject.tag != "Untagged")
                {
                    if (!Groups.ContainsKey(child.gameObject.tag))
                    {
                        Groups.Add(child.gameObject.tag, new GameObject(child.gameObject.tag));
                        GroupParents.Add(Groups[child.gameObject.tag], BuildingStorey.transform);
                    }

                    GroupChildren.Add(child.gameObject, Groups[child.gameObject.tag].transform);
                }
            }
        }

        foreach (KeyValuePair<GameObject, Transform> group in GroupChildren)
        {
            group.Key.transform.SetParent(group.Value);
        }

        foreach (KeyValuePair<GameObject, Transform> group in GroupParents)
        {
            group.Key.transform.SetParent(group.Value);
        }
    }

    #endregion

    #region LoadFile
    /// <summary>
    /// Open files
    /// </summary>
    [EasyButtons.Button]
    private void openFile()
    {
        filePath = EditorUtility.OpenFilePanel("Open with ifc", "", "ifc");
        if (filePath.Length != 0)
        {
            string file_directory = Path.GetDirectoryName(filePath) + @"\Output\";
            string OutputFileName = Path.GetFileNameWithoutExtension(filePath);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string curentDir = Directory.GetCurrentDirectory();
            string setDir = Path.Combine(curentDir, @"Assets\IfcConvert\");

            if (!Directory.Exists(file_directory)) Directory.CreateDirectory(file_directory);

            startInfo.WorkingDirectory = setDir;
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "cmd.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            string outputFile_obj = file_directory + OutputFileName + ".obj";
            //UnityEngine.Debug.Log(outputFile_obj);
            startInfo.Arguments = "/c IfcConvert " + '"' + filePath + '"' + " " + '"' + outputFile_obj + '"' + " --use-element-guids";

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                UnityEngine.Debug.Log("Converting Error!");
            }

            string outputFile_xml = file_directory + OutputFileName + ".xml";
            //UnityEngine.Debug.Log(outputFile_xml);
            startInfo.Arguments = "/c IfcConvert " + '"' + filePath + '"' + " " + '"' + outputFile_xml + '"' + " --use-element-guids";

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                UnityEngine.Debug.Log("Converting Error!");
            }

            if (File.Exists(outputFile_obj)) LoadOBJ(outputFile_obj);
            else UnityEngine.Debug.Log("Converting Obj file fails!");

            if (File.Exists(outputFile_xml)) LoadXML(outputFile_xml);
            else UnityEngine.Debug.Log("Converting xml file fails!");
        }
    }
    #endregion

    #region GUI button
    /// <summary>
    /// Create Button on GUI
    /// </summary>
    void OnGUI()
    {
        if (loadedOBJ == null)
        {
            if (GUI.Button(new Rect(10, 10, 100, 30), "Load ifc file"))
            {
                openFile();
            }
        }
    }
    #endregion
}
