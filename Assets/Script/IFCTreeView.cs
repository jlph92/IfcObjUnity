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
        AnnotateButton.SetActive(false);

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
    //
    //private void writeData()
    //{
    //    string file = System.IO.Path.GetFileNameWithoutExtension(filePath);
    //    string NewPath = filePath.Replace(file, file + "-Edit");

    //    var editor = new XbimEditorCredentials
    //    {
    //        ApplicationDevelopersName = "Jason Viewer",
    //        ApplicationFullName = "xbim toolkit",
    //        ApplicationIdentifier = "xbim",
    //        ApplicationVersion = "4.0",
    //        EditorsFamilyName = "Lai",
    //        EditorsGivenName = "Jason Poh Hwa",
    //        EditorsOrganisationName = "ITD"
    //    };

    //    using (var model = IfcStore.Open(filePath, editor))
    //    {
    //        using (var txn = model.BeginTransaction("Add in Proxy"))
    //        {
    //            //create semantic proxy with correspondant attribute
    //            var pProxy = model.Instances.New<Xbim.Ifc4.Kernel.IfcProxy>(r =>
    //            {
    //                r.GlobalId = Guid.NewGuid();
    //                r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("New Damage");
    //                r.Description = new Xbim.Ifc4.MeasureResource.IfcText("New Damage is done to the product");
    //                r.ObjectType = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect");
    //            });

    //            //create relationship between proxy and ifcProduct
    //            var pRelAggregates = model.Instances.New<Xbim.Ifc4.Kernel.IfcRelAggregates>(r =>
    //            {
    //                r.GlobalId = Guid.NewGuid();
    //                r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage to product");
    //                r.Description = new Xbim.Ifc4.MeasureResource.IfcText("The related product is damaged");
    //                //r.RelatingObject.Add("Defect");
    //                r.RelatedObjects.Add(pProxy);
    //            });

    //            //create relationship between proxy and ObjectType
    //            var pRelDefinesByType = model.Instances.New<Xbim.Ifc4.Kernel.IfcRelDefinesByType>(r =>
    //            {
    //                r.GlobalId = Guid.NewGuid();
    //                r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage type");
    //                r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Typification of a defect");
    //                r.RelatedObjects.Add(pProxy);
    //                r.RelatingType = model.Instances.New<Xbim.Ifc4.Kernel.IfcTypeObject>(p =>
    //                {
    //                    p.GlobalId = Guid.NewGuid();
    //                    p.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage type Spalling");
    //                    p.ApplicableOccurrence = new Xbim.Ifc4.MeasureResource.IfcIdentifier("IfcProxy/Defect");
    //                });
    //            });

    //            //set a few basic properties
    //            model.Instances.New<Xbim.Ifc4.Kernel.IfcRelDefinesByProperties>(rel =>
    //            {
    //                rel.GlobalId = Guid.NewGuid();
    //                rel.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect Measurements");
    //                rel.Description = new Xbim.Ifc4.MeasureResource.IfcText("Property parameters for Defect");

    //                rel.RelatedObjects.Add(pProxy);
    //                rel.RelatingPropertyDefinition = model.Instances.New<Xbim.Ifc4.Kernel.IfcPropertySet>(pset =>
    //                {
    //                    pset.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Pset_Condition");
    //                    pset.HasProperties.AddRange(new[] {
    //                        model.Instances.New<Xbim.Ifc4.PropertyResource.IfcPropertySingleValue>(p =>
    //                        {
    //                            p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier("Condition Rating");
    //                            p.NominalValue = new Xbim.Ifc4.MeasureResource.IfcReal(2.4);
    //                        }),
    //                        model.Instances.New<Xbim.Ifc4.PropertyResource.IfcPropertySingleValue>(p =>
    //                        {
    //                            p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier("AssessmentDate");
    //                            p.NominalValue = new Xbim.Ifc4.DateTimeResource.IfcDate("2020-11-30");
    //                        })
    //                    });
    //                });
    //            });

    //            //commit changes
    //            txn.Commit();
    //        }
    //        model.SaveAs(NewPath);
    //    }
    //}

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
                var pProxy = model.Instances.New<Xbim.Ifc4.Kernel.IfcProxy>(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(dmg.getProxyName());
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText(dmg.getProxyDescription());
                    r.ObjectType = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect");
                });

                //create relationship between proxy and ifcProduct
                var pRelAggregates = model.Instances.New<Xbim.Ifc4.Kernel.IfcRelAggregates>(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage to product");
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("The related product is damaged");
                    r.RelatingObject = getAttachedProduct(model, dmg.getProductLabel());
                    r.RelatedObjects.Add(pProxy);
                });

                //create relationship between proxy and ObjectType
                var pRelDefinesByType = model.Instances.New<Xbim.Ifc4.Kernel.IfcRelDefinesByType>(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage type");
                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Typification of a defect");
                    r.RelatedObjects.Add(pProxy);
                    r.RelatingType = model.Instances.New<Xbim.Ifc4.Kernel.IfcTypeObject>(p =>
                    {
                        p.GlobalId = Guid.NewGuid();
                        p.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(dmg.getDamageType());
                        p.ApplicableOccurrence = new Xbim.Ifc4.MeasureResource.IfcIdentifier("IfcProxy/Defect");
                    });
                });

                //set a few basic properties
                model.Instances.New<Xbim.Ifc4.Kernel.IfcRelDefinesByProperties>(rel =>
                {
                    rel.GlobalId = Guid.NewGuid();
                    rel.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect Measurements");
                    rel.Description = new Xbim.Ifc4.MeasureResource.IfcText("Property parameters for Defect");

                    rel.RelatedObjects.Add(pProxy);
                    rel.RelatingPropertyDefinition = model.Instances.New<Xbim.Ifc4.Kernel.IfcPropertySet>(pset =>
                    {
                        pset.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Pset_Condition");
                        pset.HasProperties.AddRange(createProperties(model, dmg));
                    });
                });

                //commit changes
                txn.Commit();
            }
            model.SaveAs(NewPath);
        }
    }

    Xbim.Ifc4.Kernel.IfcObjectDefinition getAttachedProduct(IfcStore model, int ProductLabel)
    {
        var AttachedProduct = model.Instances.FirstOrDefault<Xbim.Ifc4.Kernel.IfcObjectDefinition>(d => d.EntityLabel == ProductLabel);
        Debug.Log(AttachedProduct.Name);
        return AttachedProduct;
    }

    List<Xbim.Ifc4.PropertyResource.IfcPropertySingleValue> createProperties(IfcStore model, Damage dmg)
    {
        List < Xbim.Ifc4.PropertyResource.IfcPropertySingleValue > properties = new List <Xbim.Ifc4.PropertyResource.IfcPropertySingleValue>();
        foreach (DamageProperty measurement in dmg.getMeasurements())
        {
            properties.Add(
                model.Instances.New<Xbim.Ifc4.PropertyResource.IfcPropertySingleValue>(p =>
                {
                    p.Name = new Xbim.Ifc4.MeasureResource.IfcIdentifier(measurement.Property_Name);
                    p.NominalValue = measurement.getIFCUnit();
                })
            );
        }

        return properties;
    }

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

