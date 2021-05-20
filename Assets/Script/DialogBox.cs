using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public GameObject ProductLabel;
    public GameObject Cancel_Button;

    // Abort Button
    protected Button CancelBtn;

    protected DimView _DamageGUI;
    protected DamageModel _DamageInstance;

    protected void writeLabel(string ProductLabelText)
    {
        Text text = getText(ProductLabel);
        text.text = ProductLabelText;
    }

    public virtual void assignBox(DamageModel _DamageInstance, DimView _DamageGUI)
    {
        this._DamageInstance = _DamageInstance;
        this._DamageGUI = _DamageGUI;

        if (ProductLabel != null) writeLabel(_DamageInstance.ParentName);
        if (Cancel_Button != null)
        {
            CancelBtn = getButton(Cancel_Button);
            CancelBtn.onClick.AddListener(abortOperation);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected virtual void abortOperation()
    {
        if (_DamageGUI != null)
            (_DamageGUI as DamageGUI).endOperation();
    }

    protected void DoneOperation(string nexGUI)
    {
        if (_DamageGUI != null)
            (_DamageGUI as DamageGUI).switchOperation(nexGUI);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected ScrollViewContent getScrollView(GameObject gmObj)
    {
        return gmObj.GetComponent<ScrollViewContent>();
    }

    protected Button getButton(GameObject gmObj)
    {
        return gmObj.GetComponent<Button>();
    }

    protected Toggle getToggle(GameObject gmObj)
    {
        return gmObj.GetComponent<Toggle>();
    }

    protected Dropdown getDropDown(GameObject gmObj)
    {
        return gmObj.GetComponent<Dropdown>();
    }

    protected Text getText(GameObject gmObj)
    {
        return gmObj.GetComponent<Text>();
    }

    protected InputField getField(GameObject gmObj)
    {
        return gmObj.GetComponent<InputField>();
    }

    protected RawImage getRawImage(GameObject gmObj)
    {
        return gmObj.GetComponent<RawImage>();
    }

    protected Slider getSlider(GameObject gmObj)
    {
        return gmObj.GetComponent<Slider>();
    }

    protected virtual void writeData(string input, out string variable)
    {
        variable = input;
    }
}
