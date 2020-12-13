using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;
using Xbim.Ifc;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Common;
using Xbim.Ifc.ViewModels;
using SFB;


public class IfcInteract
{

    protected readonly List<PropertyItem> _properties = new List<PropertyItem>();

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

    public IEnumerable<PropertyItem> Properties
    {
        get { return _properties; }
    }

    static void properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        
    }

    public void FillPropertyData(IIfcObjectDefinition _entity)
    {
        if (_properties.Any()) //don't try to fill unless empty
            return;
        //now the property sets for any 
        if (_entity is IIfcObject)
        {
            var asIfcObject = _entity as IIfcObject;
            foreach (
                var pSet in
                    asIfcObject.IsDefinedBy.Select(
                        relDef => relDef.RelatingPropertyDefinition as IIfcPropertySet)
                )
                AddPropertySet(pSet);
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

    public virtual void Clear(bool clearHistory = true)
    {
        _properties.Clear();

        NotifyPropertyChanged("Properties");
        NotifyPropertyChanged("PropertySets");
    }

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(string info)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(info));
    }
}
