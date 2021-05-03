using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageDialogBox : DialogBox
{
    public GameObject image_1D_Button;
    public GameObject image_2D_Button;
    public GameObject image_3D_Button;
    public GameObject back_Button;

    private Button Image_1D_Btn;
    private Button Image_2D_Btn;
    private Button Image_3D_Btn;
    private Button Back_Btn;

    // Start is called before the first frame update
    void Start()
    {
        Image_1D_Btn = getButton(image_1D_Button);
        Image_2D_Btn = getButton(image_2D_Button);
        Image_3D_Btn = getButton(image_3D_Button);
        Back_Btn = getButton(back_Button);

        Image_1D_Btn.onClick.AddListener(set1DImage);
        Image_2D_Btn.onClick.AddListener(set2DImage);
        Image_3D_Btn.onClick.AddListener(set3DImage);
        Back_Btn.onClick.AddListener(back);
    }

    void set1DImage()
    {
        (_DamageInstance as DamageInstance).setImage(ImageType.Image_1D);
        DoneOperation(DimNotification.Set_1D_Image);
    }

    void set2DImage()
    {
        (_DamageInstance as DamageInstance).setImage(ImageType.Image_2D);
        DoneOperation(DimNotification.Set_2D_Image);
    }

    void set3DImage()
    {
        (_DamageInstance as DamageInstance).setImage(ImageType.Image_3D);
        DoneOperation(DimNotification.Set_3D_Image);
    }

    void back()
    {
        //(_DamageInstance as DamageInstance).setImage();
        DoneOperation(DimNotification.Back_Operation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
