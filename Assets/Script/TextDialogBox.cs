using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDialogBox : DialogBox
{
    public GameObject Add_Button;
    public GameObject Remove_Button;
    public GameObject Finish_Button;
    public GameObject PropertyContainer;

    private Button Add_Btn;
    private Button Remove_Btn;
    private Button Finish_Btn;
    private ScrollViewContent scrollViewItem;

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

    void AddText()
    {
        DoneOperation(DimNotification.Next_AddTextOperation);
    }

    void RemoveText()
    {

    }

    void FinishOperation()
    {
        DoneOperation(DimNotification.Finish_TextOperation);
    }
}
