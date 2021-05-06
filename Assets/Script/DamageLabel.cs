using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageLabel : MonoBehaviour
{
    private DamageModel _DamageModel;
    private Text LabelText;
    private Camera DamageCamera;
    private Toggle toggleBtn;

    private Button closeButton;

    public DamageModel DefectModel
    {
        get { return _DamageModel; }

        set
        {
            _DamageModel = value;
            if (LabelText == null) LabelText = GetComponentInChildren<Text>();
            LabelText.text = _DamageModel.Name;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LabelText = GetComponentInChildren<Text>();
        DamageCamera = GameObject.FindGameObjectWithTag("DamageCamera").GetComponent<Camera>();
        toggleBtn = GetComponent<Toggle>();
        toggleBtn.onValueChanged.AddListener(delegate { ToggleValueChanged(); });

        if (closeButton == null)
        {
            if (_DamageModel.Image2DView != null)
            {
                closeButton = _DamageModel.Image2DView.GetComponentInChildren<Button>();
                closeButton.onClick.AddListener(closeImage);
            }
        }
    }

    void closeImage()
    {
        toggleBtn.isOn = false;
    }

    void ToggleValueChanged()
    {
        if (toggleBtn.isOn) _DamageModel.showImage();
        else _DamageModel.hideImage();
    }

    // Update is called once per frame
    void Update()
    {
        if (DamageCamera != null && _DamageModel != null)
        {
            transform.position = DamageCamera.WorldToScreenPoint(_DamageModel.ImageOrigin);
        }   
    }
}
