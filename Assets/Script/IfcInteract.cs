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
using SFB;


public class IfcInteract : MonoBehaviour
{
    private string filePath;
    private readonly ObservableCollection<PropertyItem> _properties = new ObservableCollection<PropertyItem>();
    private List <IIfcElement> ifcProducts = new List<IIfcElement>();

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

        public void print()
        {
            Debug.Log($"IfcLabel: {IfcLabel}, PropertySetName: {PropertySetName}, Name: {Name}, Value: {Value}");
        }

        //public void printUI()
        //{
        //    GUIStyle style = new GUIStyle();
        //    style.normal.textColor = Color.black;
        //    String s1 = String.Format("Name: {0}.", Name);
        //    String s2 = String.Format("Value: {0}.", Value);

        //    GUI.Label(new Rect(10, 50, 100, 20), s1, style);
        //    GUI.Label(new Rect(10, 70, 100, 20), s2, style);
        //}
    }

    public ObservableCollection<PropertyItem> Properties
    {
        get { return _properties; }
    }

    public void setProduct(string Guid)
    {
        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                var ifcProduct = model.Instances.FirstOrDefault<IIfcElement>(d => d.GlobalId == Guid);
                FillPropertyData(ifcProduct);
            }
        }
    }

    [EasyButtons.Button]
    void open()
    {
        var extensions = new[] {
            new ExtensionFilter("IFC files", "ifc"),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open IFC File", "", extensions, false);
        filePath = path[0];

        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                ifcProducts = model.Instances.OfType<IIfcElement>().ToList();

                foreach (var ifcProduct in ifcProducts)
                {
                    Debug.Log($"IfcProduct ID: {ifcProduct.GlobalId}, Name: {ifcProduct.Name}");
                    FillPropertyData(ifcProduct);
                }  
            }
        }

        foreach (var _property in _properties)
            _property.print();
    }

    public void openFile(string filename)
    {
        filePath = filename;
        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                ifcProducts = model.Instances.OfType<IIfcElement>().ToList();

                foreach (var ifcProduct in ifcProducts)
                {
                    Debug.Log($"IfcProduct ID: {ifcProduct.GlobalId}, Name: {ifcProduct.Name}");
                }
            }
        }

        foreach (var _property in _properties)
            _property.print();
    }

    private void printProperty(IIfcProduct product)
    {
        //get all relations which can define property and quantity sets
        var properties = product.IsDefinedBy

                        //Search across all property and quantity sets. You might also want to search in a specific property set
                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)

                        //Only consider property sets in this case.
                        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)

                        //lets only consider single value properties. There are also enumerated properties, 
                        //table properties, reference properties, complex properties and other
                        .OfType<IIfcPropertySingleValue>();

        foreach (var property in properties)
            Debug.Log($"Property: {property.Name}, Value: {property.NominalValue}");
    }

    static void properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        
    }

    private void FillPropertyData(IIfcProduct _entity)
    {
        if (_properties.Any()) //don't try to fill unless empty
            return;
        //now the property sets for any 

        if (_entity is IIfcObject)
        {
            var asIfcObject = (IIfcObject)_entity;
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

    private void Clear(bool clearHistory = true)
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
