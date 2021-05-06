using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using SFB;

public class InputImageDialogBox : DialogBox
{
    public GameObject Name;
    public GameObject Description;
    public GameObject Image;
    public GameObject Back_Button;
    public GameObject Next_Button;
    public GameObject Browse_Button;
    public GameObject File_URL;
    public GameObject Object3D_Preview;
    public Texture2D Object3DTexture;

    // Object 3D implant
    private GameObject PreviewObject;
    private GameObject Damage3DObject;
    private Camera3DPreview _camera3DPreview;

    private InputField ImageName;
    private InputField ImageDescription;

    private Text ImageURL;
    private RawImage PreviewImage;

    private Button Browse_Btn;
    private Button Back_Btn;
    private Button Next_Btn;

    private ImageType imageType;

    private string _ImageName;
    private string _ImageDescription;
    private string _ImageFilePath;

    private UnityAction<string> assign_ImageName;
    private UnityAction<string> assign_ImageDescription;

    // Start is called before the first frame update
    void Start()
    {
        // Assign Unity Actions
        assign_ImageName = delegate { writeData(ImageName.text, out _ImageName); };
        assign_ImageDescription = delegate { writeData(ImageDescription.text, out _ImageDescription); };

        imageType = _DamageInstance.ImageType;

        ImageName = getField(Name);
        ImageDescription = getField(Description);

        Browse_Btn = getButton(Browse_Button);
        Back_Btn = getButton(Back_Button);
        Next_Btn = getButton(Next_Button);

        ImageURL = getText(File_URL);
        PreviewImage = getRawImage(Image);

        Browse_Btn.onClick.AddListener(browseFile);
        Back_Btn.onClick.AddListener(back);
        Next_Btn.onClick.AddListener(next);

        Next_Btn.interactable = false;

        // Map in Actions to the UI elements
        ImageName.onValueChanged.AddListener(assign_ImageName);
        ImageDescription.onValueChanged.AddListener(assign_ImageDescription);
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

    void back()
    {
        // Destroy Controller earlier
        if (PreviewObject != null)
            Destroy(PreviewObject);

        //(_DamageInstance as DamageInstance).setImage();
        DoneOperation(DimNotification.Back_Operation);
    }

    void next()
    {
        // Write Image Details
        _DamageInstance.SetImageProperties(ImageName: _ImageName, ImageDescription: _ImageDescription, ImageURL: _ImageFilePath);

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
                DoneOperation(DimNotification.Next_RefLoacationOperation);
                break;

            case ImageType.Image_2D:
                // Write in non 3D image Object
                writeNon3DImage();
                // Preview the image file
                DoneOperation(DimNotification.Next_RefLoacationOperation);
                break;

            case ImageType.Image_3D:

                // Write in 3D Object
                if (Damage3DObject != null)
                {
                    _DamageInstance.ImageObject = Damage3DObject;
                    Damage3DObject.transform.SetParent(null);
                    Damage3DObject.transform.position = Vector3.zero;
                    Damage3DObject.transform.rotation = Quaternion.identity;
                    Destroy(PreviewObject);
                }

                // Propose re-orientation function
                DoneOperation(DimNotification.Next_Ref3DLoacationOperation);
                break;
        }
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
            var Image2DObject = (_DamageGUI as DamageGUI).Create2DImage();

            if (Image2DObject != null)
            {
                var Image2D = Image2DObject.GetComponentInChildren<RawImage>();

                if (Image2D != null) Image2D.texture = _DamageInstance.Image2D;

                _DamageInstance.Image2DView = Image2DObject;
                Image2DObject.SetActive(false);
            }
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
        if (_ImageName != null && _ImageFilePath != null)
            Next_Btn.interactable = (_ImageName.Length > 0 && _ImageFilePath.Length > 0);
        else
            Next_Btn.interactable = false;
    }

    protected override void writeData(string input, out string variable)
    {
        variable = input;
        checkStatus();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
