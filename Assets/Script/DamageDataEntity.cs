using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;
using System.Text.RegularExpressions;

public class DamageDataEntity : DimModel
{
    private IfcModel ifcModel;

    public DamageDataEntity(CoreApplication app, DimController controller, string filePath, IfcModel ifcModel) : base(app, controller)
    {
        // Assign Ifc Model root
        this.ifcModel = ifcModel;
        
        // Explore damage model
        getDamageModel(filePath);
    }

    protected void getDamageModel(string filePath)
    {
        if (filePath.Length != 0)
        {
            DamageViewModel damageViewModel = new DamageViewModel(ifcModel, filePath);
            this.app.Notify(controller: controller, message: DimNotification.DamageLoaded, parameters: damageViewModel);
        }
    }
}

public class DamageGroupModel
{
    IIfcTypeObject ObjectType;
    List<IIfcRelDefinesByType> RelDefinesByTypes;
    IEnumerable<int> EntityLabels;

    public DamageGroupModel(IIfcTypeObject ObjectType)
    {
        this.ObjectType = ObjectType;
        this.RelDefinesByTypes = this.ObjectType.Types
            .ToList<IIfcRelDefinesByType>();

        if (RelDefinesByTypes != null)
        {
            foreach (var RelDefinesByType in RelDefinesByTypes)
            {
                var _EntityLabels = RelDefinesByType.RelatedObjects
                    .Select(dmg => dmg.EntityLabel);

                if (EntityLabels == null) EntityLabels = _EntityLabels;
                else EntityLabels.Concat(_EntityLabels);
            }
        this.EntityLabels = EntityLabels;
        }
    }

    public IIfcTypeObject TypeObject
    {
        get {  return ObjectType; }
    }

    public IIfcRelDefinesByType ByTypesRelDefines
    {
        get { return RelDefinesByTypes.First(); }
    }

    public List<int> Items
    {
        get { return EntityLabels.ToList<int>(); }
    }
}

// Damage Model Container
// Wrapping all the damage models
public class DamageViewModel
{
    private IfcModel rootModel;
    private string filePath;
    // Existing Damage Types
    private ObservableCollection<DamageModel> damageTypes = new ObservableCollection<DamageModel>();

    public DamageViewModel(IfcModel rootModel, string filePath)
    {
        this.rootModel = rootModel;
        this.filePath = filePath;
        setup();
        damageTypes.CollectionChanged += damageTypesUpdate;
    }

    protected void setup()
    {
        using (var Model = IfcStore.Open(filePath))
        {
            // Filter damage entities
            var Damages = Model.Instances.OfType<IIfcTypeObject>()
                .Where(dmg => dmg.ApplicableOccurrence.HasValue)
                .Where(dmg => dmg.ApplicableOccurrence.Value.Equals("IfcProxy/Defect"))
                .Select(dmg => new DamageGroupModel(dmg))
                .ToList<DamageGroupModel>();

            if (Damages.Count > 0) this.AddTypes(Damages);
        }
    }

