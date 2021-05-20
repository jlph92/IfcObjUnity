using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EditTextDialogBox : DialogBox
{
    public GameObject Name;
    public GameObject Description;
    public GameObject Option_Box;

    public GameObject Add_Button;
    public GameObject Remove_Button;
    public GameObject Finish_Button;
    public GameObject PropertyContainer;

    // UI Element
    private InputField DefectName;
    private InputField DefectDescription;

    private Button Add_Btn;
    private Button Remove_Btn;
    private Button Finish_Btn;
    private ScrollViewContent scrollViewItem;
    private Dropdown OptionBox;

    // Internal Parameters
    private string defectName;
    private string defectDescription;

    // Start is called before the first frame update
    void Start()
    {
        Add_Btn = getButton(Add_Button);
        Remove_Btn = getButton(Remove_Button);
        Finish_Btn = getButton(Finish_Button);

        scrollViewItem = getScrollView(PropertyContainer);

        Add_Btn.onClick.AddListener(AddText);
        Remove_Btn.onClick.AddListener(RemoveText);
        Finish_Btn.onClick.AddListener(FinishOperation);

        Finish_Btn.interactable = false;

        // Map in Actions to the UI elements
        if (DefectName != null) DefectName.onValueChanged.AddListener(delegate { writeData(DefectName.text, out defectName); });
        if (DefectDescription != null) DefectDescription.onValueChanged.AddListener(delegate { writeData(DefectDescription.text, out defectDescription); });

        if (_DamageInstance != null)
        {
            var _DamageProperties = _DamageInstance.Properties;

            if (_DamageProperties != null)
            {
                if (_DamageProperties.Count > 0)
                {
                    foreach (var _DamageProperty in _DamageProperties)
                        Debug.LogFormat("Damage Property: {0}", _DamageProperty.ToString());

                    scrollViewItem.AddList(_DamageInstance);
                    Finish_Btn.interactable = true;
                }
            }
        }
    }

    public override void assignBox(DamageModel _DamageInstance, DimView _DamageGUI)
    {
        Debug.Log("Assign Edit Box");

        this._DamageInstance = _DamageInstance;
        this._DamageGUI = _DamageGUI;

        if (ProductLabel != null) writeLabel(_DamageInstance.ParentName);
        if (Cancel_Button != null)
        {
            CancelBtn = getButton(Cancel_Button);
            CancelBtn.onClick.AddListener(abortOperation);
        }

        defectName = _DamageInstance.Name;
        defectDescription = _DamageInstance.Description;

        writeExistingData();
    }

    void writeExistingData()
    {
        DefectName = getField(Name);
        DefectDescription = getField(Description);
        OptionBox = getDropDown(Option_Box);


        if (DefectName != null) DefectName.text = defectName;
        if (DefectDescription != null) DefectDescription.text = defectDescription;

        if (OptionBox != null)
        {
            // Clear the old options of the Dropdown menu
            OptionBox.ClearOptions();

            // Add the options created in the List above
            OptionBox.AddOptions(DamageModel.DamageTypes);

            Debug.LogFormat("Current Selection: {0}", _DamageInstance.DefectTypeOption);
            // Write Option value to current value
            OptionBox.value = _DamageInstance.DefectTypeOption;
        }
    }

    void writeDataIn()
    {
        if (this.defectName != null)
        {
            if (string.Compare(_DamageInstance.Name, this.defectName) != 1)
                _DamageInstance.Name = this.defectName;
        }

        if (this.defectDescription != null)
        {
            if (string.Compare(_DamageInstance.Description, this.defectDescription) != 1)
                _DamageInstance.Description = this.defectDescription;
        }
    }

    void setDefectType()
    {
        var DefectTypes = System.Enum.GetValues(typeof(DamageTypes))
            .Cast<DamageTypes>()
            .ToArray();

        var DefectType = DefectTypes[OptionBox.value];

        if (_DamageInstance.DefectType != DefectType)
            _DamageInstance.DefectType = DefectType;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AddText()
    {
        DoneOperation(DimNotification.Next_EditTextOperation);
    }

    void RemoveText()
    {

    }

    void FinishOperation()
    {
        writeDataIn();
        setDefectType();

        // Finisg Edit the Entity
        DoneOperation(DimNotification.Finish_EditOperation);
    }
}
