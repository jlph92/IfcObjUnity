using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneOrientationDialogBox : DialogBox
{
    public GameObject X_Axis_Slider;
    public GameObject Y_Axis_Slider;
    public GameObject Z_Axis_Slider;

    public GameObject X_Axis_Value;
    public GameObject Y_Axis_Value;
    public GameObject Z_Axis_Value;

    public GameObject Done_Button;

    public GameObject ImagePlane;

    // UI Elements
    private Slider X_Axis_slider;
    private Slider Y_Axis_slider;
    private Slider Z_Axis_slider;

    private InputField X_Axis_Input;
    private InputField Y_Axis_Input;
    private InputField Z_Axis_Input;

    private Button Done_Btn;

    // Internal Parameter
    private GameObject PlaneObject;
    private Quaternion PlaneRotation;
    private Vector3 planeCrossPostion;

    // Start is called before the first frame update
    void Start()
    {
        // Extract Slider Elements
        X_Axis_slider = getSlider(X_Axis_Slider);
        Y_Axis_slider = getSlider(Y_Axis_Slider);
        Z_Axis_slider = getSlider(Z_Axis_Slider);

        // Extract Input Elements
        X_Axis_Input = getField(X_Axis_Value);
        Y_Axis_Input = getField(Y_Axis_Value);
        Z_Axis_Input = getField(Z_Axis_Value);

        // Assign Buttons
        Done_Btn = getButton(Done_Button);

        Debug.LogFormat("Read Vector: {0}", _DamageInstance.ImageOrigin);
        PlaneObject = Instantiate(ImagePlane);
        planeCrossPostion = _DamageInstance.ImageOrigin;
        PlaneObject.transform.position = planeCrossPostion;
        PlaneRotation = PlaneObject.transform.rotation;

        var rotationVector = PlaneRotation.eulerAngles;
        X_Axis_slider.value = rotationVector.x;
        Y_Axis_slider.value = rotationVector.y;
        Z_Axis_slider.value = rotationVector.z;

        // Link slider action
        X_Axis_slider.onValueChanged.AddListener(delegate { Update_X_SliderValue(); });
        Y_Axis_slider.onValueChanged.AddListener(delegate { Update_Y_SliderValue(); });
        Z_Axis_slider.onValueChanged.AddListener(delegate { Update_Z_SliderValue(); });

        // Link input text action
        X_Axis_Input.onEndEdit.AddListener(delegate { Update_X_InputValue(); });
        Y_Axis_Input.onEndEdit.AddListener(delegate { Update_Y_InputValue(); });
        Z_Axis_Input.onEndEdit.AddListener(delegate { Update_Z_InputValue(); });

        // Link Button action
        Done_Btn.onClick.AddListener(Done_Operation);

        Update_X_SliderValue();
        Update_Y_SliderValue();
        Update_Z_SliderValue();
    }

    void Done_Operation()
    {
        if (PlaneObject != null)
        {
            var imageRotation = PlaneObject.transform.rotation;
            _DamageInstance.ImageRotation = imageRotation;
        }

        DoneOperation(DimNotification.Next_PlaneOrienatationOperation);
    }

    void SectionClip()
    {
        var attachedObject = _DamageInstance.AttachedObject;

        if (attachedObject != null)
        {

        }
    }

    // Update Input Text
    void Update_X_SliderValue()
    {
        var x_value = System.String.Format("{0}", X_Axis_slider.value);
        X_Axis_Input.text = x_value;

        updateRotation();
    }

    void Update_Y_SliderValue()
    {
        var y_value = System.String.Format("{0}", Y_Axis_slider.value);
        Y_Axis_Input.text = y_value;

        updateRotation();
    }

    void Update_Z_SliderValue()
    {
        var z_value = System.String.Format("{0}", Z_Axis_slider.value);
        Z_Axis_Input.text = z_value;

        updateRotation();
    }

    void Update_X_InputValue()
    {
        var x_value = System.Single.Parse(X_Axis_Input.text);
        X_Axis_slider.value = x_value;

        updateRotation();
    }

    void Update_Y_InputValue()
    {
        var y_value = System.Single.Parse(Y_Axis_Input.text);
        Y_Axis_slider.value = y_value;

        updateRotation();
    }

    void Update_Z_InputValue()
    {
        var z_value = System.Single.Parse(Z_Axis_Input.text);
        Z_Axis_slider.value = z_value;

        updateRotation();
    }

    void updateRotation()
    {
        var newRotation = new Vector3(X_Axis_slider.value, Y_Axis_slider.value, Z_Axis_slider.value);
        PlaneRotation.eulerAngles = newRotation;
        PlaneObject.transform.rotation = PlaneRotation;
    }
}
