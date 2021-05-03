using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Xbim.Ifc4.Interfaces;
using System.Linq;
using SFB;


public class Damage
{
    // Kill switch
    public event System.EventHandler AbortionCalled; // event
    public event System.EventHandler EndCalled; // event
    public event System.EventHandler TriggerSurfaceBuilder; // event
    public event System.EventHandler EndExternalFile; // event

    // Action Pack
    private UnityAction<string> m_AssignProxyName;
    private UnityAction<string> m_AssignProxyDescription;
    private UnityAction<int> m_AssignDamageType;

    private UnityAction<string> m_AssignPropertyName;
    private UnityAction<string> m_AssignPropertyDescription;
    private UnityAction<int> m_AssignPropertyUnit;

    private UnityAction<string> m_AssignExternalFileName;
    private UnityAction<string> m_AssignExternalFileDescription;
    private UnityAction<int> m_AssignExternalFileType;
    private UnityAction m_AssignURL;

    // Store items going to be destroy after reference
    private List<GameObject> afterReference = new List<GameObject>();

    protected virtual void OnAbortionCalled(System.EventArgs e) //protected virtual method
    {
        AbortionCalled?.Invoke(this, e);
    }

    protected virtual void OnEndCalled(System.EventArgs e) //protected virtual method
    {
        EndCalled?.Invoke(this, e);
    }

    protected virtual void OnTriggerSurfaceBuilder(System.EventArgs e) //protected virtual method
    {
        TriggerSurfaceBuilder?.Invoke(this, e);
    }

    protected virtual void OnEndExternalFile(System.EventArgs e) //protected virtual method
    {
        EndExternalFile?.Invoke(this, e);
    }

    // Set the origin of external file
    public void SetOrigin(Vector3 Point)
    {
        externalFile.SetOrigin(Point);
    }

    public bool getRelativePlacement(out Vector3 Result)
    {
        return externalFile.getRelativePlacement(out Result);
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
    public GameObject surfaceBuilder;

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

        Transform url = ElementsList.Find("URLField");

        foreach (Transform child in url)
        {
            targetField.Add(child.gameObject.name, child.gameObject);
        }
    }

    void setProxy()
    {
        ActivateObj(true); //Activate all UI Elements
        targetField["URLField"].SetActive(false);
        targetField["InputField-3"].SetActive(false);
        targetField["ScrollView"].SetActive(false);
        targetField["ListOptions"].SetActive(false);

        // Set Title
        getText(targetField["Title"]).text = "IfcProxy";

        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input2 = getField(targetField["InputField-2"]);
        Toggle step_option = getToggle(targetField["Toggle-3D"]);
        Toggle mea_option = getToggle(targetField["Toggle-Measurement"]);
        Dropdown m_Dropdown = getDropDown(targetField["Dropdown"]);

        Button Btn_OK = getButton(targetField["OK"]);
        Button Btn_Cancel = getButton(targetField["Cancel"]);

        // Insert Damage Type
        setProxyType(targetField["Dropdown"]);

        // Set button label
        changeBtnText(targetField["OK"], "Next");

        // Map in the accurate Actions
        m_AssignProxyName = delegate { writeData(input1, out Proxy_Name); };
        m_AssignProxyDescription = delegate { writeData(input2, out Proxy_Description); };
        m_AssignDamageType = delegate { writeType(m_Dropdown.value, out Damage_Type); };

        // Map in Actions to the UI elements
        input1.onEndEdit.AddListener(m_AssignProxyName);
        input2.onEndEdit.AddListener(m_AssignProxyDescription);

        writeOption(step_option, out Step_file);
        writeOption(mea_option, out Measurement);

        step_option.onValueChanged.AddListener(delegate { writeOption(step_option, out Step_file); });
        mea_option.onValueChanged.AddListener(delegate { writeOption(mea_option, out Measurement); });
        m_Dropdown.onValueChanged.AddListener(m_AssignDamageType);

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

        // Map action with method
        m_AssignPropertyName = delegate { writePropertyName(input1); };
        m_AssignPropertyDescription = delegate { writePropertyValue(input3); };
        m_AssignPropertyUnit = delegate { writePropertyUnit(m_Dropdown.value); };

        // Map in Actions to the UI elements
        input1.onEndEdit.AddListener(m_AssignPropertyName);
        input3.onEndEdit.AddListener(m_AssignPropertyDescription);
        m_Dropdown.onValueChanged.AddListener(m_AssignPropertyUnit);
        Btn_OK.onClick.AddListener(backSequence);

        // Lock the Ok button with certain condition
        dmgProp.dataCompleted += showAddButton;
        dmgProp.dataImcomplete += hideAddButton;
    }