    private void damageTypesUpdate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Debug.Log("Damage Types updated.");
    }

    public List<DamageModel> DamageModels
    {
        get { return new List<DamageModel>(damageTypes); }
    }

    public void AddTypes(List<DamageGroupModel> damageGroupModels)
    {
        foreach (var damageGroupModel in damageGroupModels)
            AddType(damageGroupModel);
    }

    public void AddType(DamageGroupModel damageGroupModel)
    {
        damageTypes.Add(new DamageType(damageGroupModel, rootModel));
    }

    public void AddItem(DamageModel damageModel)
    {
        try
        {
            var newDamageType = damageTypes.Single(dmg => dmg.DefectType == damageModel.DefectType);
            (newDamageType as DamageType).Add(damageModel);
        }
        catch (System.Exception)
        {
            damageTypes.Add(new DamageType(damageModel));
        }
    }

    public void writeIfcFIle()
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

                foreach (var damageType in damageTypes)
                {
                    if (damageType.ObjectType == null)
                    {
                        damageType.ObjectType = new Create(model).TypeObject(p =>
                        {
                            p.GlobalId = System.Guid.NewGuid();
                            p.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(damageType.Name);
                            p.ApplicableOccurrence = new Xbim.Ifc4.MeasureResource.IfcIdentifier("IfcProxy/Defect");
                        });

                        //create relationship between proxy and ObjectType
                        damageType.ByTypesRelDefines = new Create(model).RelDefinesByType(r =>
                        {
                            r.GlobalId = System.Guid.NewGuid();
                            r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage type");
                            r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Typification of a defect");
                            // r.RelatedObjects.Add(_proxy);
                            if (damageType.ObjectType != null) r.RelatingType = damageType.ObjectType;
                        });
                    }

                    else
                    {
                        // Referesh to current version of model
                        damageType.ObjectType = model.Instances.OfType<IIfcTypeObject>()
                                    .Single(dmg => dmg.EntityLabel == damageType.EntityLabel);

                        damageType.ByTypesRelDefines = model.Instances.OfType<IIfcRelDefinesByType>()
                                    .Single(dmg => dmg.EntityLabel == damageType.ByTypesRelDefines.EntityLabel);
                    }

                    // The product attached to Type
                    foreach (var damageInstance in damageType.Children)
                    {
                        if (damageInstance.isOriginal)
                        {
                            damageType.ObjectType = model.Instances.OfType<IIfcTypeObject>()
                                    .Single(dmg => dmg.EntityLabel == damageType.EntityLabel);
                        }

                        else
                        {
                            var _proxy = new Create(model).BuildingElementProxy( p =>
                            {
                                p.GlobalId = System.Guid.NewGuid();
                                p.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(damageInstance.Name4Ifc);
                                p.Description = new Xbim.Ifc4.MeasureResource.IfcText(damageInstance.Description);
                                p.ObjectPlacement = getAttachedPlacement(model, damageInstance);
                            });


                            //create relationship between proxy and ifcProduct

                            // Check if Parent already has relationship
                            var ParentEntity = getAttachedProduct(model, damageInstance);

                            if (ParentEntity != null)
                            {
                                var _RelAggrates = (ParentEntity as IIfcObject).IsDecomposedBy
                                        .OfType<IIfcRelAggregates>()
                                        .Where(dmg => dmg.Name.HasValue)
                                        .Single(dmg => (dmg.Name.Value.Value as string) == "Damage to product");

                                if (_RelAggrates != null) _RelAggrates.RelatedObjects.Add(_proxy);
                            }

                            else
                            {
                                var _RelAggrates = new Create(model).RelAggregates(r =>
                                {
                                    r.GlobalId = System.Guid.NewGuid();
                                    r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Damage to product");
                                    r.Description = new Xbim.Ifc4.MeasureResource.IfcText("The related product is damaged");
                                    r.RelatingObject = ParentEntity;
                                    r.RelatedObjects.Add(_proxy);
                                });
                            }

                            damageType.ByTypesRelDefines.RelatedObjects.Add(_proxy);

                            if (damageInstance.Entity == null)
                            {
                                damageInstance.IfcProperties = new Create(model).PropertySet(pset =>
                                {
                                    pset.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Pset_Condition");
                                });

                                var RefRelationProperties = new Create(model).RelDefinesByProperties(rel =>
                                {
                                    rel.GlobalId = System.Guid.NewGuid();
                                    rel.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Defect Measurements");
                                    rel.Description = new Xbim.Ifc4.MeasureResource.IfcText("Property parameters for Defect");

                                    rel.RelatedObjects.Add(_proxy);
                                    rel.RelatingPropertyDefinition = damageInstance.IfcProperties;
                                });
                            }

                            if (damageInstance.hasContent)
                            {
                                if (damageInstance.hasImage)
                                {
                                    //create reference document metaData
                                    var _docInformation = new Create(model).DocumentInformation(r =>
                                    {
                                        r.Identification = new Xbim.Ifc4.MeasureResource.IfcIdentifier(System.Guid.NewGuid().ToString());
                                        r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("MetaData");
                                        r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Associate information for image file.");
                                        r.Purpose = new Xbim.Ifc4.MeasureResource.IfcText("For external reference if any.");
                                        r.Location = new Xbim.Ifc4.ExternalReferenceResource.IfcURIReference(damageInstance.ImageMetaLocation);
                                        r.ElectronicFormat = new Xbim.Ifc4.MeasureResource.IfcIdentifier("Stl");
                                    });

                                    // create reference document
                                    // build based on meta data information
                                    var _imagePropertySet = new Create(model).PropertySet(pset =>
                                    {
                                        pset.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Image Properties");

                                        foreach (var property in damageInstance.ImageProperties)
                                            pset.HasProperties.Add(property.DIM2IfcProperty(model));
                                    });

                                    //create reference document
                                    var _docReference = new Create(model).DocumentReference(r =>
                                    {
                                        r.Location = new Xbim.Ifc4.ExternalReferenceResource.IfcURIReference(damageInstance.ImageLocation);
                                        r.Identification = new Xbim.Ifc4.MeasureResource.IfcIdentifier(System.Guid.NewGuid().ToString());
                                        r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel(damageInstance.ImageName);
                                        r.Description = new Xbim.Ifc4.MeasureResource.IfcText(damageInstance.ImageDescription);
                                        r.ReferencedDocument = _docInformation;
                                    });

                                    //create relationship between proxy and external source
                                    var _relAssociateDoc = new Create(model).RelAssociatesDocument(r =>
                                    {
                                        r.GlobalId = System.Guid.NewGuid();
                                        r.Name = new Xbim.Ifc4.MeasureResource.IfcLabel("Related Parameters");
                                        r.Description = new Xbim.Ifc4.MeasureResource.IfcText("Connect related parameters for the reference document.");
                                        r.RelatedObjects.Add(_proxy);
                                        r.RelatedObjects.Add(_imagePropertySet);
                                        r.RelatingDocument = _docReference;
                                    });
                                }
                            }
                        }

                        var _properties = damageInstance.IfcProperties.HasProperties;
                        _properties.Clear();

                        foreach (var property in damageInstance.Properties)
                            _properties.Add(property.DIM2IfcProperty(model));
                    }
                }

                //commit changes
                txn.Commit();
            }

            model.SaveAs(NewPath);
        }
    }

    IIfcObjectDefinition getAttachedProduct(IfcStore model, DamageModel damageInstance)
    {
        var ProductLabel = damageInstance.IfcParent.EntityLabel;
        var AttachedProduct = model.Instances.FirstOrDefault<IIfcObjectDefinition>(d => d.EntityLabel == ProductLabel);
        Debug.Log(AttachedProduct.Name);

        return AttachedProduct;
    }

    IIfcObjectPlacement getAttachedPlacement(IfcStore model, DamageModel damageInstance)
    {
        var ProductLabel = damageInstance.IfcParent.EntityLabel;
        var AttachedProduct = model.Instances.FirstOrDefault<IIfcProduct>(d => d.EntityLabel == ProductLabel);
        Debug.Log(AttachedProduct.Name);

        return AttachedProduct.ObjectPlacement;
    }
}

