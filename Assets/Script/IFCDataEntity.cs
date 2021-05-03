using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;

public class IFCDataEntity : DimModel
{
    public IFCDataEntity(CoreApplication app, DimController controller, string filePath) : base(app, controller)
    {
        getIfcModel(filePath);
    }

    protected void getIfcModel(string filePath)
    {
        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                // Build ifcItem tree hierachy
                IfcModel ifcRoot = setup(model);

                // Asign Location
                BIMPlacement.extractLocation(ifcRoot);

                this.app.Notify(controller: controller, message: DimNotification.IfcLoaded, parameters: ifcRoot);
            }
        }
    }

    protected virtual IfcModel setup(IfcStore Model)
    {
        var project = Model.Instances.OfType<IIfcProject>().FirstOrDefault();

        if (project != null)
        {
            IfcModel ifc_root_model = new IfcModel(new XbimModelViewModel(project, null));

            return ifc_root_model;
        }
        else return null;
    }
}


// Ifc wrapper data model for entity model
public class IfcModel
{
    private bool has_Geometry = false;

    private string EntityName;
    private string EntityDescription;

    private GameObject Ifc_Geometry;
    private List<PropertyItem> Ifc_Semantic;
    private IXbimViewModel Ifc_Entity;

    private IfcModel parent;
    private List<IfcModel> children = new List<IfcModel>();

    public event System.EventHandler OnSelectChanged; // event

    public void OnSelected(System.EventArgs e) //protected virtual method
    {
        OnSelectChanged?.Invoke(this, e);
    }

    public IfcModel(IXbimViewModel Ifc_Entity)
    {
        this.Ifc_Entity = Ifc_Entity;
        Ifc_Semantic = IFCDataProperty.GetProperties(Ifc_Entity.Entity);

        LazyLoadAll(this);
    }

    public IfcModel()
    {

    }

    private void LazyLoadAll(IfcModel parent)
    {
        // create IfcModel for children
        // Recursive loop for explorative children assignment
        this.children = Ifc_Entity.Children.Select(child => new IfcModel(child))
            .ToList<IfcModel>();

        foreach (var child in children)
        {
            child.Parent = parent;
        }
    }

    public void AddInstance(IfcModel newModel)
    {
        children.Add(newModel);
        newModel.Parent = this;
    }

    MouseHighlight mouseHighlight;

    public void assignGeometric(GameObject Ifc_Geometry)
    {
        this.Ifc_Geometry = Ifc_Geometry;
        var mouseHighlight = Ifc_Geometry.AddComponent<MouseHighlight>();
        mouseHighlight.setIfcModel(this);
        has_Geometry = true;
    }

    public bool Selected
    {
        get { return mouseHighlight.highlight; }

        set
        {
            mouseHighlight = Ifc_Geometry.GetComponent<MouseHighlight>();
            mouseHighlight.highlight = value;
        }
    }

    public GameObject Geometry
    {
        get { return Ifc_Geometry; }
    }

    public string Name
    {
        get
        {
            if (Ifc_Entity != null) return Ifc_Entity.Name;
            else if (EntityName != null) return EntityName;
            else return string.Empty;
        }

        set { EntityName = value;  }
    }

    public string Description
    {
        get
        {
            if (EntityDescription != null) return EntityDescription;
            else return string.Empty;
        }

        set { EntityDescription = value; }
    }

    public int EntityLabel
    {
        get
        {
            if (Ifc_Entity != null) return Ifc_Entity.EntityLabel;
            else return 0;
        }
    }

    public IXbimViewModel Entity
    {
        get { return Ifc_Entity; }
    }

    public List<PropertyItem> Properties
    {
        get { return Ifc_Semantic; }
    }

    public IfcModel Parent { get; private set; }

    public IfcModel[] Children
    {
        get { return children.ToArray(); }
    }

    public bool is_Annotatable
    {
        get { return has_Geometry; }
    }

    public bool is_Editable
    {
        get { return has_Geometry; }
    }

    public Vector3 localPosition { get; set; }

    public Vector3 position { get; set; }

    public static void attachGeometry(IfcModel ifcModel, List<GameObject> visualItems)
    {
        string entity_label = System.String.Format("id-{0}", ifcModel.EntityLabel);

        var attachedVisualItems = from visualItem in visualItems
                                 where System.String.Compare(visualItem.name, entity_label) == 0
                                 select visualItem;

        if (attachedVisualItems != null)
        {
            foreach (var attachedVisualItem in attachedVisualItems)
                ifcModel.assignGeometric(attachedVisualItem);
        }

        if (ifcModel.Children.Length > 0)
        {
            foreach (IfcModel child in ifcModel.Children)
            {
                IfcModel.attachGeometry(child, visualItems);
            }
        }

        return;
    }

    private bool checkModel (int EntityLabel)
    {
        return this.EntityLabel == EntityLabel;
    }

    public XbimPlacementNode placementNode { get; set; }

    public static IfcModel getIfcModel(IfcModel rootModel, int EntityLabel)
    {
        if (rootModel.checkModel(EntityLabel))
        {
            return rootModel;
        }

        else if (rootModel.Children.Length > 0)
        {
            foreach (IfcModel model in rootModel.Children)
            {
                IfcModel searchedModel = IfcModel.getIfcModel(model, EntityLabel);

                if (searchedModel != null) return searchedModel;
            } 
        }

        return null;
    }
}


