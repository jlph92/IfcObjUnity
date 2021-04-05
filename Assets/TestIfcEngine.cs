using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IfcEngineWrapper;
using System.IO;

public class TestIfcEngine : MonoBehaviour
{
    List<IfcItem> items;
    List<Mesh> meshes = new List<Mesh>();

    // Start is called before the first frame update
    void Start()
    {
        string path = Directory.GetCurrentDirectory();
        Debug.Log(path);
        IfcUtil util = new IfcUtil();
        string Location = "C:\\Users\\Jason\\Documents\\IfcObjUnity\\Assets\\Ifc Sample\\ifc_example\\ifc_example\\B_Damage_Types_andCondition.ifc";
        //Int64 ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, Location, "IFC2X3_TC1.exp");
        bool result = util.ParseIFCFile(Location);
        Debug.Log("Found georeference with coordinates:" + util.Latitude.ToString() + "," + util.Longitude.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
