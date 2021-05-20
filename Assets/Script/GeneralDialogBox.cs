using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GeneralDialogBox : DialogBox
{
    public GameObject Name;
    public GameObject Description;

    public GameObject Image_Button;
    public GameObject Text_Button;
    public GameObject Option_Box;

    private InputField DefectName;
    private InputField DefectDescription;

    private Button ImageBtn;
    private Button TextBtn;
    private Dropdown OptionBox;

    private string defectName;
    private string defectDescription;

    // Start is called before the first frame update
    void Start()
    {
        DefectName = getField(Name);
        DefectDescription = getField(Description);

        ImageBtn = getButton(Image_Button);
        TextBtn = getButton(Text_Button);
        OptionBox = getDropDown(Option_Box);

        // Map in Actions to the UI elements
        DefectName.onValueChanged.AddListener(delegate { writeData(DefectName.text, out defectName); });
        DefectDescription.onValueChanged.AddListener(delegate { writeData(DefectDescription.text, out defectDescription); });

        // Map in Actions to the UI elements
        ImageBtn.onClick.AddListener(setImage);
        TextBtn.onClick.AddListener(setText);

        // Clear the old options of the Dropdown menu
        OptionBox.ClearOptions();
        // Add the options created in the List above
        OptionBox.AddOptions(DamageModel.DamageTypes);
        // Listen to DropDown options
        OptionBox.onValueChanged.AddListener(delegate { setDefectType(); });

        ImageBtn.interactable = false;
        TextBtn.interactable = false;
    }

    void setImage()
    {
        writeDataIn();
        DoneOperation(DimNotification.SetImageType);
    }

    void setText()
    {
        writeDataIn();
        (_DamageInstance as DamageInstance).setText();
        DoneOperation(DimNotification.SetTextType);
    }

    void setDefectType()
    {
        var DefectTypes = System.Enum.GetValues(typeof(DamageTypes))
            .Cast<DamageTypes>()
            .ToArray();

        var DefectType = DefectTypes[OptionBox.value];
        (_DamageInstance as DamageInstance).DefectType = DefectType;
    }

    protected override void writeData(string input, out string variable)
    {
        variable = input;
        checkStatus();
    }

    void writeDataIn()
    {
       if (this.defectName != null)  _DamageInstance.Name = this.defectName;
       if (this.defectDescription != null) _DamageInstance.Description = this.defectDescription;
    }

    void checkStatus()
    {
        if (defectName != null)
        {
            ImageBtn.interactable = (defectName.Length > 0);
            TextBtn.interactable = (defectName.Length > 0);
        } 
        else
        {
            ImageBtn.interactable = false;
            TextBtn.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
