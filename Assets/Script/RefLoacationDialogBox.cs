using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RefLoacationDialogBox : DialogBox
{
    // Elements
    public GameObject Name;

    // Surface Depth Layer
    public GameObject DepthLayer;
    public GameObject DepthValue;
    public GameObject DepthConfirmButton;

    // Surface Selection Layer
    public GameObject SurfaceLayer;
    public GameObject SelectedNumberValue;
    public GameObject SurfaceConfirmButton;

    // Reset Button Object
    public GameObject ResetButton;

    // Options
    public GameObject OptionsLayer;
    public GameObject Back_Button;
    public GameObject Next_Button;

    // Object Asset
    public GameObject Plane_Control;
    public GameObject Double_axis_arrow;
    public GameObject Point3D;

    // Intersection Point
    private GameObject ImageOrigin;

    // Surface Plane
    private GameObject plane;

    // Extracted UI Elements
    private Text ImageName;
    private Text DepthText;
    private Text SelectedNumberText;

    private Button Reset_Btn;
    private Button Depth_Confirm_Btn;
    private Button Surface_Confirm_Btn;
    private Button Back_Btn;
    private Button Next_Btn;

    private ObservableCollection<Surface> surfaces = new ObservableCollection<Surface>();

    // keep a copy of the executing script
    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        // Extract Elements
        ImageName = getText(Name);
        DepthText = getText(DepthValue);
        SelectedNumberText = getText(SelectedNumberValue);

        // Assign Buttons
        Reset_Btn = getButton(ResetButton);
        Depth_Confirm_Btn = getButton(DepthConfirmButton);
        Surface_Confirm_Btn = getButton(SurfaceConfirmButton);
        Back_Btn = getButton(Back_Button);
        Next_Btn = getButton(Next_Button);

        // Mapping Button with Action
        Reset_Btn.onClick.AddListener(reset);
        Back_Btn.onClick.AddListener(back);
        Next_Btn.onClick.AddListener(next);

        // Disable Button Action
        if (Next_Btn != null) Next_Btn.interactable = false;
        if (Reset_Btn != null) Reset_Btn.interactable = false;

        // Draw the name of image
        ImageName.text = _DamageInstance.ImageName;

        // Surface List Change
        surfaces.CollectionChanged += updateSurfacesCount;

        // Set coroutine with starter queue
        coroutine = plot3D();
        StartCoroutine(coroutine);
    }

    void UnFreezeScreen()
    {
        if (_DamageGUI != null)
            (_DamageGUI as DamageGUI).deFreezeScreen();
    }

    void updateSurfacesCount(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (SelectedNumberText != null)
            SelectedNumberText.text = System.String.Format("{0}", surfaces.Count);

        if (surfaces.Count > 2) CreatePoint();
    }

    void reset()
    {
        StopAllCoroutines();
        StartCoroutine(coroutine);
    }

    void back()
    {
        //(_DamageInstance as DamageInstance).setImage();
        DoneOperation(DimNotification.Back_Operation);
    }

    void next()
    {
        //(_DamageInstance as DamageInstance).setImage();
        if (ImageOrigin != null)
        {
            var imageOrigin = ImageOrigin.transform.position;
            _DamageInstance.ImageOrigin = imageOrigin;
            Destroy(ImageOrigin);
        }

        nextSequence(_DamageInstance.ImageType);
    }

    void nextSequence(ImageType _imageType)
    {
        switch (_imageType)
        {
            case ImageType.Image_1D:
                // Preview the image file
                DoneOperation(DimNotification.Next_RefLoacationOperation);
                break;

            case ImageType.Image_2D:
                // Preview the image file
                DoneOperation(DimNotification.Next_RefLoacationOperation);
                break;

            case ImageType.Image_3D:
                // Propose re-orientation function
                DoneOperation(DimNotification.Next_Ref3DLoacationOperation);
                break;
        }
    }

    IEnumerator plot3D()
    {
        surfaces.Clear();
        UnFreezeScreen();
        yield return CreateSurface();
    }

    IEnumerator CreateSurface()
    {
        if (DepthLayer != null) DepthLayer.SetActive(false);
        if (Reset_Btn != null) Reset_Btn.interactable = false;

        // Define Temporary plane Object
        GameObject plane = Instantiate(Plane_Control, Vector3.zero, Quaternion.identity);

        var _SurfaceInstance = plane.AddComponent<SurfaceInstance>();
        _SurfaceInstance.DefineSurface(Surface_Confirm_Btn, Depth_Confirm_Btn, DepthText, DepthLayer, SurfaceConfirmButton);
        _SurfaceInstance.OnSurfaceBuilt += surfaceBuilt;

        while (plane)
        {
            yield return null;
        }

        if (surfaces.Count < 3) yield return CreateSurface();
    }

    void surfaceBuilt(object sender, Surface surface)
    {
        if (validateSurfaces(surface)) surfaces.Add(surface);
        Destroy(sender as GameObject);
    }

    bool validateSurfaces(Surface surface)
    {
        foreach (var s in surfaces)
            if (Vector3.Dot(surface.normal, s.normal) == 1) return false;

        return true;
    }

    // Return Point if found
    void CreatePoint()
    {
        Plane p0 = surfaces[0].plane;
        Plane p1 = surfaces[1].plane;
        Plane p2 = surfaces[2].plane;

        Vector3 intersectionPoint;
        if (planesIntersectAtSinglePoint(p0: p0, p1: p1, p2: p2, out intersectionPoint))
        {
            ImageOrigin = Instantiate(Point3D, intersectionPoint, Quaternion.identity);
            ImageOrigin.name = System.Guid.NewGuid().ToString("D");
            SurfaceLayer.SetActive(false);
            DepthLayer.SetActive(false);
            if (Next_Btn != null) Next_Btn.interactable = true;
        } 
    }

    // Check if surfaces make sense
    //IEnumerator createLine()
    //{
    //    Surface s1 = surfaces[0];
    //    Surface s2 = surfaces[1];

    //    Vector3 origin;
    //    Vector3 direction;

    //    if(PlanePlaneIntersection(out origin, out direction, plane1Normal: s1.normal, plane1Position: s1.position, plane2Normal: s2.normal, plane2Position: s2.position))
    //    {
    //        Instantiate(Resources.Load<GameObject>("Prefabs/Double_axis_arrow"), origin, Quaternion.FromToRotation(Vector3.up, direction));
    //        possible_surface = new Surface(position: origin, normal: direction);
    //    } 

    //    yield return null;
    //}

    // Reference: https://gist.github.com/StagPoint/2eaa878f151555f9f96ae7190f80352e
    // A small fix that the dot product might be negative due to direction
    private static bool planesIntersectAtSinglePoint(Plane p0, Plane p1, Plane p2, out Vector3 intersectionPoint)
    {
        const float EPSILON = 1e-4f;

        var det = Vector3.Dot(Vector3.Cross(p0.normal, p1.normal), p2.normal);
        if (System.Math.Abs(det) < EPSILON)
        {
            intersectionPoint = Vector3.zero;
            Debug.Log("No point found.");
            return false;
        }

        intersectionPoint =
            (-(p0.distance * Vector3.Cross(p1.normal, p2.normal)) -
            (p1.distance * Vector3.Cross(p2.normal, p0.normal)) -
            (p2.distance * Vector3.Cross(p0.normal, p1.normal))) / det;

        return true;
    }

    public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position)
    {

        linePoint = Vector3.zero;
        lineVec = Vector3.zero;

        //We can get the direction of the line of intersection of the two planes by calculating the 
        //cross product of the normals of the two planes. Note that this is just a direction and the line
        //is not fixed in space yet. We need a point for that to go with the line vector.
        lineVec = Vector3.Cross(plane1Normal, plane2Normal);

        //Next is to calculate a point on the line to fix it's position in space. This is done by finding a vector from
        //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
        //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
        //the cross product of the normal of plane2 and the lineDirection.		
        Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);

        float denominator = Vector3.Dot(plane1Normal, ldir);

        //Prevent divide by zero and rounding errors by requiring about 5 degrees angle between the planes.
        if (Mathf.Abs(denominator) > 0.006f)
        {

            Vector3 plane1ToPlane2 = plane1Position - plane2Position;
            float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / denominator;
            linePoint = plane2Position + t * ldir;

            return true;
        }

        //output not valid
        else
        {
            return false;
        }
    }   
}

