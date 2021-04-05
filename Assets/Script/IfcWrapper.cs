using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Linq;
using UnityEngine;

/// <summary>
/// The main entry point for the application.
/// </summary>
/// 

/// <summary>
/// Types of supported movements
/// </summary>
enum MOVE_TYPE
{
    ROTATE,
    PAN,
    ZOOM,
    NONE,
}

/// <summary>
/// IFCItem presents a single ifc item for drawing 
/// </summary>
public class IFCItem
{
    public void CreateItem(IFCItem parent, long ifcID, string ifcType, string globalID, string name, string desc)
    {

        this.parent = parent;
        this.next = null;
        this.child = null;
        this.globalID = globalID;
        this.ifcID = ifcID;
        this.ifcType = ifcType;
        this.description = desc;
        this.name = name;

        if (parent != null)
        {
            if (parent.child == null)
            {
                parent.child = this;
            }
            else
            {
                IFCItem NextChild = parent;

                while (true)
                {
                    if (NextChild.next == null)
                    {
                        NextChild.next = this;
                        break;
                    }
                    else
                    {
                        NextChild = NextChild.next;
                    }

                }

            }

        }
    }
    public long ifcID = 0;
    public string globalID;
    public string ifcType;
    public string name;
    public string description;
    public IFCItem parent = null;
    public IFCItem next = null;
    public IFCItem child = null;
    public long noVerticesForFaces;
    public long noPrimitivesForFaces;
    public float[] verticesForFaces;
    public long[] indicesForFaces;
    public long vertexOffsetForFaces;
    public long indexOffsetForFaces;
    public long noVerticesForWireFrame;
    public int noPrimitivesForWireFrame;
    public float[] verticesForWireFrame;
    public long[] indicesForWireFrame;
    public long[] indicesForWireFrameLineParts;
    public long vertexOffsetForWireFrame;
    public long indexOffsetForWireFrame;

    //public IFCTreeItem ifcTreeItem = null;


}

/// <summary>
/// Class aims to read ifc file and draw its objects 
/// </summary>
public class IfcViewerWrapper
{
    public IFCItem RootIfcItem = null;
    //private TreeView _treeControl = null;
    //public CIFCTreeData _treeData = new CIFCTreeData();
    private Vector3 _vEyePt = new Vector3(1.5f, 0, .5f);
    private Vector3 _vTargetPt = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 _vUpVector = new Vector3(0.0f, 0.0f, 1.0f);
    private int _counter = 0;
    private float _valueZ = 0;
    private float _valueX = 0;
    private MOVE_TYPE _currentMoveType = MOVE_TYPE.NONE;
    private bool _enableWireFrames = true;
    private bool _enableFaces = true;
    private bool _enableHover = true;
    private int currentPos = 0;
    private int currentPosInd = 0;
    private float roll_val = 0.0f;
    private float pitch_val = 0.0f;
    private float yaw_val = 0.0f;
    private float _zoomIndex = 0F;
    private Vector3 _panVector = new Vector3(0, 0, 0);
    //private VertexBuffer m_vertexBuffer = null;
    //private IndexBuffer m_indexBuffer = null;
    Material _mtrlDefault;
    Material _mtrlBlack;
    Material _mtrlRed;

    private IFCItem _hoverIfcItem = null;
    private IFCItem _selectedIfcItem = null;
    Vector3 center = new Vector3();
    float size = 0;

    public string Location { get; private set; }

    public string Name { get; private set; }

    public object Data { get; set; }

    // Entering Line
    public IfcViewerWrapper(string sPath)
    {
        Location = sPath;
        Name = "Ifc File" + Path.GetFileName(sPath).Split('.').First();

        Debug.Log("Loading ifc file " + Location);

        var thread = new Thread(LoadThreadFunction)
        {
            Name = "Ifc loader " + Path.GetFileName(Location)
        };
        thread.Start();
    }

