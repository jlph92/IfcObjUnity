using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera3DPreview : MonoBehaviour
{
    public GameObject CenterPoint;
    public GameObject Origin;

    private GameObject Object3DItem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void insert3DObject(GameObject gmObj)
    {
        if (Object3DItem != null) Destroy(Object3DItem);

        Object3DItem = gmObj;
        Object3DItem.transform.SetParent(CenterPoint.transform, false);
        checkBound();
        //Debug.LogFormat("Origin: {0}", Origin.transform.localPosition);
        gmObj.transform.localPosition = Origin.transform.localPosition * -1.0f;
        Origin.transform.localPosition = Object3DItem.transform.localPosition;
    }

    private void checkBound()
    {
        Bounds bounds = new Bounds(Object3DItem.transform.position, Vector3.zero);
        Renderer[] renderers = Object3DItem.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        //Debug.LogFormat("Center of 3D Object: {0}", bounds.center);

        Origin.transform.position = bounds.center;
    }
}