public class DamageModel
{
    public event System.EventHandler OnSelectChanged; // event

    public void OnSelected(System.EventArgs e) //protected virtual method
    {
        OnSelectChanged?.Invoke(this, e);
    }

    public virtual DamageModel Parent { get; }

    public IIfcRelDefinesByType ByTypesRelDefines { get; set; }

    protected List<DamageModel> children = new List<DamageModel>();

    protected DamageModel() { }

    public DamageTypes DefectType { get; set; }

    public virtual IfcModel IfcParent { get; }

    public virtual IXbimViewModel Entity { get; }

    public IIfcPropertySet IfcProperties { get; set; }

    public int DefectTypeOption
    {
        get
        {
            List<DamageTypes> damageTypes = System.Enum.GetValues(typeof(DamageTypes))
                .Cast<DamageTypes>()
                .ToList();
            Debug.LogFormat("Type of {0} is {1}", this.Name, this.DefectType.ToString());
            return damageTypes.IndexOf(DefectType);
        }
    }

    public IIfcTypeObject ObjectType { get; set; }

    public static List<string> DamageTypes
    {
        get
        {
            List<string> damageTypes = System.Enum.GetValues(typeof(DamageTypes))
                                        .Cast<DamageTypes>()
                                        .Select(d => (d.ToString()))
                                        .ToList();

            return damageTypes;
        } 
    }

