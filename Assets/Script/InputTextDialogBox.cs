using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputTextDialogBox : DialogBox
{
    public GameObject Name;
    public GameObject Value;
    public GameObject Option;
    public GameObject Done_Button;

    private InputField PropertyName;
    private InputField PropertyValue;

    private Button Done_Btn;
    private Dropdown OptionBox;

    private string _PropertyName;
    private string _PropertyValue;
    private System.Type _IfcValueType;

    // Start is called before the first frame update
    void Start()
    {
        PropertyName = getField(Name);
        PropertyValue = getField(Value);

        Done_Btn = getButton(Done_Button);
        OptionBox = getDropDown(Option);

        // Map in Actions to the UI elements
        PropertyName.onValueChanged.AddListener(delegate { writeData(PropertyName.text, out _PropertyName); });
        PropertyValue.onValueChanged.AddListener(delegate { writeData(PropertyValue.text, out _PropertyValue); });

        // Map in Actions to the UI elements
        Done_Btn.onClick.AddListener(back);

        // Clear the old options of the Dropdown menu
        OptionBox.ClearOptions();
        // Add the options created in the List above
        OptionBox.AddOptions(DamageModel.IfcValueTypes);
        // Listen to DropDown options
        OptionBox.onValueChanged.AddListener(delegate { setIfcType(); });

        Done_Btn.interactable = false;
        _IfcValueType = DamageModel.IfcValueType(0);
    }

    void back()
    {
        var newProperty = new PropertyItem
        {
            Name = _PropertyName,
            Value = _PropertyValue,
            IfcValueType = _IfcValueType
        };

        _DamageInstance.AddProperty(newProperty);

        // Retrun to Properties Box
        DoneOperation(DimNotification.Back_Operation);
    }

    protected override void writeData(string input, out string variable)
    {
        variable = input;
        checkStatus();
    }

    void setIfcType()
    {
        _IfcValueType = DamageModel.IfcValueType(OptionBox.value);
    }

    void checkStatus()
    {
        Debug.Log("Called checked status");
        if (_PropertyName != null && _PropertyValue != null)
        {
            Debug.Log("Show Done Button");
            Done_Btn.interactable = (_PropertyName.Length > 0 && _PropertyValue.Length > 0);
        }
        else
        {
            Done_Btn.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
