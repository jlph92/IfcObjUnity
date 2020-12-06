using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;

public class STLManipulator : MonoBehaviour
{
    public CoordinateSpace coordinateSpace;
    public UpAxis upAxis = UpAxis.Y;

    private CoordinateSpace InternalCoordinateSpace;
    private UpAxis InternalUpAxis;

    private System.Action ValueCoordinateSpaceChanged;
    private System.Action ValueUpAxisChanged;

    protected virtual void onValueCoordinateSpaceChanged() => ValueCoordinateSpaceChanged?.Invoke();
    protected virtual void onValueUpAxisChanged() => ValueUpAxisChanged?.Invoke();

    // Start is called before the first frame update
    void Start()
    {
        ValueCoordinateSpaceChanged += () => { flip(); };
        ValueUpAxisChanged += () => { updateAxis(); };
    }

    [EasyButtons.Button]
    void apply()
    {
        setCoordinateSpace = coordinateSpace;
        setUpAxis = upAxis;
    }

    void flip()
    {

    }

    void updateAxis()
    {
        Vector3 EulerAngles = Vector3.zero;
        
        switch (setUpAxis)
        {
            case UpAxis.X:
                EulerAngles = new Vector3( 0.0f, -90.0f, -90.0f);
                break;
            case UpAxis.Y:
                EulerAngles = Vector3.zero;
                break;
            case UpAxis.Z:
                EulerAngles = new Vector3( -90.0f, 90.0f, 0.0f);
                break;
            default:
                EulerAngles = Vector3.zero;
                break;
        }

        Quaternion Rotation = Quaternion.identity;
        Rotation.eulerAngles = EulerAngles;
        transform.rotation = Rotation;
    }

    private CoordinateSpace setCoordinateSpace
    {
        get => InternalCoordinateSpace;

        set
        {
            onValueCoordinateSpaceChanged();
            InternalCoordinateSpace = value;
        }
    }

    private UpAxis setUpAxis
    {
        get => InternalUpAxis;

        set
        {
            onValueUpAxisChanged();
            InternalUpAxis = value;
        }
    }
}
