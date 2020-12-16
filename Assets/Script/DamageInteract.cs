using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;

public class DamageInteract: IfcInteract
{
    private readonly List<PropertyItem> _typeProperties = new List<PropertyItem>();
    private readonly List<PropertyItem> _objectProperties = new List<PropertyItem>();

    public List<PropertyItem> AllProperties
    {
        get
        {
            var _anyProperties = _properties.Concat(_objectProperties);
            return _typeProperties.Concat(_anyProperties).ToList();
        }
    }

    public override void FillData(IPersistEntity _entity)
    {
        FillObjectData(_entity);
        FillTypeData(_entity);
        FillPropertyData(_entity);
        _propertiesBindings.Add(new PropertiesBinding(_entity, AllProperties));
        Clear();
    }

    public void FillTypeData(IPersistEntity _entity)
    {
        if (_typeProperties.Count > 0)
            return; // only fill once
        var ifcObj = _entity as IIfcObject;
        var typeEntity = ifcObj?.IsTypedBy.FirstOrDefault()?.RelatingType;
        if (typeEntity == null)
            return;
        var ifcType = typeEntity?.ExpressType;

        //now do properties in further specialisations that are text labels
        foreach (var pInfo in ifcType.Properties.Where
            (p => p.Value.EntityAttribute.Order > 4
                  && p.Value.EntityAttribute.State != EntityAttributeState.DerivedOverride)
            ) //skip the first for of root, and derived and things that are objects
        {
            var val = pInfo.Value.PropertyInfo.GetValue(typeEntity, null);
            if (!(val is ExpressType))
                continue;
            var pi = new PropertyItem { Name = pInfo.Value.PropertyInfo.Name, Value = ((ExpressType)val).ToString() };
            _typeProperties.Add(pi);
        }
    }

    private void FillObjectData(IPersistEntity _entity)
    {

        if (_objectProperties.Count > 0)
            return; //don't fill unless empty
        if (_entity == null)
            return;
        
        //_objectProperties.Add(new PropertyItem { Name = "Ifc Label", Value = "#" + _entity.EntityLabel, PropertySetName = "General" });

        var ifcType = _entity.ExpressType;
        //_objectProperties.Add(new PropertyItem { Name = "Type", Value = ifcType.Type.Name, PropertySetName = "General" });

        var ifcObj = _entity as IIfcObject;
        var typeEntity = ifcObj?.IsTypedBy.FirstOrDefault()?.RelatingType;
        //if (typeEntity != null)
        //{
        //    _objectProperties.Add(
        //        new PropertyItem
        //        {
        //            Name = "Defining Type",
        //            Value = typeEntity.Name,
        //            PropertySetName = "General",
        //            IfcLabel = typeEntity.EntityLabel
        //        }
        //    );
        //}

        var props = ifcType.Properties.Values;
        foreach (var prop in props)
        {
            ReportProp(_entity, prop, false);
        }
        var invs = ifcType.Inverses;

        foreach (var inverse in invs)
        {
            ReportProp(_entity, inverse, false);
        }
    }

    private void ReportProp(IPersistEntity entity, ExpressMetaProperty prop, bool verbose)
    {
        var propVal = prop.PropertyInfo.GetValue(entity, null);
        if (propVal == null)
        {
            if (!verbose)
                return;
            propVal = "<null>";
        }

        if (prop.EntityAttribute.IsEnumerable)
        {
            var propCollection = propVal as System.Collections.IEnumerable;

            if (propCollection != null)
            {
                var propVals = propCollection.Cast<object>().ToArray();
                switch (propVals.Length)
                {
                    case 0:
                        if (!verbose)
                            return;
                        _objectProperties.Add(new PropertyItem { Name = prop.PropertyInfo.Name, Value = "<empty>", PropertySetName = "General" });
                        break;
                    case 1:
                        to_add = true;
                        var tmpSingle = GetPropItem(propVals[0]);
                        tmpSingle.Name = prop.PropertyInfo.Name + "[0]";
                        tmpSingle.PropertySetName = "General";
                        if (to_add) _objectProperties.Add(tmpSingle);
                        break;
                    default:
                        int i = 0;
                        foreach (var item in propVals)
                        {
                            var tmpLoop = GetPropItem(item);
                            tmpLoop.Name = $"{prop.PropertyInfo.Name}[{i++}]";
                            tmpLoop.PropertySetName = prop.PropertyInfo.Name;
                            _objectProperties.Add(tmpLoop);
                        }
                        break;
                }
            }
            else
            {
                if (!verbose)
                    return;
                _objectProperties.Add(new PropertyItem { Name = prop.PropertyInfo.Name, Value = "<not an enumerable>" });
            }
        }
        else
        {
            var tmp = GetPropItem(propVal);
            tmp.Name = prop.PropertyInfo.Name;
            tmp.PropertySetName = "General";
            _objectProperties.Add(tmp);
        }
    }

    private bool to_add = true;
    private PropertyItem GetPropItem(object propVal)
    {
        var retItem = new PropertyItem();

        var pe = propVal as IPersistEntity;
        var propLabel = 0;
        if (pe != null)
        {
            propLabel = pe.EntityLabel;
        }



        var ret = propVal.ToString();
        //Debug.Log(System.String.Format("{0}\n{1}\n{2}", typeof(IIfcRelAssociatesDocument), propVal.GetType(), typeof(IIfcRelAssociatesDocument).IsInstanceOfType(propVal)));
        if (ret == propVal.GetType().FullName)
        {
            ret = propVal.GetType().Name;
        }

        if (propVal is IIfcRelAssociatesDocument)
        {
            var doc = (IIfcRelAssociatesDocument)pe;
            var doc_content = doc.RelatingDocument as IIfcDocumentReference;
            if (doc_content != null)
            {
                _objectProperties.Add(new PropertyItem { Name = "Document Name", Value = doc_content.Name });
                _objectProperties.Add(new PropertyItem { Name = "Description", Value = doc_content.Description });
                _objectProperties.Add(new PropertyItem { Name = "Location", Value = doc_content.Location });
                to_add = false;
            }
        }

        if (pe is IIfcRepresentation)
        {
            var t = (IIfcRepresentation)pe;
            ret += $" ('{t.RepresentationIdentifier}' '{t.RepresentationType}')";
        }
        if (pe is IIfcRelDefinesByProperties)
        {
            var t = (IIfcRelDefinesByProperties)pe;
            var stringValues = new List<string>();
            var name = t.RelatingPropertyDefinition?.PropertySetDefinitions.FirstOrDefault()?.Name;
            if (!string.IsNullOrEmpty(name))
                Debug.Log(name);
                stringValues.Add($"'{name}'");
            if (stringValues.Any())
            {
                ret += $" ({string.Join(" ", stringValues.ToArray())})";
            }
        }
        retItem.Value = ret;
        retItem.IfcLabel = propLabel;
        return retItem;
    }

    protected override void Clear(bool clearHistory = true)
    {
        _objectProperties.Clear();
        _properties.Clear();
        _typeProperties.Clear();
    }
}
