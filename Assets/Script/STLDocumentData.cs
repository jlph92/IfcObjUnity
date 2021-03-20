using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class STLDocumentData : DataExternalDocument
{

    public STLDocumentData(CoreApplication app) : base(app)
    {
        internalMetaData = new STLMetaData(app, this);
        writeReferenceFile();
    }

    public override void readData()
    {
        base.readData();
        Dictionary<string, string> keyValuePairs = parseData(metaDataContent);

        writeLengthUnit(GetLengthUnit(keyValuePairs["LengthUnit"]));
        writeLocalTransformation(GetMatrix(keyValuePairs["LocalMatrix"]));
        writeStlColor(GetColor(keyValuePairs["stlColor"]));
    }

    public override void writeData()
    {
        base.writeData();
    }

    protected override void overWrite()
    {
        base.overWrite();
    }

    // Writing in MetaData Parameters

    public void writeLengthUnit(Parabox.Stl.Unit LengthUnit)
    {
        (internalMetaData as STLMetaData).LengthUnit = LengthUnit;
    }

    public void writeLocalTransformation(Matrix4x4 LocalMatrix)
    {
        Debug.Log(LocalMatrix);
        (internalMetaData as STLMetaData).LocalMatrix = LocalMatrix;
    }

    public void writeStlColor(Color stlColor)
    {
        (internalMetaData as STLMetaData).stlColor = stlColor;
    }

    // Get the data from STL Document data
    public Parabox.Stl.Unit getLengthUnit()
    {
        return (internalMetaData as STLMetaData).LengthUnit;
    }

    public Quaternion getLocalTransformation()
    {
        Matrix4x4 m_matrix = (internalMetaData as STLMetaData).LocalMatrix;
        return ExtractRotation(m_matrix);
    }

    public Color getStlColor()
    {
        return (internalMetaData as STLMetaData).stlColor;
    }

    Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    Parabox.Stl.Unit GetLengthUnit(string input)
    {
        try
        {
            Parabox.Stl.Unit unitValue = (Parabox.Stl.Unit)System.Enum.Parse(typeof(Parabox.Stl.Unit), input, true);
            return unitValue;
        }
        catch (System.ArgumentException)
        {
            throw new System.NotImplementedException("Length unit not fit.");
            return Parabox.Stl.Unit.Milimeter;
        }
    }

    Matrix4x4 GetMatrix(string input)
    {
        input = input.Trim();
        input = input.Trim(new System.Char[] { '{', '}' }).Trim();

        Dictionary<string, float> transMatrixValues = input.Split(new string[] { ", " }, System.StringSplitOptions.RemoveEmptyEntries)
            .Where(value => !System.String.IsNullOrWhiteSpace(value))
            .Select(value => value.Split(new string[] { ":" }, System.StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(pair => pair[0], pair => float.Parse(pair[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

        var newMatrix = new Matrix4x4();
        newMatrix.SetColumn(0, new Vector4(transMatrixValues["m11"], transMatrixValues["m21"], transMatrixValues["m31"], transMatrixValues["m41"]));
        newMatrix.SetColumn(1, new Vector4(transMatrixValues["m12"], transMatrixValues["m22"], transMatrixValues["m32"], transMatrixValues["m42"]));
        newMatrix.SetColumn(2, new Vector4(transMatrixValues["m13"], transMatrixValues["m23"], transMatrixValues["m33"], transMatrixValues["m43"]));
        newMatrix.SetColumn(3, new Vector4(transMatrixValues["m14"], transMatrixValues["m24"], transMatrixValues["m34"], transMatrixValues["m44"]));

        return newMatrix;
    }

    Color GetColor(string input)
    {
        input = input.Trim();
        input = input.Trim(new System.Char[] { '{', '}' }).Trim();

        Dictionary<string, float> colorValues = input.Split(new string[] { ", " }, System.StringSplitOptions.RemoveEmptyEntries)
            .Where(value => !System.String.IsNullOrWhiteSpace(value))
            .Select(value => value.Split(new string[] { ":" }, System.StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(pair => pair[0], pair => float.Parse(pair[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

        Color stlColor = new Color(colorValues["R"], colorValues["G"], colorValues["B"], colorValues["A"]);

        return stlColor;
    }

    protected override void visualizeExternalDocument()
    {
        //new STLDataVisualization(app).GenerateGameObject(this);
    }
}
