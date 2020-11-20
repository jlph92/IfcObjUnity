using UnityEngine;
using UnityEditor;

class Example : EditorWindow
{
    // Rotates the selected Game Object +45 degrees if the user presses 'g'
    // or -45 degrees if the user presses 'Shift + g'
    // If no object is selected, the Menus are grayed out.

    [MenuItem("Example/Rotate Green +45 _g")]
    static void RotateGreenPlus45()
    {
        GameObject obj = Selection.activeGameObject;
        obj.transform.Rotate(Vector3.up * 45);
    }

    [MenuItem("Example/Rotate Green +45 _g", true)]
    static bool ValidatePlus45()
    {
        return Selection.activeGameObject != null;
    }

    [MenuItem("Example/Rotate green -45 #g")]
    static void RotateGreenMinus45()
    {
        GameObject obj = Selection.activeGameObject;
        obj.transform.Rotate(Vector3.down * 45);
    }

    [MenuItem("Example/Rotate green -45 #g", true)]
    static bool ValidateMinus45()
    {
        return Selection.activeGameObject != null;
    }
}