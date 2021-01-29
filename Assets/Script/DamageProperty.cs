using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class DamageProperty
{
    // Property item
    private string _Property_Name = "";

    private string _Property_Value = "";

    private Type _selectedType = getType(0);

    public string Property_Name
    {
        get
        {
            return this._Property_Name;
        }
        set
        {
            this._Property_Name = value;
            //Debug.LogFormat("{0} is set", value);
            ValidateData();
        }
    }

    public string Property_Value
    {
        get
        {
            return this._Property_Value;
        }
        set
        {
            this._Property_Value = value;
            //Debug.LogFormat("{0} is set", value);
            ValidateData();
        }
    }

    public Type selectedType
    {
        get
        {
            return this._selectedType;
        }
        set
        {
            this._selectedType = value;
            //Debug.LogFormat("{0} is set", value);
            ValidateData();
        }
    }

    // Triger Event if data is completed
    public event EventHandler dataCompleted;

    protected virtual void OnDataCompleted(EventArgs e)
    {
        if (dataCompleted != null)
            dataCompleted(this, e);
    }

    // Triger Event if data is imcomplete
    public event EventHandler dataImcomplete;

    protected virtual void OnDataImcomplete(EventArgs e)
    {
        if (dataImcomplete != null)
            dataImcomplete(this, e);
    }

    // Check if data is complete
    private void ValidateData()
    {
        if (_Property_Name.Length > 0 && _Property_Value.Length > 0 && _selectedType != null)
            OnDataCompleted(null);
        else
            OnDataImcomplete(null);
    }

    public static Type getType(int index)
    {
        List<Type> IfcTypes = GetInheritedClasses(typeof(Xbim.Ifc4.MeasureResource.IfcValue)).ToList();
        return IfcTypes.ElementAt(index);
    }

    public static List<string> getUnitType()
    {
        return GetInheritedClasses(typeof(Xbim.Ifc4.MeasureResource.IfcValue)).Select(d => (d.Name))
                                        .ToList();
    }

    static IEnumerable<Type> GetInheritedClasses(Type MyType)
    {
        return Assembly.GetAssembly(MyType).GetTypes().Where(TheType =>  MyType.IsAssignableFrom(TheType));
    }

    public string getDataText()
    {
        return String.Format("{0}: {2} ({1})", _Property_Name, _selectedType.Name, _Property_Value);
    }

    public Xbim.Ifc4.MeasureResource.IfcValue getIFCUnit()
    {
        Xbim.Ifc4.MeasureResource.IfcValue param;
        try
        {
            param = Activator.CreateInstance(_selectedType, _Property_Value) as Xbim.Ifc4.MeasureResource.IfcValue;
        }
        catch (Exception e)
        {
            param = Activator.CreateInstance(_selectedType) as Xbim.Ifc4.MeasureResource.IfcValue;
        }
        return param;
    }
}
