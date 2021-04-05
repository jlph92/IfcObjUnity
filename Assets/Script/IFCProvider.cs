using System.Collections.Generic;
using Xbim.Ifc.ViewModels;

public class IFCProvider : DimController
{
    string filePath;

    public IFCProvider(CoreApplication app, string filePath) : base(app)
    {
        this.filePath = filePath;
    }

    public void setView(IFCTreeVisualization UIView)
    {
        UnityEngine.Debug.Log("View is set.");
        this.view = UIView;
    }

    public override void notify(string message, params object[] parameters)
    {
        UnityEngine.Debug.Log(message);

        switch (message)
        {
            case DimNotification.LoadIFCData:
                LoadIfcData();
                break;

            case DimNotification.IfcLoaded:
                updateTreeView(parameters[0] as IEnumerable<IXbimViewModel>);
                break;

            case DimNotification.ObjectDataBinding:
                //LoadObjFile(parameters[0] as ObjectBinding);
                break;

            case DimNotification.LoadIFCProperty:
                //LoadObjFile(parameters[0] as System.String);
                break;

            case DimNotification.AddDim:
                //finishLoad(parameters[0] as OBJDataVisualization);
                break;

            case DimNotification.EditDim:
                //finishLoad(parameters[0] as OBJDataVisualization);
                break;
        }
    }

    void LoadIfcData()
    {
        IFCDataEntity _IFCDataEntity = new IFCDataEntity(app, this);
        //UnityEngine.Debug.Log(filePath);
        _IFCDataEntity.getIfcModel(filePath);
    }

    void updateTreeView(IEnumerable<IXbimViewModel> ifcItems)
    {
        (view as IFCTreeVisualization).insertIfcData2Tree(ifcItems);
    }

    void objectBindingTreeView(ObjectBinding ObjectBindingList)
    {
        (view as IFCTreeVisualization).assignObjectBinding(ObjectBindingList);
    }
}
