using UnityEngine;
using AsImpL;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using SFB;
using UnityEngine.Events;


public class IfcOpenShellParser : MonoBehaviour
{
    public string filePath;
    public Material Edge;
    public float alpha = 0.5f;
    
    private GameObject loadedOBJ;
    private UnityEvent finish_loadEvent = new UnityEvent();

    private void checkBound(GameObject model)
    {
        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        FindObjectOfType<CameraControl>().setModel(bounds);
    }

    #region Object
    /// <summary>
    /// Load .obj file
    /// </summary>
    private void LoadOBJ(string file)
    {
        //var StlFile = Path.ChangeExtension(file, ".mtl");
        MultiObjectImporter obj = FindObjectOfType<MultiObjectImporter>();
        string filename = Path.GetFileNameWithoutExtension(file);

        obj.objectsList.Add(new ModelImportInfo(filename, file));
        obj.loadObj();

        FindObjectOfType<ObjectImporterUI>().addLoadEvent(finish_loadEvent);
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
        root.layer = 8;
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
        cloneForShow(root);
        addMouseHighlight(root);

        //root.AddComponent<SelectHandler>();
        openIfcfile();
    }

    private void openIfcfile()
    {
        GetComponent<IFCTreeView>().openFile(filePath);
    }

    // Add in Highlighting feature
    private void addMouseHighlight(GameObject root)
    {
        foreach (MeshRenderer m_render in root.GetComponentsInChildren<MeshRenderer>())
        {
            m_render.gameObject.AddComponent<MouseHighlight>();
        }
    }

    private void cloneForShow(GameObject root)
    {
        GameObject duplicate = Instantiate(root);
        duplicate.layer = 9;

        foreach (Transform child in duplicate.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = 9;  // change to the Damage layer. 
        }

        foreach (Renderer m_renderer in duplicate.GetComponentsInChildren<MeshRenderer>())
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

    private void AddElements(XmlNode node, GameObject parent)
    {
        if (node.Attributes.GetNamedItem("id") != null)
        {
            string searchPath = node.Attributes.GetNamedItem("id").Value;
            GameObject goElement = null;
            goElement = GameObject.Find(searchPath);
            if (goElement != null) 
            {
                goElement.AddComponent<MeshCollider>();
            }

            // What if we can't find any? We need to create
            // a new empty object
            if (goElement == null)
                goElement = new GameObject();

            if (goElement != null)
            {
                goElement.layer = 8;
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

    private string gameTag(GameObject g)
    {
        IFCData ifcData = g.GetComponent(typeof(IFCData)) as IFCData;
        return ifcData.IFCClass;
    }

    private GameObject[] getBuilding()
    {
        List<GameObject> elements = new List<GameObject>();
        IFCData[] allObjects = FindObjectsOfType<IFCData>();
        foreach (IFCData go in allObjects)
        {
            if (go.IFCClass == "IfcBuildingStorey")
            {
                //UnityEngine.Debug.Log(go.attachedObj.name);
                elements.Add(go.attachedObj);
            }
        }

        return elements.ToArray();
    }

    private void GroupElements()
    {
        GameObject[] BuildingStoreys = getBuilding();
        Dictionary<GameObject, Transform> GroupParents = new Dictionary<GameObject, Transform>();
        Dictionary<GameObject, Transform> GroupChildren = new Dictionary<GameObject, Transform>();

        foreach (GameObject BuildingStorey in BuildingStoreys)
        {
            Dictionary<string, GameObject> Groups = new Dictionary<string, GameObject>();

            foreach (Transform child in BuildingStorey.transform)
            {
                if (gameTag(child.gameObject).Length > 0)
                {
                    if (!Groups.ContainsKey(gameTag(child.gameObject)))
                    {
                        Groups.Add(gameTag(child.gameObject), new GameObject(gameTag(child.gameObject)));
                        GroupParents.Add(Groups[gameTag(child.gameObject)], BuildingStorey.transform);
                    }

                    GroupChildren.Add(child.gameObject, Groups[gameTag(child.gameObject)].transform);
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
        var extensions = new [] { 
            new ExtensionFilter("IFC files", "ifc"),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open Settings File", "", extensions, false);
        filePath = path[0];
        //filePath = EditorUtility.OpenFilePanel("Open with ifc", "", "ifc");
        if (filePath.Length != 0)
        {
            string file_directory = Path.GetDirectoryName(filePath) + @"\Output\";
            string OutputFileName = Path.GetFileNameWithoutExtension(filePath);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string curentDir = Directory.GetCurrentDirectory();

            if (!Directory.Exists(file_directory)) Directory.CreateDirectory(file_directory);

            startInfo.WorkingDirectory = curentDir;
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

            //string outputFile_xml = file_directory + OutputFileName + ".xml";
            //UnityEngine.Debug.Log(outputFile_xml);
            //startInfo.Arguments = "/c IfcConvert " + '"' + filePath + '"' + " " + '"' + outputFile_xml + '"' + " --use-element-guids";

            //try
            //{
            //    using (Process exeProcess = Process.Start(startInfo))
            //    {
            //        exeProcess.WaitForExit();
            //    }
            //}
            //catch
            //{
            //    UnityEngine.Debug.Log("Converting Error!");
            //}

            if (File.Exists(outputFile_obj)) LoadOBJ(outputFile_obj);
            else
            {
                UnityEngine.Debug.Log("Converting Obj file fails!");
                ObjFail = true;
            }
            openIfcfile();
            //if (File.Exists(outputFile_xml))
            //{
            //    finish_loadEvent.AddListener(() => LoadXML(outputFile_xml));
            //}
            //else
            //{
            //    UnityEngine.Debug.Log("Converting xml file fails!");
            //    XmlFail = true;
            //}
        }
    }
    #endregion

    #region LoadXMLFile
    /// <summary>
    /// Open files
    /// </summary>
    [EasyButtons.Button]
    private void openXMLFile()
    {
        var extensions = new[] {
            new ExtensionFilter("XML files", "xml"),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open Settings File", "", extensions, false);
        string xmlFilePath = path[0];
        //filePath = EditorUtility.OpenFilePanel("Open with ifc", "", "ifc");
        if (xmlFilePath.Length != 0)
        {
            if (File.Exists(xmlFilePath))
            {
                LoadXML(xmlFilePath);
            }
            else
            {
                UnityEngine.Debug.Log("Loading xml file fails!");
            }
        }
    }
    #endregion

    bool ObjFail = false;

    #region GUI button
    /// <summary>
    /// Create Button on GUI
    /// </summary>
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Load ifc file"))
        {
            ObjFail = false;
            openFile();
        }

        GUI.backgroundColor = Color.red;
        if (GUI.Button(new Rect(Screen.width - 40, 10, 30, 30), "X"))
        {
            Application.Quit();
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;

        if (ObjFail) GUI.Label(new Rect(10, 40, 100, 20), "Converting Obj file fails!", style);
        //if (XmlFail) GUI.Label(new Rect(10, 60, 100, 20), "Converting Xml file fails!", style);

        //IFCData[] allObjects = FindObjectsOfType<IFCData>();
        //String g = String.Format("GameObject Loaded : {0}.", allObjects.Length);
        //GUI.Label(new Rect(10, 50, 100, 20), g, style);
        
    }
    #endregion
}