    public static List<string> IfcValueTypes
    {
        get
        {
            var _IfcValueTypes = GetInheritedClasses(typeof(Xbim.Ifc4.MeasureResource.IfcValue))
                .Select(d => (d.Name))
                .ToList();

            return _IfcValueTypes;
        }
    }

    public static System.Type IfcValueType (int index)
    {
        var _IfcValueTypes = GetInheritedClasses(typeof(Xbim.Ifc4.MeasureResource.IfcValue))
                .ToArray();

        return _IfcValueTypes[index];
    }

    static IEnumerable<System.Type> GetInheritedClasses(System.Type _type)
    {
        var Types = System.Reflection.Assembly.GetAssembly(_type).GetTypes()
            .Where(type => _type.IsAssignableFrom(type));

        return Types;
    }

    public virtual GameObject Geometry
    {
        get { return null; }
    }

    public virtual bool isOriginal { get; }

    public virtual string Name4Ifc { get; }

    public virtual string Name { get; set; }

    public virtual string Description { get; set; }

    public virtual string ParentName { get; }

    public virtual GameObject DefectLabelObject { get; set; }

    public virtual GameObject AttachedObject { get; }

    public virtual int EntityLabel
    {
        get { return 0; }
    }

    public virtual List<PropertyItem> Properties
    {
        get { return null; }
    }

    public virtual DamageModel[] Children
    {
        get { return children.ToArray(); }
    }

    public virtual bool is_Editable
    {
        get { return false; }
    }

    public virtual string[] ContentTypes
    {
        get { return null; }
    }

    public virtual ImageType ImageType
    {
        get { return ImageType.Dummy; }
    }

    public virtual string updateImageURL { set; get; }

    public virtual string ImageName
    {
        get { return System.String.Empty; }
    }

    public virtual string ImageDescription
    {
        get { return System.String.Empty; }
    }

    public virtual string ImageMetaLocation { get; }

    public virtual List<PropertyItem> ImageProperties { get; }

    public virtual string ImageUrl { get; }

    public virtual bool hasContent { get; }

    public virtual bool hasImage { get; }

    public virtual string ImageLocation { get; }

    public virtual GameObject ImageObject { get; set; }

    public virtual GameObject Image2DView { get; set; }

    public virtual Vector3 ImageOrigin { get; set; }

    public virtual Quaternion ImageRotation { get; set; }

    public virtual void SetImageProperties(string ImageName, string ImageDescription, string ImageURL)
    {
        
    }

    public virtual void AddProperty(PropertyItem property) { }

    public void showImage()
    {
        if (ImageObject != null) ImageObject.SetActive(true);
        if (Image2DView != null) Image2DView.SetActive(true);
    }

    public void hideImage()
    {
        if (ImageObject != null) ImageObject.SetActive(false);
        if (Image2DView != null) Image2DView.SetActive(false);
    }

    public virtual Texture2D Image2D { get; }
}

