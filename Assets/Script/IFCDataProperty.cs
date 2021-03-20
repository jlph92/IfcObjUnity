using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;
using SFB;

public class IFCDataProperty : DimModel
{
    /// <summary>
    /// Storing IFC data properties
    /// </summary>
    protected readonly List<PropertyItem> _properties = new List<PropertyItem>();

    /// <summary>
    /// Binding IFC data properties to IFC Entity
    /// </summary>
    protected readonly List<PropertiesBinding> _propertiesBindings = new List<PropertiesBinding>();

    public IFCDataProperty(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    /// <summary>
    /// Retrieve IFC data properties from  IFC Entity from Binding List
    /// </summary>
    public IEnumerable<PropertyItem> getProperties(IPersistEntity _entity)
    {
        if (_propertiesBindings.Count > 0 && _entity != null)
        {
            if (_propertiesBindings.Exists(x => x._entity.Equals(_entity)))
                return _propertiesBindings.Find(x => x._entity.Equals(_entity)).getValue();
            else
                return null;
        }
        else return null;
    }

    /// <summary>
    /// Adding Binding data
    /// </summary>
    public virtual void FillData(IPersistEntity _entity)
    {
        FillPropertyData(_entity);
        _propertiesBindings.Add(new PropertiesBinding(_entity, _properties));
        Clear();
    }

    /// <summary>
    /// Retrieve IFC data properties from  IFC Entity
    /// </summary>
    protected void FillPropertyData(IPersistEntity _entity)
    {
        if (_properties.Any()) //don't try to fill unless empty
            return;
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
                return;
            foreach (var pSet in asIfcTypeObject.HasPropertySets.OfType<IIfcPropertySet>())
            {
                AddPropertySet(pSet);
            }
        }
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
            Value = val
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

    /// <summary>
    /// Reset Properties Set
    /// </summary>
    protected virtual void Clear(bool clearHistory = true)
    {
        _properties.Clear();
    }

    public class PropertyItem
    {
        public string Units { get; set; }

        public string PropertySetName { get; set; }

        public string Name { get; set; }

        public int IfcLabel { get; set; }

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
    }

    protected class PropertiesBinding
    {
        public IPersistEntity _entity;
        private List<PropertyItem> _properties;

        public PropertiesBinding(IPersistEntity _entity, List<PropertyItem> _properties)
        {
            this._entity = _entity;
            this._properties = _properties.ToList();
        }

        public IEnumerable<PropertyItem> getValue()
        {
            return this._properties;
        }
    }
}