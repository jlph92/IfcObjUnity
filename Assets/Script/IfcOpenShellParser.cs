using UnityEngine;
using Dummiesman;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Xml;

public class IfcOpenShellParser : MonoBehaviour
{
    public string filePath;
    private GameObject loadedOBJ;

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

    private void LoadXML(string file)
    {
        var loadedXML = new XmlDocument();
        loadedXML.Load(file);

        // basepath
        string basePath = @"//ifc/decomposition";
        GameObject root = new GameObject();
        root.name = Path.GetFileNameWithoutExtension(filePath) + " (IFC)";
        IFCData data = root.AddComponent<IFCData>() as IFCData;

        foreach (XmlNode node in loadedXML.SelectNodes(basePath + "/IfcProject"))
            data.AddElements(node, root);
    }

    void OnGUI()
    {
        if (loadedOBJ == null)
        {
            if (GUI.Button(new Rect(10, 10, 100, 30), "Load ifc file"))
            {
                filePath = EditorUtility.OpenFilePanel("Open with ifc", "", "ifc");
                if (filePath.Length != 0)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = "C:\\Users\\user\\Downloads";
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = "cmd.exe";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    var outputFile_obj = Path.ChangeExtension(filePath, ".obj");
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

                    var outputFile_xml = Path.ChangeExtension(filePath, ".xml");
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
        }
    }
}