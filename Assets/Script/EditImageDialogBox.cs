using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SFB;

public class EditImageDialogBox : DialogBox
{
    public GameObject Name;
    public GameObject Description;

    public GameObject Image;
    public GameObject Option_Box;
    public GameObject File_URL;
    public GameObject Object3D_Preview;

    public GameObject Browse_Button;
    public GameObject EditLocation_Button;
    public GameObject Finish_Button;

    // UI Element
    private InputField DefectName;
    private InputField DefectDescription;

    private Text ImageURL;
    private RawImage PreviewImage;

    private Dropdown OptionBox;
    private Button EditLocation_Btn;
    private Button Browse_Btn;
    private Button Finish_Btn;

    // Object 3D implant
    private GameObject PreviewObject;
    private GameObject Damage3DObject;
    private Camera3DPreview _camera3DPreview;

    // Internal Parameters
    private string defectName;
    private string defectDescription;
    private string _ImageFilePath;

    // Start is called before the first frame update
    void Start()
    {
        Browse_Btn = getButton(Browse_Button);
        EditLocation_Btn = getButton(EditLocation_Button);
        Finish_Btn = getButton(Finish_Button);

        // Map in Actions to the UI elements
        if (DefectName != null) DefectName.onValueChanged.AddListener(delegate { writeData(DefectName.text, out defectName); });
        if (DefectDescription != null) DefectDescription.onValueChanged.AddListener(delegate { writeData(DefectDescription.text, out defectDescription); });


        // Map in Actions to the UI elements
        Browse_Btn.onClick.AddListener(browseFile);
        EditLocation_Btn.onClick.AddListener(EditLocation);
        Finish_Btn.onClick.AddListener(finish);

    }

    void writeExistingData()
    {
        DefectName = getField(Name);
        DefectDescription = getField(Description);
        OptionBox = getDropDown(Option_Box);

        ImageURL = getText(File_URL);
        PreviewImage = getRawImage(Image);

        if (DefectName != null) DefectName.text = defectName;
        if (DefectDescription != null) DefectDescription.text = defectDescription;
        if (ImageURL != null) ImageURL.text = _ImageFilePath;

        previewWrittenFormat(_DamageInstance.ImageType);
        
        if (OptionBox != null)
        {
            // Clear the old options of the Dropdown menu
            OptionBox.ClearOptions();

            // Add the options created in the List above
            OptionBox.AddOptions(DamageModel.DamageTypes);

            //Debug.LogFormat("Current Selection: {0}", _DamageInstance.DefectTypeOption);
            // Write Option value to current value
            OptionBox.value = _DamageInstance.DefectTypeOption;
        }
    }

