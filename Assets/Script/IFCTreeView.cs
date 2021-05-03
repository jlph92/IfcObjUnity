using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;
using Battlehub.UIControls;
using Xbim.Common.Geometry;

public class IFCTreeView : MonoBehaviour
{
    public TreeView TreeView;
    public IfcPropertyView ifcPropertyView;
    public Text SelectedText;
    public GameObject AnnotateButton;
    protected string filePath;
    protected IEnumerable<IXbimViewModel> dataItems;
    protected IfcInteract ifcInteract= new IfcInteract();
    protected ObjectBinding ObjectBindingProperty = new ObjectBinding();

    // The selected attached Product Label
    private int ProductLabel;

    protected virtual void Awake()
    {
        ifcInteract = new IfcInteract();
    }

    private void Start()
    {
        if(AnnotateButton != null)
        {
            // deActivate Annotate Button
            AnnotateButton.SetActive(false);
            AnnotateButton.GetComponent<Button>().onClick.AddListener(AddProxy);
        }

        if (!TreeView)
        {
            Debug.LogError("Set TreeView field");
            return;
        }
    }

    public void openFile(string filename)
    {
        filePath = filename;
        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                //PlacementTree.BuildTree(model);
                setup(model);
            }
        }
    }

    private void setup(IfcStore Model)
    {
        this.Model = Model;

        ViewModel();

        //subscribe to events
        TreeView.ItemDataBinding += OnItemDataBinding;
        TreeView.SelectionChanged += OnSelectionChanged;
        TreeView.ItemExpanding += OnItemExpanding;

        //Bind data items
        TreeView.Items = dataItems;
    }

    private void OnItemExpanding(object sender, ItemExpandingArgs e)
    {
        //get parent data item (game object in our case)
        IXbimViewModel node = e.Item as IXbimViewModel;
        e.Children = node.Children.Cast<IXbimViewModel>().ToArray();
    }

    protected IXbimViewModel selectedItem = null;

    private void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {
        // deActivate Annotate Button
        if(AnnotateButton != null) AnnotateButton.SetActive(false);

        // get list box item and tranlate to entity

        if (e.NewItems.Length <= 0)
            return;

        var p = TreeView.SelectedItem as IXbimViewModel;
        var p2 = selectedItem;

        //Debug.Log(String.Format("Selected Items: {0}", (TreeView.SelectedItem as IXbimViewModel).Name));

        if (p2 == null)
        {
            //Debug.Log(String.Format("No Selected Item Before: {0}", p.Name));
            selectedItem = p;
        }
        else if (p.EntityLabel == p2.EntityLabel)
        {
            //Debug.Log(String.Format("Same Items: {0}, {1}", p.Name, p2.Name));
            return;
        }
        else
        {
            //Debug.Log(String.Format("Update selection: {0} to {1}", p.Name, p2.Name));
            ObjectBindingProperty.unselect(p2 as IXbimViewModel);
            selectedItem = p;
        }

        if (ObjectBindingProperty.select(TreeView.SelectedItem as IXbimViewModel))
        {
            ProductLabel = (TreeView.SelectedItem as IXbimViewModel).EntityLabel;
            AnnotateButton.SetActive(true);
        } 

        var selected = TreeView.SelectedItem as IXbimViewModel;
        var prop = ifcInteract.getProperties(selected.Entity);
        ifcPropertyView.writeProperties(prop);

        if (TreeView.SelectedItem is null) SelectedText.text = "null";
        else SelectedText.text = (TreeView.SelectedItem as IXbimViewModel).Name;
    }


    /// <summary>
    /// This method called for each data item during databinding operation
    /// You have to bind data item properties to ui elements in order to display them.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnItemDataBinding(object sender, TreeViewItemDataBindingArgs e)
    {
        IXbimViewModel dataItem = e.Item as IXbimViewModel;
        if (dataItem != null)
        {
            //We display dataItem.name using UI.Text 
            Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
            text.text = dataItem.Name;

            //Load icon from resources
            //Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
            //icon.sprite = Resources.Load<Sprite>("cube");

            //Debug.Log(dataItem.Name + ": " + dataItem.Children.Count());

            //And specify whether data item has children (to display expander arrow if needed)
            e.HasChildren = dataItem.Children.Count() > 0;
        }
    }
  

    public IfcStore Model
    {
        get { return ModelProperty; }
        set { ModelProperty = value; }
    }

    private IfcStore ModelProperty = null;


    protected virtual void ViewModel()
    {
        var project = Model.Instances.OfType<IIfcProject>().FirstOrDefault();
        if (project != null)
        {
            ObservableCollection<XbimModelViewModel> svList = new ObservableCollection<XbimModelViewModel>();
            svList.Add(new XbimModelViewModel(project, null));
            dataItems = svList;

            foreach (var child in svList)
                LazyLoadAll(child);
        }
    }

    protected void LazyLoadAll(IXbimViewModel parent)
    {
        foreach (var child in parent.Children)
        {
            if (typeof(IIfcElement).IsAssignableFrom(child.Entity.ExpressType.Type))
                ObjectBindingProperty.Register(child);
            LazyLoadAll(child);
        }
        ifcInteract.FillData(parent.Entity);
    }

    // Sample code how to write in IFC file

    public void writeSampleData()
    {
        string file = System.IO.Path.GetFileNameWithoutExtension(filePath);
        string NewPath = filePath.Replace(file, file + "-Edit");

        var editor = new XbimEditorCredentials
        {
            ApplicationDevelopersName = "Jason Viewer",
            ApplicationFullName = "xbim toolkit",
            ApplicationIdentifier = "xbim",
            ApplicationVersion = "4.0",
            EditorsFamilyName = "Lai",
            EditorsGivenName = "Jason Poh Hwa",
            EditorsOrganisationName = "ITD"
        };

        using (var model = IfcStore.Open(filePath, editor))
        {
            using (var txn = model.BeginTransaction("Add in Proxy"))
            {
                //create semantic proxy with correspondant attribute
                var pProxy = new Create(model).Proxy(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("New Damage");
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("New Damage is done to the product");
                    r.ObjectType = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect");
                });

                //create relationship between proxy and ifcProduct
                var pRelAggregates = new Create(model).RelAggregates(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage to product");
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("The related product is damaged");
                    //r.RelatingObject.Add("Defect");
                    r.RelatedObjects.Add(pProxy);
                });

                //create relationship between proxy and ObjectType
                var pRelDefinesByType = new Create(model).RelDefinesByType(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage type");
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Typification of a defect");
                    r.RelatedObjects.Add(pProxy);
                    r.RelatingType = new Create(model).TypeObject(p =>
                    {
                        p.GlobalId = Guid.NewGuid();
                        p.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage type Spalling");
                        p.ApplicableOccurrence = new Xbim.Ifc4.MeasureResource.IfcIdentifier("IfcProxy/Defect");
                    });
                });

                //set a few basic properties
                new Create(model).RelDefinesByProperties(rel =>
                {
                    rel.GlobalId = Guid.NewGuid();
                    rel.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect Measurements");
                    rel.Description = new Xbim.Ifc4.MeasureResource.IfcText("Property parameters for Defect");

                    rel.RelatedObjects.Add(pProxy);
                    rel.RelatingPropertyDefinition = new Create(model).PropertySet(pset =>
                    {
                        pset.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Pset_Condition");
                        pset.HasProperties.AddRange(new[] {
                            new Create(model).PropertySingleValue(p =>
                            {
                                p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier("Condition Rating");
                                p.NominalValue = new Xbim.Ifc4.MeasureResource.IfcReal(2.4);
                            }),
                            new Create(model).PropertySingleValue(p =>
                            {
                                p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier("AssessmentDate");
                                p.NominalValue = new Xbim.Ifc4.DateTimeResource.IfcDate("2020-11-30");
                            })
                        });
                    });
                });

                //AddExternalFile(model, pProxy);
                //getAttachedLocation(model, ProductLabel);

                //commit changes
                txn.Commit();
            }
            model.SaveAs(NewPath);
        }
    }

    private void writeIFCData(Damage dmg)
    {
        string file = System.IO.Path.GetFileNameWithoutExtension(filePath);
        string NewPath = filePath.Replace(file, file + "-Edit");

        var editor = new XbimEditorCredentials
        {
            ApplicationDevelopersName = "Jason Viewer",
            ApplicationFullName = "xbim toolkit",
            ApplicationIdentifier = "xbim",
            ApplicationVersion = "4.0",
            EditorsFamilyName = "Lai",
            EditorsGivenName = "Jason Poh Hwa",
            EditorsOrganisationName = "ITD"
        };

        using (var model = IfcStore.Open(filePath, editor))
        {

            using (var txn = model.BeginTransaction("Add in Proxy"))
            {
                //create semantic proxy with correspondant attribute
                var pProxy = new Create(model).Proxy(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(dmg.getProxyName());
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText(dmg.getProxyDescription());
                    r.ObjectType = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect");
                    r.ObjectPlacement = getAttachedPlacement(model, dmg.getProductLabel());
                });

                //create relationship between proxy and ifcProduct
                var pRelAggregates = new Create(model).RelAggregates(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage to product");
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("The related product is damaged");
                    r.RelatingObject = getAttachedProduct(model, dmg.getProductLabel());
                    r.RelatedObjects.Add(pProxy);
                });

                //create relationship between proxy and ObjectType
                var pRelDefinesByType = new Create(model).RelDefinesByType(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage type");
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Typification of a defect");
                    r.RelatedObjects.Add(pProxy);
                    r.RelatingType = new Create(model).TypeObject(p =>
                    {
                        p.GlobalId = Guid.NewGuid();
                        p.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(dmg.getDamageType());
                        p.ApplicableOccurrence = new Xbim.Ifc4.MeasureResource.IfcIdentifier("IfcProxy/Defect");
                    });
                });
                
                if (dmg.is_Measurement())
                {
                    //set a few basic properties
                    new Create(model).RelDefinesByProperties(rel =>
                    {
                        rel.GlobalId = Guid.NewGuid();
                        rel.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect Measurements");
                        rel.Description = new Xbim.Ifc4.MeasureResource.IfcText("Property parameters for Defect");

                        rel.RelatedObjects.Add(pProxy);
                        rel.RelatingPropertyDefinition = new Create(model).PropertySet(pset =>
                        {
                            pset.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Pset_Condition");
                            pset.HasProperties.AddRange(createProperties(model, dmg));
                        });
                    });
                }
                
                if (dmg.is_StepFile())
                {
                    //addPlacement(model, pProxy, dmg);
                    AddExternalFile(model, pProxy, dmg);
                }
                
                //commit changes
                txn.Commit();
            }
            model.SaveAs(NewPath);
        }

        refreshDamageTree(NewPath);
    }

    void refreshDamageTree(string PathName)
    {
        var damageTree = FindObjectOfType<Damage_TreeView>();
        damageTree.reloadFile(PathName);
    }

    //void addPlacement(IfcStore model, IIfcProxy pProxy, Damage dmg)
    //{
    //    IIfcCartesianPoint cartesianPoint;

    //    //var placement = new BIMPlacement(model);
    //    var AttachedProduct = model.Instances.FirstOrDefault<IIfcProduct>(d => d.EntityLabel == dmg.getProductLabel());

    //    //Vector3 referenceOrigin = placement.getProductOrigin(AttachedProduct);
    //    Vector3 relativeWorldPoint;

    //    if (dmg.getRelativePlacement(out relativeWorldPoint))
    //    {
    //        Debug.LogFormat("World Vector: {0}", relativeWorldPoint);
    //        Vector3 relativePoint = placement.parentMatrix.inverse.MultiplyPoint3x4(relativeWorldPoint);

    //        try
    //        {
    //            cartesianPoint = model.Instances.New<Xbim.Ifc4.GeometryResource.IfcCartesianPoint>(r =>
    //            {
    //                r.X = relativePoint.x;
    //                r.Y = relativePoint.y;
    //                r.Z = relativePoint.z;
    //            });
    //        }
    //        catch
    //        {
    //            cartesianPoint = model.Instances.New<Xbim.Ifc2x3.GeometryResource.IfcCartesianPoint>(r =>
    //            {
    //                r.X = relativePoint.x;
    //                r.Y = relativePoint.y;
    //                r.Z = relativePoint.z;
    //            });
    //        }

    //        Debug.LogFormat("BIM Vector written: {0}", relativePoint);
    //    }

    //    else return;

    //    var axis2Placement3D = new Create(model).Axis2Placement3D(r =>
    //    {
    //        r.Location = cartesianPoint;
    //    });

    //    var localPlacement = new Create(model).LocalPlacement(r =>
    //    {
    //        r.PlacementRelTo = pProxy.ObjectPlacement;
    //        r.RelativePlacement = axis2Placement3D;
    //    });

    //    pProxy.ObjectPlacement = localPlacement;

    //    //Vector3 referenceOrigin = placement.getProductOrigin(pProxy as IIfcProduct);
    //    //Debug.LogFormat("Vector Point of {0} written in : {1}", pProxy.Name, referenceOrigin);
    //}

    //protected Vector3 getAttachedLocation(IfcStore model, int ProductLabel, bool showOrigin = true)
    //{
    //    var AttachedProduct = model.Instances.FirstOrDefault<IIfcProduct>(d => d.EntityLabel == ProductLabel);
    //    var placement = new BIMPlacement(model);

    //    Vector3 referenceOrigin = placement.getProductOrigin(AttachedProduct);
    //    Debug.LogFormat("Vector Point of {0} : {1}", AttachedProduct.Name, referenceOrigin);
    //    //referenceOrigin = m.MultiplyPoint3x4(referenceOrigin);
    //    //referenceOrigin = scaleMatrix.MultiplyPoint3x4(referenceOrigin);

    //    if (showOrigin) Instantiate(Resources.Load<GameObject>("Prefabs/Origin"), referenceOrigin, Quaternion.identity);
    //    //XbimMatrix3D.Identity;
    //    //var AttachedProductLocation = model.Instances.FirstOrDefault<Xbim.Ifc4.Kernel.IfcProduct>(d => d.EntityLabel == ProductLabel);
    //    //Debug.Log(AttachedProductLocation.ObjectPlacement.ToString());
    //    //var placement = AttachedProductLocation.ObjectPlacement;
    //    //return AttachedProductLocation.ObjectPlacement as IfcLocalPlacement;

    //    return referenceOrigin;
    //}

    IIfcObjectPlacement getAttachedPlacement(IfcStore model, int ProductLabel)
    {
        var AttachedProduct = model.Instances.FirstOrDefault<IIfcProduct>(d => d.EntityLabel == ProductLabel);
        Debug.Log(AttachedProduct.Name);

        return AttachedProduct.ObjectPlacement;
    }

    IIfcObjectDefinition getAttachedProduct(IfcStore model, int ProductLabel)
    {
        var AttachedProduct = model.Instances.FirstOrDefault<IIfcObjectDefinition>(d => d.EntityLabel == ProductLabel);
        Debug.Log(AttachedProduct.Name);

        return AttachedProduct;
    }

    List<IIfcPropertySingleValue> createProperties(IfcStore model, Damage dmg)
    {
        List < IIfcPropertySingleValue > properties = new List <IIfcPropertySingleValue>();
        foreach (DamageProperty measurement in dmg.getMeasurements())
        {
            properties.Add(
                new Create(model).PropertySingleValue(p =>
                {
                    p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier(measurement.Property_Name);
                    p.NominalValue = measurement.getIFCUnit();
                })
            );
        }

        return properties;
    }

    // Attached in external document reference
    private void AddExternalFile(IfcStore model, IIfcProxy pProxy, Damage dmg)
    {
        
        //create reference document metaData
        var pDocInformation = new Create(model).DocumentInformation(r =>
        {
            r.Identification = new Xbim.Ifc4.MeasureResource.IfcIdentifier(Guid.NewGuid().ToString());
            r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("MetaData");
            r.Description = new Xbim.Ifc4.MeasureResource.IfcText("This is a meta data.");
            r.Purpose = new Xbim.Ifc4.MeasureResource.IfcText("For stl reference if any.");
            //r.Location = new Xbim.Ifc4.ExternalReferenceResource.IfcURIReference("sample");
            r.ElectronicFormat = new Xbim.Ifc4.MeasureResource.IfcIdentifier("Stl");
        });

        //create reference document
        var pDocReference = new Create(model).DocumentReference(r =>
        {
            r.Location = new Xbim.Ifc4.ExternalReferenceResource.IfcURIReference(dmg.getExternalFileURL());
            r.Identification = new Xbim.Ifc4.MeasureResource.IfcIdentifier(Guid.NewGuid().ToString());
            r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(dmg.getExternalFileName());
            r.Description = new Xbim.Ifc4.MeasureResource.IfcText(dmg.getExternalFileDescription());
            r.ReferencedDocument = pDocInformation;
        });

        //create reference document
        var pPropertySet = new Create(model).PropertySet(pset =>
        {
            pset.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("File Parameter");

            pset.HasProperties.AddRange(new[] {

                new Create(model).PropertySingleValue(p =>
                {
                    p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier("Model Scaling");
                    p.NominalValue = new Xbim.Ifc4.MeasureResource.IfcIdentifier(dmg.getExternalFileType());
                })
            });
        });

        //create relationship between proxy and external source
        var docAssociateProxy = new Create(model).RelAssociatesDocument(r =>
        {
            r.GlobalId = Guid.NewGuid();
            r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Related Parameters");
            r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Connect related parameters for the reference document.");
            r.RelatedObjects.Add(pProxy);
            r.RelatedObjects.Add(pPropertySet);
            r.RelatingDocument = pDocReference;
        });
    }



    // Add in new IfcProxy for semantic purpose
    public void AddProxy()
    {
        Debug.Log("Dialog Box called.");
        buildDamage();
    }

    private void buildDamage()
    {
        GameObject dmgBox = Instantiate(Resources.Load<GameObject>("Prefabs/DialogBox"), GameObject.Find("UI").transform);
        Damage dmg = new Damage(dmgBox, ProductLabel);

        // register with an event
        dmg.EndCalled += End_DialogBox;
        dmg.AbortionCalled += Abort_DialogBox;
        dmg.TriggerSurfaceBuilder += surfaceBuilder;
        dmg.EndExternalFile += endExternalFile;
    }

    void showOrigin(Damage dmg)
    {
        using (var model = IfcStore.Open(filePath))
        {
            //dmg.SetOrigin(getAttachedLocation(model, dmg.getProductLabel()));
        }
    }

    // Called surface Build for 3D point setup event handler
    private void surfaceBuilder(object sender, EventArgs e)
    {
        showOrigin(sender as Damage);

        GameObject surfaceBuilder = new GameObject("3D Point");
        //surfaceBuilder.AddComponent<surfaceBuider>();

        //(sender as Damage).surfaceBuilder = surfaceBuilder;
    }

    // End External Reference process
    private void endExternalFile(object sender, EventArgs e)
    {
        foreach (GameObject g in (sender as Damage).getReferenceObjects())
        {
            Destroy(g);
        }
    }

    // Abort Dialog Box event handler
    private void Abort_DialogBox(object sender, EventArgs e)
    {
        Destroy((sender as Damage).getGameObject());
    }

    // End Dialog Box event handler
    private void End_DialogBox(object sender, EventArgs e)
    {
        writeIFCData(sender as Damage);
        Destroy((sender as Damage).getGameObject());
    }
}

