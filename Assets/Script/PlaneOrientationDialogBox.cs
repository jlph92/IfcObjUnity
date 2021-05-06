using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneOrientationDialogBox : DialogBox
{
    public GameObject X_Pos_Slider;
    public GameObject Y_Pos_Slider;
    public GameObject Z_Pos_Slider;

    public GameObject X_Axis_Slider;
    public GameObject Y_Axis_Slider;
    public GameObject Z_Axis_Slider;

    public GameObject X_Pos_Value;
    public GameObject Y_Pos_Value;
    public GameObject Z_Pos_Value;

    public GameObject X_Axis_Value;
    public GameObject Y_Axis_Value;
    public GameObject Z_Axis_Value;

    public GameObject Done_Button;

    public GameObject ImagePlane;

    public Material ClippingMaterial;

    // UI Elements
    private Slider X_Pos_slider;
    private Slider Y_Pos_slider;
    private Slider Z_Pos_slider;

    private Slider X_Axis_slider;
    private Slider Y_Axis_slider;
    private Slider Z_Axis_slider;

    private InputField X_Pos_Input;
    private InputField Y_Pos_Input;
    private InputField Z_Pos_Input;

    private InputField X_Axis_Input;
    private InputField Y_Axis_Input;
    private InputField Z_Axis_Input;

    private Button Done_Btn;

    // Internal Parameter
    private GameObject PlaneObject;
    private GameObject searchObject;
    private GameObject cloneObject;
    private GameObject Quad;
    private Vector3 Centre;
    private Vector3 BoundSize;

    private Quaternion PlaneRotation;
    private Vector3 planeCrossPostion = Vector3.zero;

    private Material originalMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // Extract Slider Elements
        X_Pos_slider = getSlider(X_Pos_Slider);
        Y_Pos_slider = getSlider(Y_Pos_Slider);
        Z_Pos_slider = getSlider(Z_Pos_Slider);

        X_Axis_slider = getSlider(X_Axis_Slider);
        Y_Axis_slider = getSlider(Y_Axis_Slider);
        Z_Axis_slider = getSlider(Z_Axis_Slider);

        // Extract Input Elements
        X_Pos_Input = getField(X_Pos_Value);
        Y_Pos_Input = getField(Y_Pos_Value);
        Z_Pos_Input = getField(Z_Pos_Value);

        X_Axis_Input = getField(X_Axis_Value);
        Y_Axis_Input = getField(Y_Axis_Value);
        Z_Axis_Input = getField(Z_Axis_Value);

        // Assign Buttons
        Done_Btn = getButton(Done_Button);

        Debug.LogFormat("Read Vector: {0}", _DamageInstance.ImageOrigin);
        PlaneObject = Instantiate(ImagePlane);
        Quad = PlaneObject.transform.GetChild(0).gameObject;

        PlaneRotation = Quad.transform.rotation;

        X_Pos_slider.value = planeCrossPostion.x;
        X_Pos_slider.value = planeCrossPostion.x;
        X_Pos_slider.value = planeCrossPostion.x;

        var rotationVector = PlaneRotation.eulerAngles;
        X_Axis_slider.value = rotationVector.x;
        Y_Axis_slider.value = rotationVector.y;
        Z_Axis_slider.value = rotationVector.z;

        // Link slider action
        X_Pos_slider.onValueChanged.AddListener(delegate { Update_X_Pos_SliderValue(); });
        Y_Pos_slider.onValueChanged.AddListener(delegate { Update_Y_Pos_SliderValue(); });
        Z_Pos_slider.onValueChanged.AddListener(delegate { Update_Z_Pos_SliderValue(); });

        X_Axis_slider.onValueChanged.AddListener(delegate { Update_X_SliderValue(); });
        Y_Axis_slider.onValueChanged.AddListener(delegate { Update_Y_SliderValue(); });
        Z_Axis_slider.onValueChanged.AddListener(delegate { Update_Z_SliderValue(); });

        // Link input text action
        X_Pos_Input.onEndEdit.AddListener(delegate { Update_X_Pos_InputValue(); });
        Y_Pos_Input.onEndEdit.AddListener(delegate { Update_Y_Pos_InputValue(); });
        Z_Pos_Input.onEndEdit.AddListener(delegate { Update_Z_Pos_InputValue(); });

        X_Axis_Input.onEndEdit.AddListener(delegate { Update_X_InputValue(); });
        Y_Axis_Input.onEndEdit.AddListener(delegate { Update_Y_InputValue(); });
        Z_Axis_Input.onEndEdit.AddListener(delegate { Update_Z_InputValue(); });

        // Link Button action
        Done_Btn.onClick.AddListener(Done_Operation);

        Update_X_Pos_SliderValue();
        Update_Y_Pos_SliderValue();
        Update_Z_Pos_SliderValue();

        Update_X_SliderValue();
        Update_Y_SliderValue();
        Update_Z_SliderValue();

        SectionClip();
    }

    void Done_Operation()
    {
        if (PlaneObject != null)
        {
            _DamageInstance.ImageOrigin = planeCrossPostion + Centre;
            _DamageInstance.ImageRotation = PlaneRotation;
        }

        Destroy(PlaneObject);
        if (searchObject != null) searchObject.SetActive(true);
        if (cloneObject != null)
        {
            _DamageInstance.ImageObject = cloneObject;
            cloneObject.SetActive(false);
        }

        DoneOperation(DimNotification.Next_PlaneOrienatationOperation);
    }

    void SectionClip()
    {
        UnFreezeScreen();

        var attachedObject = _DamageInstance.AttachedObject;
        
        if (attachedObject != null)
        {
            var searchName = string.Format("{0}(Damage View)", attachedObject.name);
            searchObject = GameObject.Find(searchName);

            if (searchObject != null)
            {
                cloneObject = Instantiate(searchObject);
                searchObject.SetActive(false);
                var m_Renderer = cloneObject.GetComponent<Renderer>();
                originalMaterial = m_Renderer.material;

                m_Renderer.material = ClippingMaterial;

                var bound = m_Renderer.bounds;
                Centre = bound.center;

                X_Pos_slider.maxValue = bound.extents.x;
                Y_Pos_slider.maxValue = bound.extents.y;
                Z_Pos_slider.maxValue = bound.extents.z;

                X_Pos_slider.minValue = bound.extents.x * -1.0f;
                Y_Pos_slider.minValue = bound.extents.y * -1.0f;
                Z_Pos_slider.minValue = bound.extents.z * -1.0f;

                PlaneObject.transform.position = planeCrossPostion + Centre;
            }
        }
    }

    void UnFreezeScreen()
    {
        if (_DamageGUI != null)
            (_DamageGUI as DamageGUI).deFreezeScreen();
    }

    // Update Input Text
    void Update_X_Pos_SliderValue()
    {
        var x_value = System.String.Format("{0}", X_Pos_slider.value);
        Debug.Log(x_value);
        X_Pos_Input.text = x_value;

        updatePosition();
    }

    void Update_Y_Pos_SliderValue()
    {
        var y_value = System.String.Format("{0}", Y_Pos_slider.value);
        Y_Pos_Input.text = y_value;

        updatePosition();
    }

    void Update_Z_Pos_SliderValue()
    {
        var z_value = System.String.Format("{0}", Z_Pos_slider.value);
        Z_Pos_Input.text = z_value;

        updatePosition();
    }

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

    void Update_X_Pos_InputValue()
    {
        var x_value = System.Single.Parse(X_Pos_Input.text);
        X_Pos_slider.value = x_value;

        updatePosition();
    }

    void Update_Y_Pos_InputValue()
    {
        var y_value = System.Single.Parse(Y_Pos_Input.text);
        Y_Pos_slider.value = y_value;

        updatePosition();
    }

    void Update_Z_Pos_InputValue()
    {
        var z_value = System.Single.Parse(Z_Pos_Input.text);
        Z_Pos_slider.value = z_value;

        updatePosition();
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

    void updatePosition()
    {
        planeCrossPostion = new Vector3(X_Pos_slider.value, Y_Pos_slider.value, Z_Pos_slider.value);
        PlaneObject.transform.position = planeCrossPostion + Centre;
    }

    void updateRotation()
    {
        var newRotation = new Vector3(X_Axis_slider.value, Y_Axis_slider.value, Z_Axis_slider.value);
        PlaneRotation.eulerAngles = newRotation;
        Quad.transform.rotation = PlaneRotation;
    }
}
