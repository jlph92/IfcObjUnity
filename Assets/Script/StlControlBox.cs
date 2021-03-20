using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StlControlBox : MonoBehaviour
{
    public GameObject RotateClockwiseButton;
    public GameObject RotateCounterClockwiseButton;

    public GameObject XAxis;
    public GameObject YAxis;
    public GameObject ZAxis;

    public GameObject DoneButton;

    private GameObject setPoint;
    private AxisTypes currentAxis;

    public delegate void endControlBox(object sender, Matrix4x4 matrix);

    public event endControlBox OnEndControlBox;

    void Start()
    {
        // This line need to change to selecteable.
        setPoint = GameObject.Find("setPoint");

        Button RotateCwButton = getButton(RotateClockwiseButton);
        Button RotateCcwButton = getButton(RotateCounterClockwiseButton);
        Button Done_Button = getButton(DoneButton);

        Toggle XAxis_option = getToggle(XAxis);
        Toggle YAxis_option = getToggle(YAxis);
        Toggle ZAxis_option = getToggle(ZAxis);

        RotateCwButton.onClick.AddListener(rotateClockwise);
        RotateCcwButton.onClick.AddListener(rotateCounterClockwise);
        Done_Button.onClick.AddListener(doneTransform);

        XAxis_option.onValueChanged.AddListener(delegate { writeOption(AxisTypes.X_axis); });
        YAxis_option.onValueChanged.AddListener(delegate { writeOption(AxisTypes.Y_axis); });
        ZAxis_option.onValueChanged.AddListener(delegate { writeOption(AxisTypes.Z_axis); });
    }

    //[EasyButtons.Button]
    void rotateClockwise()
    {
        switch (currentAxis)
        {
            case AxisTypes.X_axis:
                setPoint.transform.Rotate(90.0f, 0.0f, 0.0f, Space.Self);
                break;
            case AxisTypes.Y_axis:
                setPoint.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
                break;
            case AxisTypes.Z_axis:
                setPoint.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
                break;
        }
    }

    //[EasyButtons.Button]
    void rotateCounterClockwise()
    {
        switch (currentAxis)
        {
            case AxisTypes.X_axis:
                setPoint.transform.Rotate(-90.0f, 0.0f, 0.0f, Space.Self);
                break;
            case AxisTypes.Y_axis:
                setPoint.transform.Rotate(0.0f, -90.0f, 0.0f, Space.Self);
                break;
            case AxisTypes.Z_axis:
                setPoint.transform.Rotate(0.0f, 0.0f, -90.0f, Space.Self);
                break;
        }
    }

    void doneTransform()
    {
        Quaternion rotation = setPoint.transform.localRotation;
        Matrix4x4 m_rotation = Matrix4x4.Rotate(rotation);

        OnEndControlBox(this, m_rotation);
   
        Debug.Log("Control Box has been destroyed");
        Destroy(gameObject);
    }

    void writeOption(AxisTypes axis)
    {
        currentAxis = axis;
    }

    Button getButton(GameObject gmObj)
    {
        return gmObj.GetComponent<Button>();
    }

    Toggle getToggle(GameObject gmObj)
    {
        return gmObj.GetComponent<Toggle>();
    }
}

enum AxisTypes
{
    X_axis,
    Y_axis,
    Z_axis
}