public struct Surface
{
    public Surface(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
        this.plane = new Plane(this.normal, this.position);
    }

    public Vector3 position { get; private set; }

    public Vector3 normal { get; private set; }

    public Plane plane { get; private set; }

    public void updatePosition(float distance)
    {
        this.position += distance * this.normal.normalized;
        this.plane = new Plane(this.normal, this.position);
    }

    public void flipSide()
    {
        this.normal = -1 * this.normal;
    }
}

public class SurfaceInstance : MonoBehaviour
{
    // Surface Layer
    private Surface surface;

    // Surface Depth Layer
    private GameObject DepthLayer;

    // Surface Selection Layer
    private GameObject SurfaceConfirmButton;

    private Button Depth_Confirm_Btn;
    private Button Surface_Confirm_Btn;
    private Text DepthText;

    // bool if confirm surface is set
    private bool confirm_Surface = false;

    private Arrow_Ctrl arrow_ctrl;
    private float depth_Value = 0.0f;

    public delegate void SurfaceBuilt(object sender, Surface surface);

    public event SurfaceBuilt OnSurfaceBuilt;

    public void DefineSurface( Button Surface_Confirm_Btn, Button Depth_Confirm_Btn, Text DepthText, GameObject DepthLayer, GameObject SurfaceConfirmButton)
    {
        this.Surface_Confirm_Btn = Surface_Confirm_Btn;
        this.Depth_Confirm_Btn = Depth_Confirm_Btn;
        this.DepthText = DepthText;
        this.DepthLayer = DepthLayer;
        this.SurfaceConfirmButton = SurfaceConfirmButton;
        startSurface();
    }

