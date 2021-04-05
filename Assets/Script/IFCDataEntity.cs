using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;

public class IFCDataEntity : DimModel
{
    ObjectBinding ObjectBindingProperty = new ObjectBinding();

    public IFCDataEntity(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    public void getIfcModel(string filePath)
    {
        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                IEnumerable<IXbimViewModel> ifcItems = setup(model);
                this.app.Notify(controller: controller, message: DimNotification.IfcLoaded, parameters: ifcItems);
                this.app.Notify(controller: controller, message: DimNotification.ObjectDataBinding, parameters: ObjectBindingProperty);
            }
        }
    }

    protected virtual IEnumerable<IXbimViewModel> setup(IfcStore Model)
    {
        var project = Model.Instances.OfType<IIfcProject>().FirstOrDefault();

        if (project != null)
        {
            ObservableCollection<XbimModelViewModel> svList = new ObservableCollection<XbimModelViewModel>();
            svList.Add(new XbimModelViewModel(project, null));
            IEnumerable<IXbimViewModel> ifcItems = svList;

            foreach (var child in svList)
                LazyLoadAll(child);

            return ifcItems;
        }
        else return null;
    }

    protected void LazyLoadAll(IXbimViewModel parent)
    {
        foreach (var child in parent.Children)
        {
            if (typeof(IIfcElement).IsAssignableFrom(child.Entity.ExpressType.Type))
                ObjectBindingProperty.Register(child);

            LazyLoadAll(child);
        }
        //ifcInteract.FillData(parent.Entity);
    }
}
