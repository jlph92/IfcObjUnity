using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageGUI : DimView
{
    public GameObject General_DialogBox;
    public GameObject Image_DialogBox;
    public GameObject InputImage_DialogBox;
    public GameObject Text_DialogBox;
    public GameObject InputText_DialogBox;
    public GameObject OrientationBox;
    public GameObject PlaneOrientationBox;
    public GameObject SurfaceBuilder;

    private GameObject previousDialogBox;
    private GameObject currentDialogBox;

    // Store current DialogBox
    private DialogBox _DialogBox;

    // Create a Damage Instance
    private DamageModel _DamageInstance;

    public DamageGUI(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    public void initialise(IfcModel ifcModel)
    {
        if (_DamageInstance == null) _DamageInstance = DamageInstance.CreateInstance(ifcModel);
        startDialogBox();
    }

    public void startDialogBox()
    {
        // Generate the General Dialog Box
        Create(General_DialogBox);
    }

    void Start()
    {
        (controller as GUIHandler).setView(this);
    }

    DialogBox getDialogBox(GameObject gmObj)
    {
        return gmObj.GetComponent<DialogBox>();
    }

    DialogBox Create(GameObject gmObj)
    {
        if (gmObj != null)
        {
            currentDialogBox = Instantiate(gmObj, transform);
            _DialogBox = getDialogBox(currentDialogBox);
            _DialogBox.assignBox(_DamageInstance, this);

            return _DialogBox;
        }

        else return null;
    }

    public void endOperation()
    {
        Destroy(currentDialogBox);
        this.app.Notify(controller: controller, message: DimNotification.AbortDim, parameters: gameObject);
    }

    public void switchOperation(string nexGUI)
    {
        Destroy(currentDialogBox);
        // Create next scene
        var NextDialogBox = Create(nextScene(nexGUI));
        if (NextDialogBox == null) this.app.Notify(controller: controller, message: DimNotification.FinishEditDim, parameters: _DamageInstance);
    }

    public void deFreezeScreen()
    {
        // Unfreeze Scene
        this.app.Notify(controller: controller, message: DimNotification.UnFreezeScreen, parameters: null);
    }

    public void Generate3DFile(string filepath)
    {
        this.app.Notify(controller: controller, message: DimNotification.LoadStlFile, parameters: filepath);
    }

    public void FeedIn3DImage(GameObject gmObj)
    {
        if (_DialogBox != null) (_DialogBox as InputImageDialogBox).FeedIn3DImage(gmObj);
    }

    public GameObject Create2DImage()
    {
        var ImageObject = Instantiate(Resources.Load<GameObject>("Prefabs/ImageView"), transform.parent);
        return ImageObject;
    }

    GameObject nextScene(string nexGUI)
    {
        Debug.Log(nexGUI);

        switch (nexGUI)
        {
            case DimNotification.SetImageType:
                previousDialogBox = General_DialogBox;
                return Image_DialogBox;
                break;

            case DimNotification.SetTextType:
                previousDialogBox = General_DialogBox;
                return Text_DialogBox;
                break;

            case DimNotification.Set_1D_Image:
                previousDialogBox = Image_DialogBox;
                return InputImage_DialogBox;
                break;

            case DimNotification.Set_2D_Image:
                previousDialogBox = Image_DialogBox;
                return InputImage_DialogBox;
                break;

            case DimNotification.Set_3D_Image:
                previousDialogBox = Image_DialogBox;
                return InputImage_DialogBox;
                break;

            case DimNotification.Back_Operation:
                if (previousDialogBox != null)
                    return previousDialogBox;
                else return null;
                break;

            case DimNotification.Next_InputImageOperation:
                previousDialogBox = InputImage_DialogBox;
                return SurfaceBuilder;
                break;

            case DimNotification.Next_RefLoacationOperation:
                previousDialogBox = SurfaceBuilder;
                return PlaneOrientationBox;
                break;

            case DimNotification.Next_Ref3DLoacationOperation:
                previousDialogBox = SurfaceBuilder;
                return OrientationBox;
                break;

            case DimNotification.Next_AddTextOperation:
                previousDialogBox = Text_DialogBox;
                return InputText_DialogBox;
                break;

            case DimNotification.Next_OrienatationOperation:
                return null;
                break;

            case DimNotification.Next_PlaneOrienatationOperation:
                return null;
                break;

            case DimNotification.Finish_TextOperation:
                return null;
                break;
        }

        return null;
    }
}