    void startSurface()
    {
        Surface_Confirm_Btn.interactable = false;
        Depth_Confirm_Btn.interactable = false;
        DepthLayer.SetActive(false);
        SurfaceConfirmButton.SetActive(true);

        arrow_ctrl = GetComponentInChildren<Arrow_Ctrl>();
        arrow_ctrl.Activate(false);
        foreach (var MeshRender in GetComponentsInChildren<MeshRenderer>())
            MeshRender.enabled = false;

        surface = new Surface(position: Vector3.zero, normal: Vector3.up);

        Depth_Confirm_Btn.onClick.AddListener(ConfirmDepth);
        Surface_Confirm_Btn.onClick.AddListener(ConfirmSurface);

        // Action event Binding
        arrow_ctrl.OnDragged += calculate;
        showDepth();
    }

    void ConfirmSurface()
    {
        arrow_ctrl.Activate(true);
        Surface_Confirm_Btn.interactable = false;
        SurfaceConfirmButton.SetActive(false);
        Depth_Confirm_Btn.interactable = true;
        DepthLayer.SetActive(true);
        confirm_Surface = true;
    }

    void ConfirmDepth()
    {
        surface.updatePosition(depth_Value);
        arrow_ctrl.OnDragged -= calculate;
        EndSurface();
    }

    void Update()
    {
        if (arrow_ctrl != null )
            if (!confirm_Surface && Input.GetMouseButtonDown(0)) getSurface();
    }

    void EndSurface()
    {
        Depth_Confirm_Btn.onClick.RemoveAllListeners();
        Surface_Confirm_Btn.onClick.RemoveAllListeners();
        OnSurfaceBuilt(gameObject, surface);
    }

    void showDepth()
    {
        if (DepthText != null) DepthText.text = System.String.Format("{0:0.00}", depth_Value);
    }

    void calculate(object sender, Vector3 position)
    {
        float distance = Vector3.Distance(surface.position, position);

        if (!((position - surface.position).normalized == surface.normal.normalized))
        {
            transform.SetPositionAndRotation(position, Quaternion.FromToRotation(Vector3.up, -1 * surface.normal));
            surface.flipSide();
        }

        depth_Value = distance;
        showDepth();
    }

    // Check only the designated layer for surface normal
    private void getSurface()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        int layerMask = 1 << LayerMask.NameToLayer("Control");
        layerMask = ~layerMask;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            // Find the line from the gun to the point that was clicked.
            Vector3 position = hit.point;
            Vector3 normal = hit.normal;

            // Print out hit position
            // Debug.LogFormat("Hit point: {0}", position);
            // Debug.LogFormat("Hit Normal: {0}", normal);

            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return;

            // Extract mesh information
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            List<SurfaceMesh> surfaceMesh = new List<SurfaceMesh>();