    void setStlFile()
    {
        deRegisterProxy();

        //Trigger the surface builder gameobject
        OnTriggerSurfaceBuilder(System.EventArgs.Empty);

        //Find out the coordinate for attachment
        setCoordinate();
    }

    void setCoordinate()
    {
        if (surfaceBuilder != null)
        {
            DialogBox.SetActive(false);

            //surfaceBuider sBuilder = surfaceBuilder.GetComponent<surfaceBuider>();
            //sBuilder.OnPointObtained += setProxyCoordinate;
            //sBuilder.plot3D();
        }
    }

    // Return in the 3D point
    void setProxyCoordinate(object sender, Vector3 intersectionPoint)
    {
        externalFile.SetPoint(intersectionPoint);
        //(sender as surfaceBuider).OnPointObtained -= setProxyCoordinate;
        DialogBox.SetActive(true);
        addExternalFileSequence();
    }

    void assignURL()
    {
        //Proxy status filled
        Proxy_Set = true;

        Debug.Log("The assign URL sequence is called.");

        ActivateObj(false);  //Deactivate all UI Elements
        targetField["Title"].SetActive(true);
        targetField["InputField-1"].SetActive(true);
        targetField["InputField-2"].SetActive(true);
        targetField["Dropdown"].SetActive(true);

        targetField["URLField"].SetActive(true);
        targetField["Background"].SetActive(true);
        targetField["Browse"].SetActive(true);

        targetField["Options"].SetActive(true);
        targetField["OK"].SetActive(true);
        targetField["Cancel"].SetActive(true);

        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input2 = getField(targetField["InputField-2"]);
        Dropdown m_Dropdown = getDropDown(targetField["Dropdown"]);
        Button Btn_Browse = getButton(targetField["Browse"]);
        Button Btn_OK = getButton(targetField["OK"]);

        // Map Unity Action with method
        m_AssignExternalFileName = delegate { writeExternalFileName(input1); };
        m_AssignExternalFileDescription = delegate { writeExternalFileDescription(input1); };
        m_AssignExternalFileType = delegate { writeExternalFileType(m_Dropdown.value); };

        // Set Title
        getText(targetField["Title"]).text = "Step File Attachment";

        // Insert Unit Type
        setExternalFileType(targetField["Dropdown"]);

        // Set button label
        changeBtnText(targetField["OK"], "Finish");  

        // Map in Actions to the UI elements
        input1.onEndEdit.AddListener(m_AssignExternalFileName);
        input2.onEndEdit.AddListener(m_AssignExternalFileDescription);
        m_Dropdown.onValueChanged.AddListener(m_AssignExternalFileType);

        Btn_Browse.onClick.AddListener(browseURL);
        Btn_OK.onClick.AddListener(finishSequence);
    }

