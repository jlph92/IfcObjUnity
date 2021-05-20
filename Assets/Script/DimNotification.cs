using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimNotification
{
    // Internal Process
    public const string ConfigureGuiInterface = "Configure.Gui.Interface"; //Configure GUI interface to control all IFC related data

    // Ifc Notiification
    public const string CreateIFCInterface = "Create.IFC.Interface"; //Create IFC interface to control all IFC related data

    // Read IFC file
    public const string LoadIFCFile = "Load.IFC.File";

    // End Session with IFC file
    public const string UnloadIFCFile = "Unload.IFC.File";

    // Write Ifc Triggered
    public const string WriteIFCFile = "Write.IFC.FIle";

    // Load Ifc data
    public const string IfcLoaded = "IFC.Data.Loaded"; // Load in Data and update Ifc Tree View
    public const string OverwriteIFCFile = "Overwrite.IFC.FIle";
    public const string LoadIFCProperty = "Load.IFC.Property";

    // Read Damage data
    public const string DamageLoaded = "Damage.Data.Loaded"; // Load in Data and update Damage Tree View
    public const string LoadDamageProperty = "Load.Damage.Property";

    public const string UnloadStlFile = "Unload.Stl.File";
    public const string LoadStlFile = "Load.Stl.File";
    public const string OverwriteStlFile = "Overwrite.Stl.FIle";

    public const string FinishLoadIFC2Obj = "Finish.Load.IFC2Obj.File"; // Finish loaing Obj File from IFC

    public const string UnloadObjFile = "Unload.Obj.File";
    public const string LoadObjFile = "Load.Obj.File";
    public const string OverwriteObjFile = "Overwrite.Obj.FIle";

    public const string AddDim = "Add.DIM";
    public const string EditDim = "Edit.Dim";
    public const string FinishEditDim = "Finish.Edit.DIM";
    public const string RefreshDim = "Refresh.Dim";
    public const string AbortDim = "Abort.DIM";

    public const string CreateIfcTree = "Create.Ifc.Tree";

    // Set Tree for scene
    public const string TreeSet = "Tree.Set";
    public const string DamageTreeSet = "Damage.Tree.Set";

    // Select item in scene
    public const string SelectItem = "Select.Item";

    // Anotation Button Visibility
    public const string ShowAnnotateButton = "Show.Annotate.Button";
    public const string HideAnnotateButton = "Hide.Annotate.Button";

    // Edit Button Visibility
    public const string ShowEditButton = "Show.Edit.Button";
    public const string HideEditButton = "Hide.Edit.Button";

    // Input Type Creation
    public const string SetImageType = "Set.Image.Type";
    public const string SetTextType = "Set.Text.Type";

    // Image Type Creation
    public const string Set_1D_Image = "Set.1D.Image";
    public const string Set_2D_Image = "Set.2D.Image";
    public const string Set_3D_Image = "Set.3D.Image";

    // Locate reference Point
    public const string Set_2D_ImageLocation = "Set.2D.Image.Location";
    public const string Set_3D_ImageLocation = "Set.3D.Image.Location";

    // Local transformation control
    public const string Set_3D_ImageTransformation = "Set.3D.Image.Transformation";

    // Add single damage property
    public const string Input_Single_Text = "Input.Single.Text";

    // Back Operation
    public const string Back_Operation = "Back.Operation";

    // Next Operation
    public const string Next_InputImageOperation = "Next.Input.Image.Operation";
    public const string Next_RefLoacationOperation = "Next.Ref.Location.Operation";
    public const string Next_Ref3DLoacationOperation = "Next.Ref.3D.Location.Operation";
    public const string Next_OrienatationOperation = "Next.Orienatation.Operation";
    public const string Next_PlaneOrienatationOperation = "Next.Plane.Orienatation.Operation";
    public const string Finish_TextOperation = "Finish.Text.Operation";
    public const string Edit_Non3DLocationOperation = "Edit.Non.3D.Location.Operation";
    public const string Edit_3DLocationOperation = "Edit.3D.Location.Operation";
    public const string Finish_EditOperation = "Finish.Edit.Operation";

    // Text Operation
    public const string Next_AddTextOperation = "Next.Add.Text.Operation";
    public const string Next_EditTextOperation = "Next.Edit.Text.Operation";

    // Freeze & Unfreeze Viewer
    public const string FreezeScreen = "Freeze.Screen";
    public const string UnFreezeScreen = "UnFreeze.Screen";
}