    void writeTexture()
    {
        if (PreviewImage != null)
        {
            PreviewImage.texture = _DamageInstance.Image2D;
            Image.SetActive(true);
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

    void EditLocation()
    {
        // Write Defect name
        writeDataIn();
        // Write Defect Type
        setDefectType();
        // Write Image Details
        _DamageInstance.updateImageURL = _ImageFilePath;

        // Call in Image Done Operation
        nextSequence(_DamageInstance.ImageType);
    }

    void nextSequence(ImageType _imageType)
    {
        switch (_imageType)
        {
            case ImageType.Image_1D:
                // Write in non 3D image Object
                writeNon3DImage();
                // Preview the image file
                DoneOperation(DimNotification.Edit_Non3DLocationOperation);
                break;

            case ImageType.Image_2D:
                // Write in non 3D image Object
                writeNon3DImage();
                // Preview the image file
                DoneOperation(DimNotification.Edit_Non3DLocationOperation);
                break;

            case ImageType.Image_3D:

                // Write in 3D Object
                //if (Damage3DObject != null)
                //{
                //    _DamageInstance.ImageObject = Damage3DObject;
                //    Damage3DObject.transform.SetParent(null);
                //    Damage3DObject.transform.position = Vector3.zero;
                //    Damage3DObject.transform.rotation = Quaternion.identity;
                //    Destroy(PreviewObject);
                //}

                //// Propose re-orientation function
                //DoneOperation(DimNotification.Next_Ref3DLoacationOperation);
                break;
        }
    }

    void finish()
    {
        // Write Defect name
        writeDataIn();
        // Write Defect Type
        setDefectType();
        // Write Image Details
        _DamageInstance.updateImageURL = _ImageFilePath;
        // Update Image Data
        switch(_DamageInstance.ImageType)
        {
            case ImageType.Image_1D:
                writeNon3DImage();
                break;

            case ImageType.Image_2D:
                writeNon3DImage();
                break;

            case ImageType.Image_3D:

                // Write in 3D Object
                if (Damage3DObject != null)
                {
                    var OriginalObject = _DamageInstance.ImageObject;
                    Damage3DObject.transform.SetParent(OriginalObject.transform.parent);
                    Damage3DObject.transform.localPosition = OriginalObject.transform.localPosition;
                    Damage3DObject.transform.localRotation = OriginalObject.transform.localRotation;

                    foreach (Transform trans in Damage3DObject.GetComponentsInChildren<Transform>(true))
                    {
                        // Set Layer to Preview Layer
                        trans.gameObject.layer = 9;
                    }

                    // Update Image Object to a new Object
                    _DamageInstance.ImageObject = Damage3DObject;
                    Destroy(OriginalObject);
                    Destroy(PreviewObject);
                }

                break;
        }
        // Finisg Edit the Entity
        DoneOperation(DimNotification.Finish_EditOperation);
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

    void writeNon3DImage()
    {
        if (_DamageInstance.Image2D != null)
        {
            Create2DImage();
        }
    }

    void Create2DImage()
    {
        if (_DamageGUI != null)
        {
            var Image2DObject = _DamageInstance.Image2DView;

            if (Image2DObject != null)
            {
                var Image2D = Image2DObject.GetComponentInChildren<RawImage>();

                if (Image2D != null) Image2D.texture = _DamageInstance.Image2D;

                Image2DObject.SetActive(false);
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
        _ImageFilePath = _DamageInstance.ImageUrl;

        writeExistingData();
    }

    void previewWrittenFormat(ImageType _imageType)
    {
        switch (_imageType)
        {
            case ImageType.Image_1D:
                // Preview the image file
                writeTexture();
                break;

            case ImageType.Image_2D:
                // Preview the image file
                writeTexture();
                break;

            case ImageType.Image_3D:
                write3DObject();
                break;
        }
    }

    void write3DObject()
    {
        setObject3D();
        Texture Object3DTexture = Resources.Load<Texture>("Textures/3D_Camera_Texture");

        if (_DamageInstance.ImageObject != null)
        {
            // Clone the existing 3D Object
            var gmObj = Instantiate(_DamageInstance.ImageObject);

            // Reset GameObject location
            gmObj.transform.localPosition = Vector3.zero;
            gmObj.transform.localRotation = Quaternion.identity;
            gmObj.transform.localScale = Vector3.one;

            // Asign texture into image
            PreviewImage.texture = Object3DTexture;
            // Asign Preview Layer
            setPreviewLayer(gmObj);
            // Store Loaded Object
            Damage3DObject = gmObj;
            // Set 3D Object under Preview Module
            if (_camera3DPreview != null) _camera3DPreview.insert3DObject(gmObj);
            // Make preview Image visible
            Image.SetActive(true);
        }
    }

    void browseFile()
    {
        var extensions = _DamageInstance.ContentTypes
            .Select(typ => new ExtensionFilter("Material images", typ))
            .Append(new ExtensionFilter("All Files", "*"))
            .ToArray();

        var path = StandaloneFileBrowser.OpenFilePanel("Open Material Image File", "", extensions, false);

        if (path.Length > 0)
        {
            string filePath = path[0];

            if (filePath.Length > 0)
            {
                // Show file path browse
                ImageURL.text = filePath;

                // Preview based on image dimension
                previewFormat(_DamageInstance.ImageType, filePath);
            }
        }
    }

    void setObject3D()
    {
        if (PreviewObject == null)
        {
            PreviewObject = Instantiate(Resources.Load<GameObject>("Prefabs/3D_Camera"), Vector3.zero, Quaternion.identity);

            // Single out Rotater from 3D Camera View
            _camera3DPreview = PreviewObject.GetComponent<Camera3DPreview>();
        }
    }


    void previewFormat(ImageType _imageType, string filepath)
    {
        switch (_imageType)
        {
            case ImageType.Image_1D:
                // Preview the image file
                previewImage(filepath);
                break;

            case ImageType.Image_2D:
                // Preview the image file
                previewImage(filepath);
                break;

            case ImageType.Image_3D:
                preview3DImage(filepath);
                break;
        }
    }

    void previewImage(string filepath)
    {
        Texture2D imageTexture = new Texture2D(150, 150);

        var image_bytes = System.IO.File.ReadAllBytes(filepath);

        imageTexture.LoadImage(image_bytes);
        // Asign texture name
        imageTexture.name = System.IO.Path.GetFileName(filepath);
        // Asign texture into image
        PreviewImage.texture = imageTexture;
        // Make preview Image visible
        Image.SetActive(true);

        writeData(filepath, out _ImageFilePath);
    }

    void preview3DImage(string filepath)
    {
        setObject3D();
        (_DamageGUI as DamageGUI).Generate3DFile(filepath);
        writeData(filepath, out _ImageFilePath);
    }

    public void FeedIn3DImage(GameObject gmObj)
    {
        Texture Object3DTexture = Resources.Load<Texture>("Textures/3D_Camera_Texture");

        // Asign texture into image
        PreviewImage.texture = Object3DTexture;
        // Asign Preview Layer
        setPreviewLayer(gmObj);
        // Store Loaded Object
        Damage3DObject = gmObj;
        // Set 3D Object under Preview Module
        if (_camera3DPreview != null) _camera3DPreview.insert3DObject(gmObj);
        // Make preview Image visible
        Image.SetActive(true);
    }

    void setPreviewLayer(GameObject gmObj)
    {
        foreach (Transform trans in gmObj.GetComponentsInChildren<Transform>(true))
        {
            // Set Layer to Preview Layer
            trans.gameObject.layer = 13;
        }
    }

    protected override void abortOperation()
    {
        if (PreviewObject != null)
            Destroy(PreviewObject);

        if (_DamageGUI != null)
            (_DamageGUI as DamageGUI).endOperation();
    }

    void checkStatus()
    {
        if (defectName != null && _ImageFilePath != null)
            Finish_Btn.interactable = (defectName.Length > 0 && _ImageFilePath.Length > 0);
        else
            Finish_Btn.interactable = false;
    }

    protected override void writeData(string input, out string variable)
    {
        variable = input;
        checkStatus();
    }
}
