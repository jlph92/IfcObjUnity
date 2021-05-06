using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            using (var model = IfcStore.Open(filePath))
            {
                DamageViewModel damageViewModel = setup(model);
                this.app.Notify(controller: controller, message: DimNotification.DamageLoaded, parameters: damageViewModel);
            }
        }
    }

    protected virtual DamageViewModel setup(IfcStore Model)
    {
        // Filter damage entities
        var Damages = Model.Instances.OfType<IIfcRelDefinesByType>()
            .Where(dmg => dmg.RelatingType.ApplicableOccurrence.HasValue)
            .Where(dmg => dmg.RelatingType.ApplicableOccurrence.Value.Equals("IfcProxy/Defect"))
            .GroupBy(dmg => dmg.RelatingType as IIfcTypeObject,
            (dmgType, dmgObj) => new DamageGroupModel
            (
                ObjectType: dmgType,
                EntityLabels: dmgObj.SelectMany(dmg => dmg.RelatedObjects.Select(itm => itm.EntityLabel)
            )
            ))
            .ToList<DamageGroupModel>();
        
        // Initialise Damage View Model
        DamageViewModel damageViewModel = new DamageViewModel(ifcModel);
        if (Damages.Count > 0) damageViewModel.AddTypes(Damages);

        return damageViewModel;
    }
}

public class DamageGroupModel
{
    IIfcTypeObject ObjectType;
    IEnumerable<int> EntityLabels;

    public DamageGroupModel(IIfcTypeObject ObjectType, IEnumerable<int> EntityLabels)
    {
        this.ObjectType = ObjectType;
        this.EntityLabels = EntityLabels;
    }

    public IIfcTypeObject TypeObject
    {
        get {  return ObjectType; }
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
    private ObservableCollection<DamageModel> damageTypes = new ObservableCollection<DamageModel>();

    public DamageViewModel(IfcModel rootModel)
    {
        this.rootModel = rootModel;
        damageTypes.CollectionChanged += damageTypesUpdate;
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
}

public class DamageModel
{
    protected List<DamageModel> children = new List<DamageModel>();

    protected DamageModel() { }

    public DamageTypes DefectType { get; set; }

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

    public virtual string Name { get; set; }

    public virtual string Description { get; set; }

    public virtual string ParentName { get; }

    public virtual GameObject DefectLabelObject { get; set; }

    public virtual GameObject AttachedObject { get; }

    public virtual int EntityLabel
    {
        get { return 0; }
    }

    public virtual IXbimViewModel Entity
    {
        get { return null; }
    }

    public virtual List<PropertyItem> Properties
    {
        get { return null; }
    }

    public virtual List<PropertyItem> NewProperties
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

    public virtual string ImageName
    {
        get { return System.String.Empty; }
    }

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
    private IIfcTypeObject ObjectType;

    // Properties for Damage Types
    private List<PropertyItem> TypeProperties = new List<PropertyItem>();

    private ObservableCollection<DamageModel> damageModels = new ObservableCollection<DamageModel>();

    public DamageType(DamageGroupModel damageGroupModel, IfcModel rootModel) : base()
    {
        this.ObjectType = damageGroupModel.TypeObject;
        this.rootModel = rootModel;
        this.Add(damageGroupModel.Items);

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
            } 
        }

        TypeProperties.Add(new PropertyItem
        {
            IfcLabel = ObjectType.EntityLabel,
            Name = "Description",
            Value = this.Description
        });

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
            damageModels.Add(new DamageInstance(damageModel));
    }

    public void Add(DamageModel _DamageInstance)
    {
        damageModels.Add(_DamageInstance);
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

            return "Undefined";
        }
    }

    public override GameObject DefectLabelObject
    {
        get { return null; }
    }

    public override int EntityLabel
    {
        get
        {
            if (ObjectType != null) return ObjectType.EntityLabel;
            else return 1;
        }
    }

    private string Description
    {
        get
        {
            if (ObjectType.Description.HasValue)
                return ObjectType.Description.Value.Value as string;

            else return "Undefined";
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
    private bool writable = true;

    private GameObject Damage_Geometry;

    private DamageContent _DamageContent;

    private IfcModel ifcModel;

    public DamageInstance(IfcModel ifcModel): base()
    {
        this.ifcModel = ifcModel;
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

    public override string Name
    {
        get { return ifcModel.Name; }

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

    public override IXbimViewModel Entity
    {
        get { return ifcModel.Entity; }
    }

    public override List<PropertyItem> Properties
    {
        get { return ifcModel.Properties; }
    }

    public override List<PropertyItem> NewProperties
    {
        get
        {
            if (_DamageContent != null)
                return _DamageContent.Properties;

            else return null;
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
            else return System.String.Empty;
        }
    }

    public override GameObject ImageObject
    {
        get
        {
            if (_DamageContent != null)
            {
                var _DamageImage = _DamageContent as DamageImage;
                return _DamageImage.imageObject;
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
            _DamageImage.imageName = ImageName;
            _DamageImage.imageDescription = ImageDescription;
            _DamageImage.imageURL = ImageURL;
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
                    return _DamageImage.imageOrigin;
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
                _DamageImage.imageOrigin = value;
                Debug.LogFormat("Image Origin {0} is written.", _DamageImage.imageOrigin);
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
    protected List<PropertyItem> NewProperties = new List<PropertyItem>();

    public DamageContent(IfcModel ifcModel)
    {
        this.ifcModel = ifcModel;
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
        get { return NewProperties; }
    }

    public Texture2D image2D { get; }
}

// Damage Content based on Material Image
public class DamageImage: DamageContent
{
    private ImageType imageType;

    public string imageName { get; set; }
    public string imageDescription { get; set; }
    public string imageURL { get; set; }
    public Vector3 imageOrigin { get; set; }
    public Quaternion imageRotation { get; set; }
    public GameObject imageObject { get; set; }

    public Texture2D image2D
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