    private void LoadThreadFunction()
    {
        if (true == File.Exists(Location))
        {
            var path = System.Text.Encoding.UTF8.GetBytes(Location);

            Int64 ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, Location, "IFC2X3_TC1.exp");

            string xmlSettings_IFC2x3 = @"IFC2X3-Settings.xml";
            string xmlSettings_IFC4 = @"IFC4-Settings.xml";

            if (ifcModel != 0)
            {

                IntPtr outputValue = IntPtr.Zero;

                IfcEngine.x64.GetSPFFHeaderItem(ifcModel, 9, 0, IfcEngine.x64.sdaiSTRING, out outputValue);

                string s = Marshal.PtrToStringAnsi(outputValue);


                XmlTextReader textReader = null;
                if (s.Contains("IFC2") == true)
                {
                    textReader = new XmlTextReader(xmlSettings_IFC2x3);
                }
                else
                {
                    if (s.Contains("IFC4") == true)
                    {
                        IfcEngine.x64.sdaiCloseModel(ifcModel);
                        ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, Location, "Ifc/IFC4.exp");

                        if (ifcModel != 0)
                            textReader = new XmlTextReader(xmlSettings_IFC4);
                    }
                }

                if (textReader == null)
                {
                    Debug.Log("Problem while loading ifc file");
                    return;
                }

                // if node type us an attribute
                while (textReader.Read())
                {
                    textReader.MoveToElement();

                    if (textReader.AttributeCount > 0)
                    {
                        if (textReader.LocalName == "object")
                        {
                            if (textReader.GetAttribute("name") != null)
                            {
                                string Name = textReader.GetAttribute("name").ToString();
                                //string Desc = textReader.GetAttribute("description").ToString();

                                RetrieveObjects(ifcModel, Name, Name);
                            }
                        }
                    }
                }

                int a = 0;
                GenerateGeometry(ifcModel, RootIfcItem, ref a);

                #region commented

                #endregion

                // -----------------------------------------------------------------

                IfcEngine.x64.sdaiCloseModel(ifcModel);

                this.Data = this;

                Debug.Log("Ifc file loaded");
                return;
            }
        }

        Debug.Log("Problem while loading ifc file");
        return;
    }

    public static string GetValidPathName(string path)
    {
        return Path.GetInvalidFileNameChars().Aggregate(path, (current, c) => current.Replace(c.ToString(), string.Empty));
    }

    private void GenerateWireFrameGeometry(long ifcModel, IFCItem ifcItem)
    {
        if (ifcItem.ifcID != 0)
        {
            long noVertices = 0, noIndices = 0;
            IfcEngine.x64.initializeModellingInstance(ifcModel, ref noVertices, ref noIndices, 0, ifcItem.ifcID);

            if (noVertices != 0 && noIndices != 0)
            {
                ifcItem.noVerticesForWireFrame = noVertices;
                ifcItem.verticesForWireFrame = new float[3 * noVertices];
                ifcItem.indicesForWireFrame = new long[noIndices];

                float[] pVertices = new float[noVertices * 3];

                IfcEngine.x64.finalizeModelling(ifcModel, pVertices, ifcItem.indicesForWireFrame, 0);

                int i = 0;
                while (i < noVertices)
                {
                    ifcItem.verticesForWireFrame[3 * i + 0] = pVertices[3 * i + 0];
                    ifcItem.verticesForWireFrame[3 * i + 1] = pVertices[3 * i + 1];
                    ifcItem.verticesForWireFrame[3 * i + 2] = pVertices[3 * i + 2];

                    i++;
                };

                ifcItem.noPrimitivesForWireFrame = 0;
                ifcItem.indicesForWireFrameLineParts = new long[2 * noIndices];

                long faceCnt = IfcEngine.x64.getConceptualFaceCnt(ifcItem.ifcID);

                for (int j = 0; j < faceCnt; j++)
                {
                    long startIndexFacesPolygons = 0, noIndicesFacesPolygons = 0;
                    long nonValue = 0;
                    long nonValue1 = 0;
                    long nonValue2 = 0;
                    IfcEngine.x64.getConceptualFaceEx(ifcItem.ifcID, j, ref nonValue, ref nonValue, ref nonValue, ref nonValue, ref nonValue, ref nonValue1, ref startIndexFacesPolygons, ref noIndicesFacesPolygons, ref nonValue2, ref nonValue2);

                    i = 0;
                    long lastItem = -1;
                    while (i < noIndicesFacesPolygons)
                    {
                        if (lastItem >= 0 && ifcItem.indicesForWireFrame[startIndexFacesPolygons + i] >= 0)
                        {
                            ifcItem.indicesForWireFrameLineParts[2 * ifcItem.noPrimitivesForWireFrame + 0] = lastItem;
                            ifcItem.indicesForWireFrameLineParts[2 * ifcItem.noPrimitivesForWireFrame + 1] = ifcItem.indicesForWireFrame[startIndexFacesPolygons + i];
                            ifcItem.noPrimitivesForWireFrame++;
                        }
                        lastItem = ifcItem.indicesForWireFrame[startIndexFacesPolygons + i];
                        i++;
                    }
                }
            }
        }
    }

    private void GenerateFacesGeometry(long ifcModel, IFCItem ifcItem)
    {
        if (ifcItem.ifcID != 0)
        {
            long noVertices = 0, noIndices = 0;
            IfcEngine.x64.initializeModellingInstance(ifcModel, ref noVertices, ref noIndices, 0, ifcItem.ifcID);

            if (noVertices != 0 && noIndices != 0)
            {
                ifcItem.noVerticesForFaces = noVertices;
                ifcItem.noPrimitivesForFaces = noIndices / 3;
                ifcItem.verticesForFaces = new float[6 * noVertices];
                ifcItem.indicesForFaces = new long[noIndices];

                float[] pVertices = new float[noVertices * 6];

                IfcEngine.x64.finalizeModelling(ifcModel, pVertices, ifcItem.indicesForFaces, 0);

                int i = 0;
                while (i < noVertices)
                {
                    ifcItem.verticesForFaces[6 * i + 0] = pVertices[6 * i + 0];
                    ifcItem.verticesForFaces[6 * i + 1] = pVertices[6 * i + 1];
                    ifcItem.verticesForFaces[6 * i + 2] = pVertices[6 * i + 2];

                    ifcItem.verticesForFaces[6 * i + 3] = pVertices[6 * i + 3];
                    ifcItem.verticesForFaces[6 * i + 4] = pVertices[6 * i + 4];
                    ifcItem.verticesForFaces[6 * i + 5] = pVertices[6 * i + 5];

                    i++;
                }
            }
        }
    }

    void GenerateGeometry(long ifcModel, IFCItem ifcItem, ref int a)
    {
        while (ifcItem != null)
        {
            // -----------------------------------------------------------------
            // Generate WireFrames Geometry

            long setting = 0;
            long mask = 0;
            mask += IfcEngine.x64.flagbit2;        //    PRECISION (32/64 bit)
            mask += IfcEngine.x64.flagbit3;        //	   INDEX ARRAY (32/64 bit)
            mask += IfcEngine.x64.flagbit5;        //    NORMALS
            mask += IfcEngine.x64.flagbit8;        //    TRIANGLES
            mask += IfcEngine.x64.flagbit12;       //    WIREFRAME
            setting += 0;            //    DOUBLE PRECISION (double)

            if (IntPtr.Size == 4) // indication for 32
            {
                setting += 0;            //    32 BIT INDEX ARRAY (Int32)
            }
            else
            {
                if (IntPtr.Size == 8)
                {
                    setting += IfcEngine.x64.flagbit3;     // 64 BIT INDEX ARRAY (Int64)
                }
            }

            setting += 0;            //    NORMALS OFF
            setting += 0;            //    TRIANGLES OFF
            setting += IfcEngine.x64.flagbit12;    //    WIREFRAME ON


            IfcEngine.x64.setFormat(ifcModel, setting, mask);

            GenerateWireFrameGeometry(ifcModel, ifcItem);
            // -----------------------------------------------------------------
            // Generate Faces Geometry

            setting = 0;
            setting += 0;            //    SINGLE PRECISION (float)
            if (IntPtr.Size == 4) // indication for 32
            {
                setting += 0;            //    32 BIT INDEX ARRAY (Int32)
            }
            else
            {
                if (IntPtr.Size == 8)
                {
                    setting += IfcEngine.x64.flagbit3;     //    64 BIT INDEX ARRAY (Int64)
                }
            }

            setting += IfcEngine.x64.flagbit5;     //    NORMALS ON
            setting += IfcEngine.x64.flagbit8;     //    TRIANGLES ON
            setting += 0;            //    WIREFRAME OFF 
            IfcEngine.x64.setFormat(ifcModel, setting, mask);

            GenerateFacesGeometry(ifcModel, ifcItem);

            IfcEngine.x64.cleanMemory(ifcModel, 0);

            GenerateGeometry(ifcModel, ifcItem.child, ref a);
            ifcItem = ifcItem.next;
        }
    }

    private void RetrieveObjects(long ifcModel, string sObjectSPFFName, string ObjectDisplayName)
    {
        long ifcObjectInstances = IfcEngine.x64.sdaiGetEntityExtentBN(ifcModel, ObjectDisplayName),
            noIfcObjectIntances = IfcEngine.x64.sdaiGetMemberCount(ifcObjectInstances);

        if (noIfcObjectIntances != 0)
        {
            IFCItem NewItem = null;
            if (RootIfcItem == null)
            {
                RootIfcItem = new IFCItem();
                RootIfcItem.CreateItem(null, 0, "", ObjectDisplayName, "", "");

                NewItem = RootIfcItem;
            }
            else
            {
                IFCItem LastItem = RootIfcItem;
                while (LastItem != null)
                {
                    if (LastItem.next == null)
                    {
                        LastItem.next = new IFCItem();
                        LastItem.next.CreateItem(null, 0, "", ObjectDisplayName, "", "");

                        NewItem = LastItem.next;

                        break;
                    }
                    else
                        LastItem = LastItem.next;
                };
            }


            for (int i = 0; i < noIfcObjectIntances; ++i)
            {
                long ifcObjectIns = 0;
                IfcEngine.x64.engiGetAggrElement(ifcObjectInstances, i, IfcEngine.x64.sdaiINSTANCE, out ifcObjectIns);

                IntPtr value = IntPtr.Zero;
                IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "GlobalId", IfcEngine.x64.sdaiSTRING, out value);

                string globalID = Marshal.PtrToStringAnsi((IntPtr)value);

                value = IntPtr.Zero;
                IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "Name", IfcEngine.x64.sdaiSTRING, out value);

                string name = Marshal.PtrToStringAnsi((IntPtr)value);

                value = IntPtr.Zero;
                IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "Description", IfcEngine.x64.sdaiSTRING, out value);

                string description = Marshal.PtrToStringAnsi((IntPtr)value);

                IFCItem subItem = new IFCItem();
                subItem.CreateItem(NewItem, ifcObjectIns, ObjectDisplayName, globalID, name, description);
            }
        }
    }

    private void GetDimensions(IFCItem ifcItem, ref Vector3 min, ref Vector3 max, ref bool InitMinMax)
    {
        while (ifcItem != null)
        {
            if (ifcItem.noVerticesForFaces != 0)
            {
                if (InitMinMax == false)
                {
                    min.x = ifcItem.verticesForFaces[3 * 0 + 0];
                    min.y = ifcItem.verticesForFaces[3 * 0 + 1];
                    min.z = ifcItem.verticesForFaces[3 * 0 + 2];
                    max = min;

                    InitMinMax = true;
                }

                int i = 0;
                while (i < ifcItem.noVerticesForFaces)
                {

                    min.x = Math.Min(min.x, ifcItem.verticesForFaces[6 * i + 0]);
                    min.y = Math.Min(min.y, ifcItem.verticesForFaces[6 * i + 1]);
                    min.z = Math.Min(min.z, ifcItem.verticesForFaces[6 * i + 2]);

                    max.x = Math.Max(max.x, ifcItem.verticesForFaces[6 * i + 0]);
                    max.y = Math.Max(max.y, ifcItem.verticesForFaces[6 * i + 1]);
                    max.z = Math.Max(max.z, ifcItem.verticesForFaces[6 * i + 2]);

                    i++;
                }
            }

            GetDimensions(ifcItem.child, ref min, ref max, ref InitMinMax);

            ifcItem = ifcItem.next;
        }
    }

    private void GetBufferSizes_ifcFaces(IFCItem item, ref long pVBuffSize, ref long pIBuffSize)
    {
        while (item != null)
        {
            if (item.ifcID != 0 && item.noVerticesForFaces != 0 && item.noPrimitivesForFaces != 0)
            {
                item.vertexOffsetForFaces = pVBuffSize;
                item.indexOffsetForFaces = pIBuffSize;

                pVBuffSize += item.noVerticesForFaces;
                pIBuffSize += 3 * item.noPrimitivesForFaces;
            }

            GetBufferSizes_ifcFaces(item.child, ref pVBuffSize, ref pIBuffSize);

            item = item.next;
        }
    }

    private void GetBufferSizes_ifcWireFrame(IFCItem item, ref long pVBuffSize, ref long pIBuffSize)
    {
        while (item != null)
        {
            if (item.ifcID != 0 && item.noVerticesForWireFrame != 0 && item.noPrimitivesForWireFrame != 0)
            {
                item.vertexOffsetForWireFrame = pVBuffSize;
                item.indexOffsetForWireFrame = pIBuffSize;

                pVBuffSize += item.noVerticesForWireFrame;
                pIBuffSize += 2 * item.noPrimitivesForWireFrame;
            }

            GetBufferSizes_ifcWireFrame(item.child, ref pVBuffSize, ref pIBuffSize);

            item = item.next;
        }
    }

    public bool WireFrames
    {
        get
        {
            return _enableWireFrames;
        }
        set
        {
            _enableWireFrames = value;
        }
    }
    public bool Faces
    {
        get
        {
            return _enableFaces;
        }
        set
        {
            _enableFaces = value;
        }
    }
}
