using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class IFCData : MonoBehaviour
{
    public string filePath;
    public string IFCClass;
    public string STEPName;
    public string STEPId;
    public string STEPIndex;
    public string IFCLayer;

    public List<IFCPropertySet> propertySets;
    public List<IFCPropertySet> quantitySets;

    private void AddProperties(XmlNode node, GameObject go)
    {
        IFCData ifcData = go.AddComponent(typeof(IFCData)) as IFCData;

        ifcData.IFCClass = node.Name;
        ifcData.STEPId = node.Attributes.GetNamedItem("id").Value;
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
                        Debug.Log(
                            string.Format("PropertySet = {0}",
                                          propertySet.Attributes.GetNamedItem("Name").Value));

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

    public void AddElements(XmlNode node, GameObject parent)
    {
        if (node.Attributes.GetNamedItem("id") != null)
        {
            if(node.Attributes.GetNamedItem("Name") != null)
            {
                UnityEngine.Debug.Log(string.Format("{0} => {1}",
                                    node.Attributes.GetNamedItem("id").Value,
                                    node.Attributes.GetNamedItem("Name").Value)
                );
            }
            else
            {
                UnityEngine.Debug.Log(node.Attributes.GetNamedItem("id").Value);
            }
                
            // Search an existing GameObject with this name
            // This would apply only to elements which have
            // a geometric representation and which are
            // extracted from the 3D file.
            string searchPath = Path.GetFileNameWithoutExtension(filePath) + "/" +
                node.Attributes.GetNamedItem("id").Value;
            GameObject goElement = null;
            goElement = GameObject.Find(searchPath);

            // What if we can't find any? We need to create
            // a new empty object
            if (goElement == null)
                goElement = new GameObject();

            if (goElement != null)
            {
                // Set name from the IFC Name field
                if(node.Attributes.GetNamedItem("Name") != null)
                {
                    goElement.name = node.Attributes.GetNamedItem("Name").Value;
                }
                else
                {
                    goElement.name = "Empty";
                }
                // Link the object to the parent we received
                if (parent != null)
                    goElement.transform.SetParent(parent.transform);

                // Add properties
                AddProperties(node, goElement);

                // Go through children (recursively)
                foreach (XmlNode child in node.ChildNodes)
                    AddElements(child, goElement);
            }
        }// end if "id" attribute
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