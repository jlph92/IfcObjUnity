using System;
using System.Linq;
using System.Collections.Generic;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;
using UnityEngine;
using SFB;


public class IfcInteract
{

    protected readonly List<PropertyItem> _properties = new List<PropertyItem>();

    protected readonly List<PropertiesBinding> _propertiesBindings = new List<PropertiesBinding>();
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

    public class PropertiesBinding
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

    public IEnumerable<PropertyItem> getProperties(IPersistEntity _entity)
    {
        if (_propertiesBindings.Count > 0) return _propertiesBindings.Find(x => x._entity.Equals(_entity)).getValue();
        else return null;
    }

    public void FillPropertyData(IPersistEntity _entity)
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

        _propertiesBindings.Add(new PropertiesBinding(_entity, _properties));
        Clear();
    }

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

    private void AddProperty(IIfcPropertySingleValue item, string groupName)
    {
        var val = "";
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

    private void AddProperty(IIfcPropertyEnumeratedValue item, string groupName)
    {
        var val = "";
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

    protected virtual void Clear(bool clearHistory = true)
    {
        _properties.Clear();
    }

}