public enum DamageTypes
{
    Crack,
    Spalling,
    Rusting,
    Decolorisation,
    Vegetation
}

public class DamageType : DamageModel
{
    private IfcModel rootModel;

    // Properties for Damage Types
    private List<PropertyItem> TypeProperties = new List<PropertyItem>();

    public override int EntityLabel
    {
        get { return this.ObjectType.EntityLabel; }
    }

    private bool _IsTypeOriginal;

    public override bool isOriginal
    {
        get { return _IsTypeOriginal; }
    }

    private ObservableCollection<DamageModel> damageModels = new ObservableCollection<DamageModel>();

    public DamageType(DamageGroupModel damageGroupModel, IfcModel rootModel) : base()
    {
        this._IsTypeOriginal = true;
        this.ObjectType = damageGroupModel.TypeObject;
        this.ByTypesRelDefines = damageGroupModel.ByTypesRelDefines;
        this.rootModel = rootModel;

        if (ObjectType.Name.HasValue)
        {
            string ObjectName = ObjectType.Name.Value.Value as string;
            string pattern = "(?<=Damage type ).*";
            Match m = Regex.Match(ObjectName, pattern);

            if (m.Success)
            {
                var elementType = m.Value;
                // Debug.LogFormat("Damage Type Registered: {0}", elementType);
                this.DefectType = (DamageTypes)System.Enum.Parse(typeof(DamageTypes), elementType, true);
                Debug.LogFormat("********* Type {0} created. ************", this.DefectType.ToString());
            } 
        }

        TypeProperties.Add(new PropertyItem
        {
            IfcLabel = ObjectType.EntityLabel,
            Name = "Description",
            Value = this.Description
        });

        // Put in children Damage Model
        this.Add(damageGroupModel.Items);

        damageModels.CollectionChanged += damageModelsUpdate;
    }

    public DamageType(DamageModel _DamageInstance) : base()
    {
        this.DefectType = _DamageInstance.DefectType;

        TypeProperties.Add(new PropertyItem
        {
            Name = "Description",
            Value = System.String.Format("This an example case of {0}", this.DefectType.ToString("G"))
        });

        Add(_DamageInstance);

        damageModels.CollectionChanged += damageModelsUpdate;
    }

    private void damageModelsUpdate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Debug.Log("Damage Models updated.");
    }

    public void Add(List<int> EntityLabels)
    {
        foreach (int EntityLabel in EntityLabels)
        {
            this.Add(EntityLabel);
        }
    }

    public void Add(int EntityLabel)
    {
        IfcModel damageModel = IfcModel.getIfcModel(rootModel, EntityLabel);

        if (damageModel != null)
        {
            var damageInstance = new DamageInstance(damageModel);

            Debug.LogFormat("Damage Model: {0}, Type: {1}", damageInstance.Name, this.DefectType.ToString());

            damageInstance.DefectType = this.DefectType;
            damageModels.Add(damageInstance);
            damageInstance.DamageTypeObject = this;
        }   
    }

    public void Add(DamageModel _DamageInstance)
    {
        // Set Type Object
        if (_DamageInstance is DamageInstance)
            (_DamageInstance as DamageInstance).DamageTypeObject = this;

        // Check if list duplicate Object
        // Compare with main List
        if (damageModels.Contains(_DamageInstance)) return;
        else damageModels.Add(_DamageInstance);
    }

    public void Remove(DamageModel _DamageInstance)
    {
        Debug.LogFormat("Type {0} remove {1}", this.DefectType.ToString(), _DamageInstance.Name);
        damageModels.Remove(_DamageInstance);
    }

    public override string Name
    {
        get
        {
            if (ObjectType != null)
            {
                if (ObjectType.Name.HasValue)
                    return ObjectType.Name.Value.Value as string;
            }
            else if (DefectType != null)
            {
                var TypeName = System.String.Format("Damage Type {0}", DefectType.ToString("G"));
                return TypeName;
            }

            return "< Missing >";
        }
    }

    public override GameObject DefectLabelObject
    {
        get { return null; }
    }

    private string Description
    {
        get
        {
            if (ObjectType.Description.HasValue)
                return ObjectType.Description.Value.Value as string;

            else return "< Missing >";
        }
    }

    public override List<PropertyItem> Properties
    {
        get { return TypeProperties; }
    }

    public override DamageModel[] Children
    {
        get { return damageModels.ToArray(); }
    }
}

