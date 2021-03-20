using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.Stl;
using System.Linq;

public class ExternalFile
{
    // Data Url
    string fileName;
    string fileDecription;
    string url_link;
    Unit unit = Unit.Milimeter;

    Vector3 CartesianPoint;
    Vector3 Origin;

    public void SetUnit(int index)
    {
        List<Unit> UnitTypes = System.Enum.GetValues(typeof(Unit))
                                        .Cast<Unit>()
                                        .ToList();

        this.unit = UnitTypes.ElementAt(index);
    }

    public void SetFileName(string filename)
    {
        this.fileName = filename;
    }

    public void SetFileDescription(string description)
    {
        this.fileDecription = description;
    }

    public void SetURL(string url)
    {
        this.url_link = url;
    }

    public void SetPoint(Vector3 Point)
    {
        this.CartesianPoint = Point;
    }

    public void SetOrigin(Vector3 Point)
    {
        this.Origin = Point;
    }

    public string getFileName()
    {
        return this.fileName;
    }

    public string getFileDescription()
    {
        return this.fileDecription;
    }

    public string getUnit()
    {
        return this.unit.ToString("G");
    }

    public Unit getStlUnit()
    {
        return this.unit;
    }

    public string getURL()
    {
        return this.url_link;
    }

    public bool getRelativePlacement(out Vector3 result)
    {
        if (CartesianPoint != null && Origin != null)
        {
            result = translateBIMCoordinate(CartesianPoint);
            Debug.LogFormat("Vector Point Write: {0}, BIM Coordinate Write: {1}", CartesianPoint, result);
            return true;
        }
        else
        {
            result = Vector3.zero;
            return false;
        }
            
    }

    public static List<string> getUnitList()
    {
        List<string> UnitTypes = System.Enum.GetValues(typeof(Unit))
                                        .Cast<Unit>()
                                        .Select(d => (d.ToString()))
                                        .ToList();
        return UnitTypes;
    }

    Vector3 translateBIMCoordinate(Vector3 point)
    {
        Vector3 BimWorldCoordinate = new Vector3(point.x, point.z, point.y);
        return BimWorldCoordinate;
    }
}
