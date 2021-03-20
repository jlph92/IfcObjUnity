using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;

public class STLMetaData : ExternalMetaData
{
    public Unit LengthUnit;
    public Matrix4x4 LocalMatrix = Matrix4x4.identity;
    public Color stlColor;

    public STLMetaData(CoreApplication app, DimController controller) : base(app, controller)
    {
        DataType = ExternalDocumentFileType.STL;
    }

    public override string getMetaContent()
    {
        string s0 = System.String.Format("ReferenceFile = {0}\n", ReferenceFile);
        string s1 = System.String.Format("LengthUnit = {0}\n", LengthUnit);
        string s2 = System.String.Format("LocalMatrix = {0}\n", ToMatrixString());
        string s3 = System.String.Format("stlColor = {0}\n", ToColorString());

        return s0 + s1 + s2 + s3;
    }

    string ToColorString()
    {
        string colorString = System.String.Format("{{ R:{0}, G:{1}, B:{2}, A:{3} }}", stlColor.r, stlColor.g, stlColor.b, stlColor.a);
        return colorString;
    }

    string ToMatrixString()
    {
        Vector4 row_0 = LocalMatrix.GetRow(0);
        Vector4 row_1 = LocalMatrix.GetRow(1);
        Vector4 row_2 = LocalMatrix.GetRow(2);
        Vector4 row_3 = LocalMatrix.GetRow(3);

        string s0 = System.String.Format("{{ m11:{0}, m12:{1}, m13:{2}, m14:{3},", row_0.x.ToString("R"), row_0.y.ToString("R"), row_0.z.ToString("R"), row_0.w.ToString("R"));
        string s1 = System.String.Format(" m21:{0}, m22:{1}, m23:{2}, m24:{3},", row_1.x.ToString("R"), row_1.y.ToString("R"), row_1.z.ToString("R"), row_1.w.ToString("R"));
        string s2 = System.String.Format(" m31:{0}, m32:{1}, m33:{2}, m34:{3},", row_2.x.ToString("R"), row_2.y.ToString("R"), row_2.z.ToString("R"), row_2.w.ToString("R"));
        string s3 = System.String.Format(" m41:{0}, m42:{1}, m43:{2}, m44:{3} }}", row_3.x.ToString("R"), row_3.y.ToString("R"), row_3.z.ToString("R"), row_3.w.ToString("R"));

        string matrixString = s0 + s1 + s2 + s3;

        return matrixString;
    }
}