// Damage Model wrap around IFC model
public class DamageInstance : DamageModel
{
    // Damage Type of instances
    DamageType _DamageTypeObject;

    // If the damage is bind with Tree Item
    public bool is_bind = false;
    

    public DamageType DamageTypeObject
    {
        set
        {
            if (_DamageTypeObject == null) _DamageTypeObject = value;
            else if (_DamageTypeObject != value)
            {
                _DamageTypeObject.Remove(this);
                _DamageTypeObject = value;
            }
        }
    }

    public override DamageModel Parent
    {
        get { return _DamageTypeObject; }
    }

    public override IfcModel IfcParent
    {
        get { return this.ifcModel.Parent; }
    }

    public override IXbimViewModel Entity
    {
        get { return this.ifcModel.Entity; }
    }

    private bool writable = true;

    private GameObject Damage_Geometry;

    private DamageContent _DamageContent;

    private IfcModel ifcModel;

    public override bool isOriginal
    {
        get { return ifcModel.isOriginal; }
    }

    public DamageInstance(IfcModel ifcModel): base()
    {
        this.ifcModel = ifcModel;

        if (Entity != null)
        {
            var _IfcObject = Entity.Entity;
            if (_IfcObject is IIfcObject)
            {
                var asIfcObject = _IfcObject as IIfcObject;

                this.IfcProperties = asIfcObject.IsDefinedBy
                    .Select(relDef => relDef.RelatingPropertyDefinition as IIfcPropertySet)
                    .Single(relDef => relDef != null);
            }
        }
    }

    public static DamageInstance CreateInstance(IfcModel ifcModel)
    {
        var newModel = new IfcModel();
        ifcModel.AddInstance(newModel);
        return new DamageInstance(newModel);
    }

    private void LazyLoadAll(DamageModel parent)
    {
        children = ifcModel.Children.Select(child => new DamageInstance(child))
            .ToList<DamageModel>();
    }

    public override GameObject Geometry
    {
        get { return Damage_Geometry; }
    }

    public override string Name4Ifc
    {
        get { return ifcModel.Name; }
    }

    public override string Name
    {
        get
        {
            if (isOriginal) return ifcModel.Name;
            else return ifcModel.Name + " [Edit]";
        }

        set { ifcModel.Name = value; }
    }

    public override string Description
    {
        get { return ifcModel.Description; }

        set { ifcModel.Description = value; }
    }

    public override string ParentName
    {
        get
        {
            if (ifcModel.Parent != null) return ifcModel.Parent.Name;
            else return string.Empty;
        }
    }

    private GameObject _DefectLabelObject;

    public override GameObject DefectLabelObject
    {
        get { return _DefectLabelObject; }

        set
        {
            _DefectLabelObject = value;
            var damageLabel = _DefectLabelObject.GetComponent<DamageLabel>();
            damageLabel.DefectModel = this;
        }
    }

    public override GameObject AttachedObject
    {
        get
        {
            var attachedObject = this.ifcModel.Parent.Geometry;
            return attachedObject;
        }
    }

    public override int EntityLabel
    {
        get { return ifcModel.EntityLabel; }
    }

    public override List<PropertyItem> Properties
    {
        get
        {
            if (_DamageContent != null)
                return _DamageContent.Properties;

            else return ifcModel.Properties;
        }
    }

    public override DamageModel[] Children
    {
        get { return children.ToArray(); }
    }

