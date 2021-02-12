using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class surfaceBuider : MonoBehaviour
{
    List<Surface> surfaces = new List<Surface>();
    Surface possible_surface;
    private bool error = false;

    public event PointObtained OnPointObtained; // event

    public delegate void PointObtained(object sender, Vector3 intersectionPoint);

    public GameObject Point3D;

    public void plot3D()
    {
        StartCoroutine(CreateSurfaces());
    }

    IEnumerator CreateSurfaces()
    {
        yield return CreateSurface();
        yield return CreateSurface();
        yield return validate2Surfaces();
        yield return validate3Surfaces();
        yield return CreatePoint();

        //foreach (Surface s in surfaces)
        //    Debug.Log(System.String.Format("Normal: {0}, Position: {1}", s.getNormal(), s.getPosition()));    
    }

    IEnumerator CreatePoint()
    {
        Plane p0 = surfaces[0].getPlane();
        Plane p1 = surfaces[1].getPlane();
        Plane p2 = surfaces[2].getPlane();

        Vector3 intersectionPoint;
        if (planesIntersectAtSinglePoint(p0 : p0, p1: p1, p2: p2, out intersectionPoint))
        {
            Point3D = Instantiate(Resources.Load<GameObject>("Prefabs/Pointer"), intersectionPoint, Quaternion.identity);
            OnPointObtained(this, intersectionPoint);
        } 

        yield return null;
    }

    IEnumerator validate2Surfaces()
    {
        while (surfaces[0].getNormal().normalized == surfaces[1].getNormal().normalized)
        {
            error = true;
            yield return CreateSurface();
        }
        error = false;
        yield return null;
    }

    IEnumerator validate3Surfaces()
    {
        if (surfaces.Count < 3) yield return CreateSurface();
        while (surfaces[0].getNormal().normalized == surfaces[2].getNormal().normalized || surfaces[1].getNormal().normalized == surfaces[2].getNormal().normalized)
        {
            error = true;
            yield return CreateSurface();
        }
        error = false;
        yield return null;
    }

    IEnumerator createLine()
    {
        Surface s1 = surfaces[0];
        Surface s2 = surfaces[1];

        Vector3 origin;
        Vector3 direction;

        if(PlanePlaneIntersection(out origin, out direction, plane1Normal: s1.getNormal(), plane1Position: s1.getPosition(), plane2Normal: s2.getNormal(), plane2Position: s2.getPosition()))
        {
            Instantiate(Resources.Load<GameObject>("Prefabs/Double_axis_arrow"), origin, Quaternion.FromToRotation(Vector3.up, direction));
            possible_surface = new Surface(position: origin, normal: direction);
        } 

        yield return null;
    }

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

    IEnumerator CreateSurface()
    {
        GameObject object1 = new GameObject();
        SurfaceVariable sv = object1.AddComponent<SurfaceVariable>() as SurfaceVariable;
        sv.OnSurfaceBuilt += TerminateObject;

        while (object1)
        {
            yield return null;
        }
    }

    void TerminateObject(object sender, Surface surface)
    {
        if (error) surfaces[surfaces.Count - 1] = surface;
        else surfaces.Add(surface);

        SurfaceVariable sv = sender as SurfaceVariable;
        sv.OnSurfaceBuilt -= TerminateObject;
        Destroy(sv.gameObject);
    }

    void OnGUI()
    {
        if (error) GUI.Label(new Rect(Screen.width - 200, 50, 200, 50), "Surfaces cannot be of same direction!!");
    }
}

public struct Surface
{
    Vector3 position;
    Vector3 normal;
    Plane plane;

    public Surface(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
        this.plane = new Plane(this.normal, this.position);
    }

    public Vector3 getPosition()
    {
        return this.position;
    }

    public Vector3 getNormal()
    {
        return this.normal;
    }

    public Plane getPlane()
    {
        return this.plane;
    }

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

public class SurfaceVariable : MonoBehaviour
{
    private bool confirm = false;
    private bool depth = false;
    private Surface surface;
    private GameObject plane;
    private Arrow_Ctrl arrow_ctrl;
    private float depth_Value = 0.0f;

    public delegate void surfaceBuilt(object sender, Surface surface);
    public event surfaceBuilt OnSurfaceBuilt;

    void Start()
    {
        if (plane == null) plane = Instantiate(Resources.Load<GameObject>("Prefabs/Plane_Control"));
        arrow_ctrl = plane.GetComponentInChildren<Arrow_Ctrl>();
        arrow_ctrl.Activate(false);
        surface = new Surface(position: Vector3.zero, normal: Vector3.up);

        // Action event Binding
        arrow_ctrl.OnDragged += calculate;
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

            plane.transform.SetPositionAndRotation(position, Quaternion.FromToRotation(Vector3.up, normal));

            surface = new Surface(position: position, normal: normal);
        }
    }

    void ConfirmSurface()
    {
        if (GUI.Button(new Rect(Screen.width - 120, Screen.height / 2, 100, 50), "Confirm\nSurface"))
        {
            confirm = true;
            arrow_ctrl.Activate(true);
        }
    }

    void ConfirmDepth()
    {
        if (GUI.Button(new Rect(Screen.width - 120, Screen.height / 2, 100, 50), "Confirm\nDepth"))
        {
            depth = true;
            surface.updatePosition(depth_Value);
            arrow_ctrl.OnDragged -= calculate;
            Destroy(plane);

            OnSurfaceBuilt(this, surface);
        }
    }

    void showDepth()
    {
        string depth_String = System.String.Format("{0}", depth_Value);
        depth_String = GUI.TextField(new Rect(Screen.width - 120, Screen.height / 2 - 100, 100, 30), depth_String, 5);
    }

    void calculate(object sender, Vector3 position)
    {
        float distance = Vector3.Distance(surface.getPosition(), position);

        if (!((position - surface.getPosition()).normalized == surface.getNormal().normalized))
        {
            plane.transform.SetPositionAndRotation(position, Quaternion.FromToRotation(Vector3.up, -1 * surface.getNormal()));
            surface.flipSide();
        }

        depth_Value = distance;
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (!confirm)
        {
            if (e.button == 0 && e.isMouse) getSurface();
            ConfirmSurface();
        }
        else if (!depth)
        {
            showDepth();
            ConfirmDepth();
        }
    }
}
