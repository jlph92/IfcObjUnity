using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xbim.Ifc4.Interfaces;
using System.Linq;


public class Damage
{
    // Kill switch
    public event System.EventHandler AbortionCalled; // event
    public event System.EventHandler EndCalled; // event

    protected virtual void OnAbortionCalled(System.EventArgs e) //protected virtual method
    {
        AbortionCalled?.Invoke(this, e);
    }

    protected virtual void OnEndCalled(System.EventArgs e) //protected virtual method
    {
        EndCalled?.Invoke(this, e);
    }

    // Semantic options
    bool Step_file;
    bool Measurement;

    // Proxy Set?
    bool Proxy_Set = false;

    // Measurement Set?
    bool Measurement_Set = false;

    // Property Set?
    bool Property_Set = false;

    private GameObject DialogBox;

    public GameObject getGameObject() { return DialogBox; }

    private Dictionary<string, GameObject> targetField = new Dictionary<string, GameObject>();

    public Damage(GameObject DialogBox, int attachedProduct)
    {
        this.DialogBox = DialogBox;
        this.attachedProduct = attachedProduct;
        if (this.DialogBox != null) setUpTargetField();
        setProxy();
    }

    void setUpTargetField()
    {
        Transform ElementsList = DialogBox.transform.Find("Elements");

        foreach (Transform child in ElementsList)
        {
            targetField.Add(child.gameObject.name, child.gameObject);
        }

        Transform opt = ElementsList.Find("Options");

        foreach (Transform child in opt)
        {
            targetField.Add(child.gameObject.name, child.gameObject);
        }

        Transform list = ElementsList.Find("ListOptions");

        foreach (Transform child in list)
        {
            targetField.Add(child.gameObject.name, child.gameObject);
        }
    }

    void setProxy()
    {
        ActivateObj(true); //Activate all UI Elements
        targetField["InputField-3"].SetActive(false);
        targetField["ScrollView"].SetActive(false);
        targetField["ListOptions"].SetActive(false);

        // Set Title
        getText(targetField["Title"]).text = "IfcProxy";

        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input2 = getField(targetField["InputField-2"]);
        Toggle step_option = getToggle(targetField["Toggle-STEP"]);
        Toggle mea_option = getToggle(targetField["Toggle-Measurement"]);
        Dropdown m_Dropdown = getDropDown(targetField["Dropdown"]);

        Button Btn_OK = getButton(targetField["OK"]);
        Button Btn_Cancel = getButton(targetField["Cancel"]);

        // Insert Damage Type
        setProxyType(targetField["Dropdown"]);

        // Set button label
        changeBtnText(targetField["OK"], "Next");

        // Map in Actions to the UI elements
        input1.onEndEdit.AddListener(delegate { writeData(input1, out Proxy_Name); });
        input2.onEndEdit.AddListener(delegate { writeData(input2, out Proxy_Description); });
        writeOption(step_option, out Step_file);
        writeOption(mea_option, out Measurement);
        step_option.onValueChanged.AddListener(delegate { writeOption(step_option, out Step_file); });
        mea_option.onValueChanged.AddListener(delegate { writeOption(mea_option, out Measurement); });
        m_Dropdown.onValueChanged.AddListener(delegate { writeType(m_Dropdown.value, out Damage_Type); });

        Btn_OK.onClick.AddListener(nextSequence);
        Btn_Cancel.onClick.AddListener(cancelSequence);
    }

    void setMeasurement()
    {
        //Proxy status filled
        Proxy_Set = true;

        //Measurement status filled
        Measurement_Set = false;

        ActivateObj(false);  //Deactivate all UI Elements
        targetField["Title"].SetActive(true);
        targetField["ScrollView"].SetActive(true);
        targetField["ListOptions"].SetActive(true);
        targetField["Add"].SetActive(true);
        targetField["Remove"].SetActive(true);
        targetField["Options"].SetActive(true);
        targetField["OK"].SetActive(true);
        targetField["Cancel"].SetActive(true);

        // Set Title
        getText(targetField["Title"]).text = "Measurement";

        // Set button label
        if (Measurements.Count > 0) changeBtnText(targetField["OK"], "Finish");

        // Map in variables for the UI elements
        // ScrollViewContent scrollViewItem = getScrollView(targetField["ScrollView"]);
        Button Btn_Add = getButton(targetField["Add"]);
        Button Btn_Remove = getButton(targetField["Remove"]);

        Button Btn_OK = getButton(targetField["OK"]);

        // Map in Actions to the UI elements
        Btn_Add.onClick.AddListener(AddMeasurement);
        Btn_Remove.onClick.AddListener(RemoveMeasurement);
        Btn_OK.onClick.AddListener(finishSequence);
    }