    public override bool is_Editable
    {
        get { return writable; }
    }

    public override string ImageName
    {
        get
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                return _DamageImage.imageName;
            }
            else return "< Missing >";
        }
    }

    public override string ImageDescription
    {
        get
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                return _DamageImage.imageDescription;
            }
            else return "< Missing >";
        }
    }

    public override bool hasContent
    {
        get { return _DamageContent != null; }
    }

    public override bool hasImage
    {
        get { return _DamageContent is DamageImage; }
    }

    public override GameObject ImageObject
    {
        get
        {
            if (_DamageContent != null)
            {
                if (_DamageContent is DamageImage)
                {
                    var _DamageImage = _DamageContent as DamageImage;
                    return _DamageImage.imageObject;
                }
                else return null;
            }
            else return null;
        }

        set
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                _DamageImage.imageObject = value;

                if (this.ImageType == ImageType.Image_3D)
                { 
                    _DamageImage.imageObject.layer = 9;
                }
            }  
        }
    }

    public void setImage(ImageType imageType)
    {
        if (_DamageContent == null) _DamageContent = new DamageImage(ifcModel, imageType);
    }

    public override void SetImageProperties(string ImageName, string ImageDescription, string ImageURL)
    {
        if (_DamageContent != null)
        {
            var _DamageImage = _DamageContent as DamageImage;

            if (_DamageImage.imageName == null) _DamageImage.imageName = ImageName;
            if (_DamageImage.imageDescription == null) _DamageImage.imageDescription = ImageDescription;
            if (_DamageImage.imageURL == null) _DamageImage.imageURL = ImageURL;
        }
    }

    public override string ImageMetaLocation
    {
        get
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;

                if (_DamageImage.imageMetaURL != null)
                {
                    return _DamageImage.imageMetaURL;
                }

                else return "< Missing >";
            }

            else return "< Missing >";
        }
    }

    public override string ImageLocation
    {
        get
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                return _DamageImage.imageURL;
            } 

            else return "< Missing >";
        }
    }

    public override string updateImageURL
    {
        set
        {
            var _DamageImage = _DamageContent as DamageImage;

            if (string.Compare(_DamageImage.imageURL, value) != 1)
                _DamageImage.imageURL = value;
        }
    }

    public override List<PropertyItem> ImageProperties
    {
        get
        {
            if (_DamageContent != null)
            {
                return _DamageContent.ImageProperties;
                
            }

            else return null;
        }
    }

    public override Vector3 ImageOrigin
    {
        get
        {
            if (_DamageContent != null)
            {
                if (_DamageContent is DamageImage)
                {
                    var _DamageImage = _DamageContent as DamageImage;
                    return _DamageImage.Location;
                }

                else
                {
                    return ifcModel.Parent.position;
                }
            }

            else return Vector3.one;
            
        }

        set
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                _DamageImage.Location = value;
                Debug.LogFormat("Image Origin {0} is written.", _DamageImage.Location);
            }
        }
    }

    public override Quaternion ImageRotation
    {
        get
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                return _DamageImage.imageRotation;
            }
            else return Quaternion.identity;
        }

        set
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                _DamageImage.imageRotation = value;
            }
        }
    }
    
    public void setText()
    {
        if (_DamageContent == null) _DamageContent = new DamageText(ifcModel);
    }

    public override string[] ContentTypes
    {
        get { return _DamageContent.ContentTypes; }
    }

    public override ImageType ImageType
    {
        get { return _DamageContent.ImageType; }
    }

    public override void AddProperty(PropertyItem property)
    {
        if (_DamageContent != null) _DamageContent.AddProperty(property);
        else
        {
            _DamageContent = new DamageText(ifcModel);
            _DamageContent.AddProperty(property);
        }
    }

    public override Texture2D Image2D
    {
        get
        {
            Debug.Log("Image Returned.");
            if (_DamageContent != null)
            {
                if (_DamageContent is DamageImage)
                {
                    Debug.Log("Image Found.");
                    return (_DamageContent as DamageImage).image2D;
                }
            }

            return null;
        }
    }
}

