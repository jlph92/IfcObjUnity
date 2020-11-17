using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using UnityEditor;

public class IFCData : MonoBehaviour
{
    public string IFCClass;
    public string STEPName;
    public string STEPId;
    public string STEPIndex;
    public string IFCLayer;
    public string Tag;

    public List<IFCPropertySet> propertySets;
    public List<IFCPropertySet> quantitySets;

    public static void AddProperties(XmlNode node, GameObject go)
    {
        go.tag = AddTag(node.Name);
        IFCData ifcData = go.AddComponent(typeof(IFCData)) as IFCData;

        ifcData.IFCClass = node.Name;

        ifcData.STEPId = node.Attributes.GetNamedItem("id").Value;
        if (node.Attributes.GetNamedItem("Tag") != null)
        {
            ifcData.Tag = node.Attributes.GetNamedItem("Tag").Value;
        }
        if (node.Attributes.GetNamedItem("Name") != null)
        {
            ifcData.STEPName = node.Attributes.GetNamedItem("Name").Value;
        }

        // Initialize PropertySets and QuantitySets
        if (ifcData.propertySets == null)
            ifcData.propertySets = new List<IFCPropertySet>();
        if (ifcData.quantitySets == null)
            ifcData.quantitySets = new List<IFCPropertySet>();


        // Go through Properties (and Quantities and ...)
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.Name)
            {
                case "IfcPropertySet":
                    break;
                case "IfcElementQuantity":
                    // we only receive a link beware of # character:
                    string link = child.Attributes.GetNamedItem("xlink:href").Value.TrimStart('#');
                    string path = String.Format("//ifc/properties/IfcPropertySet[@id='{0}']", link);
                    if (child.Name == "IfcElementQuantity")
                        path = String.Format("//ifc/quantities/IfcElementQuantity[@id='{0}']", link);
                    XmlNode propertySet = child.SelectSingleNode(path);
                    if (propertySet != null)
                    {
                        //Debug.Log(
                        //    string.Format("PropertySet = {0}",
                        //                  propertySet.Attributes.GetNamedItem("Name").Value));

                        // initialize this propertyset (Name, Id)
                        IFCPropertySet myPropertySet = new IFCPropertySet();
                        myPropertySet.propSetName = propertySet.Attributes.GetNamedItem("Name").Value;
                        myPropertySet.propSetId = propertySet.Attributes.GetNamedItem("id").Value;
                        if (myPropertySet.properties == null)
                            myPropertySet.properties = new List<IFCProperty>();

                        // run through property values
                        foreach (XmlNode property in propertySet.ChildNodes)
                        {
                            string propName, propValue = "";
                            IFCProperty myProp = new IFCProperty();
                            propName = property.Attributes.GetNamedItem("Name").Value;

                            if (property.Name == "IfcPropertySingleValue")
                                propValue = property.Attributes.GetNamedItem("NominalValue").Value;
                            if (property.Name == "IfcQuantityLength")
                                propValue = property.Attributes.GetNamedItem("LengthValue").Value;
                            if (property.Name == "IfcQuantityArea")
                                propValue = property.Attributes.GetNamedItem("AreaValue").Value;
                            if (property.Name == "IfcQuantityVolume")
                                propValue = property.Attributes.GetNamedItem("VolumeValue").Value;
                            // Write property (name & value)
                            myProp.propName = propName;
                            myProp.propValue = propValue;
                            myPropertySet.properties.Add(myProp);
                        }

                        // add propertyset to IFCData
                        if (child.Name == "IfcPropertySet")
                            ifcData.propertySets.Add(myPropertySet);
                        if (child.Name == "IfcElementQuantity")
                            ifcData.quantitySets.Add(myPropertySet);

                    } // end if PropertySet
                    break;
                default:
                    // all the rest...
                    break;
            } // end switch
        } // end foreach
    }


    private static string AddTag(string tag)
    {
        UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if ((asset != null) && (asset.Length > 0))
        {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return tag;     // Tag already present, nothing to do.
                }
            }

            tags.InsertArrayElementAtIndex(0);
            tags.GetArrayElementAtIndex(0).stringValue = tag;
            so.ApplyModifiedProperties();
            so.Update();
        }
        return tag;
    }
}

[System.Serializable]
public class IFCPropertySet
{
    public string propSetName = "";
    public string propSetId = "";

    public List<IFCProperty> properties;
}

[System.Serializable]
public class IFCProperty
{
    public string propName = "";
    public string propValue = "";
}