    void setDamageProperty()
    {
        Debug.Log("Damage property is called.");

        //Measurement status filled
        Measurement_Set = true;

        ActivateObj(false);  //Deactivate all UI Elements
        targetField["Title"].SetActive(true);
        targetField["InputField-1"].SetActive(true);
        targetField["Dropdown"].SetActive(true);
        targetField["InputField-3"].SetActive(true);
        targetField["Options"].SetActive(true);
        //targetField["OK"].SetActive(true);
        targetField["Cancel"].SetActive(true);

        // Set Title
        getText(targetField["Title"]).text = "Damage Property";

        // Create a new Damage Property
        dmgProp = new DamageProperty();

        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input3 = getField(targetField["InputField-3"]);
        Dropdown m_Dropdown = getDropDown(targetField["Dropdown"]);
        Button Btn_OK = getButton(targetField["OK"]);

        // Set button label
        changeBtnText(targetField["OK"], "Add");

        // Set Unit Type
        setUnitType(targetField["Dropdown"]);

        // Map in Actions to the UI elements
        input1.onEndEdit.AddListener(delegate { writePropertyName(input1); });
        input3.onEndEdit.AddListener(delegate { writePropertyValue(input3); });
        m_Dropdown.onValueChanged.AddListener(delegate { writePropertyUnit(m_Dropdown.value); });
        Btn_OK.onClick.AddListener(backSequence);

        // Lock the Ok button with certain condition
        dmgProp.dataCompleted += showAddButton;
        dmgProp.dataImcomplete += hideAddButton;
    }

    void deRegisterProxy()
    {
        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        Dropdown m_Dropdown = getDropDown(targetField["Dropdown"]);
        Button Btn_OK = getButton(targetField["OK"]);

        // DeRegistered Action from Event
        input1.onEndEdit.RemoveListener(delegate { writeData(input1, out Proxy_Name); });
        m_Dropdown.onValueChanged.RemoveListener(delegate { writeType(m_Dropdown.value, out Damage_Type); });
        Btn_OK.onClick.RemoveListener(nextSequence);
    }

    void deRegisterMeasurement()
    {
        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input3 = getField(targetField["InputField-3"]);
        Button Btn_Add = getButton(targetField["Add"]);
        Button Btn_Remove = getButton(targetField["Remove"]);

        Button Btn_OK = getButton(targetField["OK"]);

        // De-map in Actions to the UI elements
        Btn_Add.onClick.RemoveListener(AddMeasurement);
        Btn_Remove.onClick.RemoveListener(RemoveMeasurement);
        Btn_OK.onClick.RemoveListener(finishSequence);

        // Clear text field
        input1.text = "";
        input3.text = "";
    }

    void deRegisterDamageProperty()
    {
        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input3 = getField(targetField["InputField-3"]);
        Dropdown m_Dropdown = getDropDown(targetField["Dropdown"]);
        Button Btn_OK = getButton(targetField["OK"]);

        // De-map in Actions to the UI elements
        input1.onEndEdit.RemoveListener(delegate { writePropertyName(input1); });
        input3.onEndEdit.RemoveListener(delegate { writePropertyValue(input3); });
        m_Dropdown.onValueChanged.RemoveListener(delegate { writePropertyUnit(m_Dropdown.value); });
        Btn_OK.onClick.RemoveListener(backSequence);

        // Deregister of button Action
        dmgProp.dataCompleted -= showAddButton;
        dmgProp.dataImcomplete -= hideAddButton;
    }

    void updateList()
    {
        // Map in variables for the UI elements
        ScrollViewContent scrollViewItem = getScrollView(targetField["ScrollView"]);
        //scrollViewItem.clearList();

        foreach (DamageProperty dmgProp in Measurements)
        {
            string contentText = dmgProp.getDataText();
            scrollViewItem.Add(dmgProp, contentText);
        }

        //Debug.Log(Measurements.Count);

        //foreach (DamageProperty dmgProp in Measurements)
        //{
        //    string contentText = dmgProp.getDataText();
        //    Debug.Log(contentText);
        //}

        setMeasurement();
    }

    void showAddButton(object sender, System.EventArgs e)
    {
        targetField["OK"].SetActive(true);
    }

    void hideAddButton(object sender, System.EventArgs e)
    {
        targetField["OK"].SetActive(false);
    }

    void AddMeasurement()
    {
        deRegisterMeasurement();
        if (!Measurement_Set) setDamageProperty();
    }