// Damage Content newly built
public class DamageContent
{
    protected DataExternalDocument externalDocument;

    protected GameObject Damage_Geometry;

    protected IfcModel ifcModel;

    // List of new text properties
    protected ObservableCollection<PropertyItem> NewProperties;

    // List for image properties only
    public virtual List<PropertyItem> ImageProperties { get; }

    public DamageContent(IfcModel ifcModel)
    {
        this.ifcModel = ifcModel;
        NewProperties = new ObservableCollection<PropertyItem>(this.ifcModel.Properties);

        foreach (var property in NewProperties)
            Debug.Log(property.ToString());

        NewProperties.CollectionChanged += OnPropertyChanged;
    }

    void OnPropertyChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
        
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (PropertyItem addProperty in e.NewItems)
            {
                Debug.LogFormat("Property Added: {0}", addProperty.ToString());
                this.ifcModel.AddProperty(addProperty);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (PropertyItem removeProperty in e.OldItems)
            {
                Debug.LogFormat("Property Removed: {0}", removeProperty.ToString());
                this.ifcModel.RemoveProperty(removeProperty);
            }
        }
    }

    public virtual string[] ContentTypes
    {
        get { return null; }
    }

    public virtual ImageType ImageType
    {
        get { return ImageType.Dummy; }
    }

    public void AddProperty(PropertyItem property)
    {
        NewProperties.Add(property);
    }

    public List<PropertyItem> Properties
    {
        get { return new List<PropertyItem>(NewProperties); }
    }

    public virtual List<PropertyItem> imageProperties { get; }

    public virtual Texture2D image2D { get; }

    public Vector3 Location { get; set; }
}

// Damage Content based on Material Image
public class DamageImage: DamageContent
{
    private ImageType imageType;

    public string imageName { get; set; }
    public string imageDescription { get; set; }

    string _imageURL;
    public string imageURL
    {
        get
        {
            if (_imageURL != null)
                return _imageURL;
            else
                return "< Missing >";
        }

        set
        {
            _imageURL = value;
            imageMetaURL = System.IO.Path.ChangeExtension(_imageURL, ".itd");
        }
    }

    public string imageMetaURL { get; private set; }

    public Quaternion imageRotation { get; set; }
    public GameObject imageObject { get; set; }
    

    public override List<PropertyItem> imageProperties
    {
        get { return null; }
    }

    public override Texture2D image2D
    {
        get
        {
            if (imageURL != null)
            {
                try
                {
                    Texture2D imageTexture = new Texture2D(400, 400);
                    var image_bytes = System.IO.File.ReadAllBytes(this.imageURL);
                    imageTexture.LoadImage(image_bytes);

                    return imageTexture;
                }
                catch (System.Exception)
                {

                    return null;
                }
            }
            return null;
        }
    }

    public DamageImage(IfcModel ifcModel, ImageType imageType) : base(ifcModel)
    {
        this.imageType = imageType;
        Debug.LogFormat("{0} instance is created.", this.imageType.ToString());
    }

    public override ImageType ImageType
    {
        get { return imageType; }
    }

    public override string[] ContentTypes
    {
        get
        {
            switch (imageType)
            {
                case ImageType.Image_1D:
                    return new string[] { "png", "jpg" };
                    break;

                case ImageType.Image_2D:
                    return new string[] { "png", "jpg" };
                    break;

                case ImageType.Image_3D:
                    return new string[] { "stl" };
                    break;
            }

            return null;
        }
    }
}

// Damage Content based on Text Properties
public class DamageText : DamageContent
{
    public DamageText(IfcModel ifcModel) : base(ifcModel)
    {
    }
    
}

public enum ImageType
{
    Dummy,
    Image_1D,
    Image_2D,
    Image_3D
}