            //Debug.LogFormat("Number of triangle: {0}", triangles.Length);
            //Debug.LogFormat("Number of vertices: {0}", vertices.Length);
            int triangle_index = 0;
            // The hit normal vector match triangle
            for (int i = 0; i < triangles.Length; i = i + 3)
            {
                surfaceMesh.Add(new SurfaceMesh(
                    triangle_index,
                    new Vector3[] { vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]] },
                    new Vector3[] { normals[triangles[i]], normals[triangles[i + 1]], normals[triangles[i + 2]] }
                ));

                triangle_index++;

                //Debug.Log("Triangle consists of:");
                //Debug.LogFormat("Vertex in Mesh: {0}, Normal in Mesh: {1}", vertices[triangles[i]], normals[triangles[i]]);
                //Debug.LogFormat("Vertex in Mesh: {0}, Normal in Mesh: {1}", vertices[triangles[i + 1]], normals[triangles[i + 1]]);
                //Debug.LogFormat("Vertex in Mesh: {0}, Normal in Mesh: {1}", vertices[triangles[i + 2]], normals[triangles[i + 2]]);
            }

            //var selectedTriangles = normals.Where(itm => Vector3.Dot(itm, normal) == 1)

            var hitSurface = surfaceMesh.Single(s_mesh => s_mesh.index == hit.triangleIndex);

            Vector3 p0 = hitSurface.vertex[0];
            Vector3 p1 = hitSurface.vertex[1];
            Vector3 p2 = hitSurface.vertex[2];

            var TotalSurface = surfaceMesh.Where(s_mesh => s_mesh.checkNormal(normal))
                .Select(s_mesh => s_mesh.vertex)
                .SelectMany(s_mesh => new List<Vector3>(s_mesh));

            var VerticesIndex = TotalSurface.Distinct()
                .ToArray();

            var VerticesIndexList = IndexMesh.buildMeshIndex(VerticesIndex);

            var TriangleIndexArray = TotalSurface.Select(s_mesh => IndexMesh.getIndex(VerticesIndexList, s_mesh))
                .ToArray();

            // Transform of the hit point
            Transform hitTransform = hit.collider.transform;
            //Debug.LogFormat("Previous point: {0}, {1}, {2}", p0, p1, p2);
            p0 = hitTransform.TransformPoint(p0);
            p1 = hitTransform.TransformPoint(p1);
            p2 = hitTransform.TransformPoint(p2);
            //Debug.LogFormat("Transform point: {0}, {1}, {2}", p0, p1, p2);

            var VerticesIndexArray = TotalSurface.Distinct()
                .Select(s_vertex => hitTransform.TransformPoint(s_vertex))
                .ToArray();

            //foreach (var verticesIndexArray in VerticesIndexArray)
            //    Debug.LogFormat("Vertex: {0}", verticesIndexArray);

            //foreach (var triangleIndexArray in TriangleIndexArray)
            //    Debug.LogFormat("Triangle: {0}", triangleIndexArray);

            var colliderMesh = new Mesh();
            colliderMesh.vertices = VerticesIndexArray;
            colliderMesh.triangles = TriangleIndexArray;
            //colliderMesh.vertices = new Vector3[] { p0, p1, p2 };
            //colliderMesh.triangles = new int[] { 0, 1, 2 };
            colliderMesh.RecalculateNormals();

            transform.SetPositionAndRotation(position, Quaternion.FromToRotation(Vector3.up, normal));

            arrow_ctrl.updateMesh(colliderMesh);
            surface = new Surface(position: position, normal: normal);

            foreach (var MeshRender in GetComponentsInChildren<MeshRenderer>())
                MeshRender.enabled = true;

            Surface_Confirm_Btn.interactable = true;
        }
    }

    struct IndexMesh
    {
        int index { get; set; }
        Vector3 vertex { get; set; }

        IndexMesh(int index, Vector3 vertex)
        {
            this.index = index;
            this.vertex = vertex;
        }

        public static List<IndexMesh> buildMeshIndex(Vector3[] Vertices)
        {
            List<IndexMesh> indexMeshList = new List<IndexMesh>();

            for (int i = 0; i < Vertices.Length; i++)
                indexMeshList.Add(new IndexMesh(i, Vertices[i]));

            return indexMeshList;
        }

        public static int getIndex(List<IndexMesh> meshIndex, Vector3 vertex)
        {
            var indexMesh = meshIndex.Single(idxMesh => idxMesh.vertex == vertex);
            return indexMesh.index;
        }
    }

    struct SurfaceMesh
    {
        public int index { get; private set; }
        public Vector3[] vertex { get; private set; }
        public Vector3[] normal { get; private set; }

        public SurfaceMesh(int index, Vector3[] vertex, Vector3[] normal)
        {
            this.index = index;
            this.vertex = vertex;
            this.normal = normal;
        }

        public bool checkNormal(Vector3 normal)
        {
            bool result = false;
            result = Vector3.Dot(this.normal[0], normal) == 1;
            result = result && Vector3.Dot(this.normal[1], normal) == 1;
            result = result && Vector3.Dot(this.normal[2], normal) == 1;

            return result;
        }
    }
}