    void browseURL()
    {
        var extensions = new[] {
            new ExtensionFilter("IFC files", "stl"),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open Settings File", "", extensions, false);
        string filePath = path[0];

        if (filePath.Length != 0)
        {
            //ExternalDocumentLoaderFactory.Create(filePath);

            targetField["Background"].GetComponentInChildren<Text>().text = filePath;
            //surfaceBuider sBuilder = surfaceBuilder.GetComponent<surfaceBuider>();
            //Transform objectPoint = sBuilder.Point3D.transform;
        }
    }

    void controlBoxCalled(object sender, GameObject controlBox)
    {
        DialogBox.SetActive(false);
        StlControlBox stlCtrlBox = controlBox.GetComponent<StlControlBox>();

        stlCtrlBox.OnEndControlBox += writeMatrix;
    }

    // need to write in data
    void writeMatrix(object sender, Matrix4x4 matrix)
    {
        DialogBox.SetActive(true);
        //stlDocData.writeData();
    }

    void deRegisterProxy()
    {
        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input2 = getField(targetField["InputField-2"]);
        Dropdown m_Dropdown = getDropDown(targetField["Dropdown"]);
        Button Btn_OK = getButton(targetField["OK"]);

        // DeRegistered Action from Event
        input1.onEndEdit.RemoveListener(m_AssignProxyName);
        input2.onEndEdit.RemoveListener(m_AssignProxyDescription);
        m_Dropdown.onValueChanged.RemoveListener(m_AssignDamageType);
        Btn_OK.onClick.RemoveListener(nextSequence);

        // Clear text field
        input1.text = "";
        input2.text = "";
    }

    void deRegisterMeasurement()
    {
        // Map in variables for the UI elements
        InputField input1 = getField(targetField["InputField-1"]);
        InputField input3 = getField(targetField["InputField-2"]);
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
        //// Map in variables for the UI elements
        //ScrollViewContent scrollViewItem = getScrollView(targetField["ScrollView"]);
        ////scrollViewItem.clearList();

        //foreach (DamageProperty dmgProp in Measurements)
        //{
        //    string contentText = dmgProp.getDataText();
        //    scrollViewItem.Add(dmgProp, contentText);
        //}

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

        //foreach (DamageProperty dmgProp in scrollViewItem.ToRemove())
        //{
        //    Measurements.Remove(dmgProp);
        //}
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
        if (!Proxy_Set)
        {
            if (Measurement) setMeasurement();
            else if (Step_file) setStlFile();
        }
    }

    void addExternalFileSequence()
    {
        Debug.Log("called the Add External File");
        
        if (!Proxy_Set)
        {
            if (Step_file) assignURL();
        }
    }

    void endExternalFileSequence()
    {
        //surfaceBuider sBuilder = surfaceBuilder.GetComponent<surfaceBuider>();
        //GameObject point = sBuilder.Point3D;
        GameObject point = new GameObject();
        GameObject sphere = point.transform.Find("Sphere").gameObject;
        GameObject arrow = GameObject.Find("Axis_Arrow(Clone)");

        afterReference.Add(surfaceBuilder);
        afterReference.Add(sphere);
        afterReference.Add(arrow);

        OnEndExternalFile(System.EventArgs.Empty);
    }

    void finishSequence()
    {
        if (externalFile != null) endExternalFileSequence();
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

    void writeExternalFileName(InputField input)
    {
        if (externalFile != null) externalFile.SetFileName(input.text);
    }

    void writeExternalFileDescription(InputField input)
    {
        if (externalFile != null) externalFile.SetFileDescription(input.text);
    }

    void writeExternalFileURL(string url)
    {
        if (externalFile != null) externalFile.SetURL(url);
    }

    void writeExternalFileType(int index)
    {
        if (externalFile != null) externalFile.SetUnit(index);
    }

    void setExternalFileType(GameObject gmObj)
    {
        List<string> unitTypes = ExternalFile.getUnitList();

        //Fetch the Dropdown GameObject the script is attached to
        Dropdown m_Dropdown = getDropDown(gmObj);
        //Clear the old options of the Dropdown menu
        m_Dropdown.ClearOptions();
        //Add the options created in the List above
        m_Dropdown.AddOptions(unitTypes);
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
        Debug.LogFormat("All items in UI is {0}.", active);

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

    public bool is_StepFile() { return this.Step_file; }

    public bool is_Measurement() { return this.Measurement; }

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

    public string getExternalFileName()
    {
        if (externalFile != null) return externalFile.getFileName();
        else return System.String.Empty;
    }

    public string getExternalFileDescription()
    {
        if (externalFile != null) return externalFile.getFileDescription();
        else return System.String.Empty;
    }

    public string getExternalFileType()
    {
        if (externalFile != null) return externalFile.getUnit();
        else return System.String.Empty;
    }

    public string getExternalFileURL()
    {
        if (externalFile != null) return externalFile.getURL();
        else return System.String.Empty;
    }

    public IEnumerable<DamageProperty> getMeasurements()
    {
        return Measurements;
    }

    public List<GameObject> getReferenceObjects()
    {
        return afterReference;
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
    ExternalFile externalFile = new ExternalFile();
    List<DamageProperty> Measurements = new List<DamageProperty>();
}
