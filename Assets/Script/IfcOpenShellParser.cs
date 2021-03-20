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
using OutlineToolkit;


public class IfcOpenShellParser : MonoBehaviour
{
    public string filePath;
    public float alpha = 0.5f;
    
    private GameObject loadedOBJ;
    private UnityEvent finish_loadEvent = new UnityEvent();

    void Start()
    {
        GetComponent<EdgeDetect>().enabled = false;
    }

    private void checkBound(GameObject model)
    {
        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            //UnityEngine.Debug.Log(System.String.Format(" X: {0} Y:{1} Z:{2}", r.bounds.size.x, r.bounds.size.y, r.bounds.size.z));
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
        if(obj != null)
        {
            obj.objectsList.Add(new ModelImportInfo(path: file));
            obj.parentTransform = new GameObject().transform;
            obj.loadObj();
        }

        FindObjectOfType<ObjectImporterUI>().addLoadEvent(finish_loadEvent);
    }
    #endregion


    private void openIfcfile()
    {
        GetComponent<EdgeDetect>().enabled = true;
        var root = GameObject.Find(Path.GetFileNameWithoutExtension(filePath));
        checkBound(root);
        GetComponent<IFCTreeView>().openFile(filePath);
        GetComponent<Damage_TreeView>().openFile(filePath);
        cloneForShow(root);
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
        duplicate.name = root.name;
        duplicate.layer = 9;

        foreach (Transform child in root.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = 8;  // change to the Actual layer. 
        }

        foreach (Transform child in duplicate.GetComponentsInChildren<Transform>())
        {
            child.name = child.name + "(Duplicate)";
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
            startInfo.Arguments = "/c IfcConvert " + '"' + filePath + '"' + " " + '"' + outputFile_obj + '"' + "  --use-element-numeric-ids";

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
            else
            {
                UnityEngine.Debug.Log("Converting Obj file fails!");
                ObjFail = true;
            }
            finish_loadEvent.AddListener(() => openIfcfile());
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
