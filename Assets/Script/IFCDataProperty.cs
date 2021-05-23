using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;
using SFB;

public class IFCDataProperty
{
    /// <summary>
    /// Storing IFC data properties
    /// </summary>
    protected readonly List<PropertyItem> _properties = new List<PropertyItem>();

    public static List<PropertyItem> GetProperties(IPersistEntity _entity)
    {
        return new IFCDataProperty().FillPropertyData(_entity);
    }

    /// <summary>
    /// Retrieve IFC data properties from  IFC Entity
    /// </summary>
    protected List<PropertyItem> FillPropertyData(IPersistEntity _entity)
    {
        if (_properties.Any()) //don't try to fill unless empty
            return _properties;
        //now the property sets for any 
        if (_entity is IIfcObject)
        {
            var asIfcObject = _entity as IIfcObject;
            //Debug.Log(System.String.Format("{0} :{1}", asIfcObject.Name, asIfcObject.EntityLabel));
            foreach (
                var pSet in
                    asIfcObject.IsDefinedBy.Select(
                        relDef => relDef.RelatingPropertyDefinition as IIfcPropertySet)
            )
            {
                AddPropertySet(pSet);
            }
        }
        else if (_entity is IIfcTypeObject)
        {
            var asIfcTypeObject = _entity as IIfcTypeObject;
            if (asIfcTypeObject.HasPropertySets == null)
                return _properties;
            foreach (var pSet in asIfcTypeObject.HasPropertySets.OfType<IIfcPropertySet>())
            {
                AddPropertySet(pSet);
            }
        }

        return _properties;
    }

    /// <summary>
    /// Building Property set
    /// </summary>
    private void AddPropertySet(IIfcPropertySet pSet)
    {
        if (pSet == null)
            return;
        foreach (var item in pSet.HasProperties.OfType<IIfcPropertySingleValue>()) //handle IfcPropertySingleValue
        {
            AddProperty(item, pSet.Name);
        }
        foreach (var item in pSet.HasProperties.OfType<IIfcComplexProperty>()) // handle IfcComplexProperty
        {
            // by invoking the undrlying addproperty function with a longer path
            foreach (var composingProperty in item.HasProperties.OfType<IIfcPropertySingleValue>())
            {
                AddProperty(composingProperty, pSet.Name + " / " + item.Name);
            }
        }
        foreach (var item in pSet.HasProperties.OfType<IIfcPropertyEnumeratedValue>()) // handle IfcComplexProperty
        {
            AddProperty(item, pSet.Name);
        }
    }

    /// <summary>
    /// Building Single Value Property
    /// </summary>
    private void AddProperty(IIfcPropertySingleValue item, string groupName)
    {
        var val = System.String.Empty;
        var nomVal = item.NominalValue;
        if (nomVal != null)
            val = nomVal.ToString();
        _properties.Add(new PropertyItem
        {
            IfcLabel = item.EntityLabel,
            PropertySetName = groupName,
            Name = item.Name,
            Value = val,
            IfcValueType = nomVal.GetType()
        });
    }

    /// <summary>
    /// Building Enumerated Value Property
    /// </summary>
    private void AddProperty(IIfcPropertyEnumeratedValue item, string groupName)
    {
        var val = System.String.Empty;
        var nomVals = item.EnumerationValues;

        foreach (var nomVal in nomVals)
        {
            if (nomVal != null)
                val = nomVal.ToString();

            _properties.Add(new PropertyItem
            {
                IfcLabel = item.EntityLabel,
                PropertySetName = groupName,
                Name = item.Name,
                Value = val
            });
        }
    }
}

public class PropertyItem
{
    public string Units { get; set; }

    public string PropertySetName { get; set; }

    public string Name { get; set; }

    public int IfcLabel { get; set; }

    public System.Type IfcValueType { get; set; }

    public string IfcUri
    {
        get { return "xbim://EntityLabel/" + IfcLabel; }
    }

    public bool IsLabel
    {
        get { return IfcLabel > 0; }
    }

    public string Value { get; set; }

    private readonly string[] _schemas = { "file", "ftp", "http", "https" };

    public bool IsLink
    {
        get
        {
            Uri uri;
            if (!Uri.TryCreate(Value, UriKind.Absolute, out uri))
                return false;
            var schema = uri.Scheme;
            return _schemas.Contains(schema);
        }
    }

    public string ToString()
    {
        return System.String.Format("{0}: {1}", this.Name, this.Value);
    }

    public IIfcProperty DIM2IfcProperty (IfcStore model)
    {
        var property = new Create(model).PropertySingleValue(p =>
        {
            p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier(this.Name);
        });

        UnityEngine.Debug.LogFormat("Property Name: {0}, Property type: {1}", this.Name, this.IfcValueType.FullName);

        try
        {
            property.NominalValue = Activator.CreateInstance(this.IfcValueType, this.Value) as Xbim.Ifc4.MeasureResource.IfcValue;
        }
        catch (Exception e)
        {
            property.NominalValue = Activator.CreateInstance(this.IfcValueType) as Xbim.Ifc4.MeasureResource.IfcValue;
        }

        return property;
    }
}