    void RemoveMeasurement()
    {
        // Map in variables for the UI elements
        ScrollViewContent scrollViewItem = getScrollView(targetField["ScrollView"]);
        foreach (DamageProperty dmgProp in scrollViewItem.ToRemove())
        {
            Measurements.Remove(dmgProp);
        }
    }

    void backSequence()
    {
        Debug.Log("called the Back");
        deRegisterDamageProperty();
        //Debug.LogFormat("Size:{0}", Measurements.Count);
        Measurements.Add(this.dmgProp);
       // Debug.LogFormat("Size:{0}, {1} is added.", Measurements.Count, dmgProp.getDataText());
        updateList();
    }

    void nextSequence()
    {
        Debug.Log("called the Next");
        deRegisterProxy();
        if (Measurement)
        {
            if (!Proxy_Set) setMeasurement();
        }
    }

    void finishSequence()
    {
        OnEndCalled(System.EventArgs.Empty);
    }

    void cancelSequence()
    {
        OnAbortionCalled(System.EventArgs.Empty);
    }

    void writeUnit(int index, out System.Type type)
    {
        type = DamageProperty.getType(index);
    }

    void writeType(int index, out DamageTypes type)
    {
        type = (DamageTypes) index;
    }

    void writeOption(Toggle m_toggle, out bool option)
    {
        option = m_toggle.isOn;
    }

    void writeData(InputField input, out string variable)
    {
        variable = input.text;
    }

    void writePropertyName(InputField input)
    {
        this.dmgProp.Property_Name = input.text;
    }

    void writePropertyValue(InputField input)
    {
        this.dmgProp.Property_Value = input.text;
    }

    void writePropertyUnit(int index)
    {
        this.dmgProp.selectedType = DamageProperty.getType(index);
    }

    void setUnitType(GameObject gmObj)
    {
        List<string> unitTypes = DamageProperty.getUnitType();

        //Fetch the Dropdown GameObject the script is attached to
        Dropdown m_Dropdown = getDropDown(gmObj);
        //Clear the old options of the Dropdown menu
        m_Dropdown.ClearOptions();
        //Add the options created in the List above
        m_Dropdown.AddOptions(unitTypes);
    }

    void setProxyType(GameObject gmObj)
    {
        List<string> damageTypes = System.Enum.GetValues(typeof(DamageTypes))
                                        .Cast<DamageTypes>()
                                        .Select(d => (d.ToString()))
                                        .ToList();

        //Fetch the Dropdown GameObject the script is attached to
        Dropdown m_Dropdown = getDropDown(gmObj);
        //Clear the old options of the Dropdown menu
        m_Dropdown.ClearOptions();
        //Add the options created in the List above
        m_Dropdown.AddOptions(damageTypes);
    }

    void ActivateObj(bool active)
    {
        List<GameObject> allObjects = getAllObjects();

        foreach (GameObject gmObj in allObjects)
            gmObj.SetActive(active);
    }

    List<GameObject> getAllObjects()
    {
        return targetField.Select(d => d.Value).ToList();
    }

    void changeBtnText(GameObject gmObj, string label)
    {
        gmObj.GetComponentInChildren<Text>().text = label;
    }

    ScrollViewContent getScrollView(GameObject gmObj)
    {
        return gmObj.GetComponent<ScrollViewContent>();
    }

    Button getButton(GameObject gmObj)
    {
        return gmObj.GetComponent<Button>();
    }

    Toggle getToggle(GameObject gmObj)
    {
        return gmObj.GetComponent<Toggle>();
    }

    Dropdown getDropDown(GameObject gmObj)
    {
        return gmObj.GetComponent<Dropdown>();
    }

    Text getText(GameObject gmObj)
    {
        return gmObj.GetComponent<Text>();
    }

    InputField getField(GameObject gmObj)
    {
        return gmObj.GetComponent<InputField>();
    }

    public string getProxyName()
    {
        return Proxy_Name;
    }

    public string getProxyDescription()
    {
        return Proxy_Description;
    }

    public string getDamageType()
    {
        return Damage_Type.ToString("G");
    }

    public int getProductLabel()
    {
        return attachedProduct;
    }

    public IEnumerable<DamageProperty> getMeasurements()
    {
        return Measurements;
    }

    // Proxy item
    string Proxy_Name;
    // Damage type
    DamageTypes Damage_Type;
    // Store Entity Label
    int Attached_Product;
    // Proxy Description
    string Proxy_Description;
    // Attached Porduct
    int attachedProduct;

    // Property data
    DamageProperty dmgProp;

    // The storage data
    string Step_URL;
    List<DamageProperty> Measurements = new List<DamageProperty>();
}

enum DamageTypes
{
    Crack,
    Spalling,
    Rusting,
    Decolorisation,
    Vegetation
}
