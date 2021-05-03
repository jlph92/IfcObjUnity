using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;
using Battlehub.UIControls;
using Xbim.ModelGeometry.Scene;

// Update in new added Object into the Tree


public class Damage_TreeView : IFCTreeView
{
    public void reloadFile(string filename)
    {
        filePath = filename;
        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                //PlacementTree.BuildTree(model);
                Debug.Log("Open the file: " + filePath);
                ViewModel();
            }
        }
    }

    protected override void Awake()
    {
        ifcInteract = new DamageInteract();
    }

    protected override void ViewModel()
    {
        Debug.Log("Damage Tree View start");
        var DamageModel = Model.Instances.OfType<IIfcProxy>()
            //Model.Instances.OfType<IIfcProduct>()
            //.Where(itm => itm is IIfcProxy || itm is IIfcAnnotation || itm is IIfcVoidingFeature)
            .Select(itm => new TypeViewModel(itm.GetType(), Model))
            .Cast<IXbimViewModel>()
            .ToArray();

        //var DamageList = DamageModel.Select(type => new TypeViewModel(type, Model)).Cast<IXbimViewModel>().ToList();
        if (DamageModel != null && DamageModel.Length > 0)
        {
            //dataItems = DamageList;
            ObservableCollection<IXbimViewModel> svList = new ObservableCollection<IXbimViewModel>();
            svList.Add(DamageModel[0]);
            dataItems = svList;

            foreach (var child in DamageModel)
            {
                Debug.Log(child.Name);
                LazyLoadAll(child);
            }
        }
        else
        {
            Debug.Log("None");
        }

        //checkDocument(Model);
    }

    //private void checkDocument(IfcStore Model)
    //{
    //    var RelAssociatesDocument = Model.Instances.OfType<IIfcRelAssociatesDocument>()
    //        .Where(itm => itm.RelatedObjects.Any(obj => obj is IIfcProxy))
    //        .ToArray();

    //    if (RelAssociatesDocument != null)
    //    {
    //        foreach (var child in RelAssociatesDocument)
    //        {
    //            var DocumentRef = child.RelatingDocument as IIfcDocumentReference;
    //            if (DocumentRef.ReferencedDocument.ElectronicFormat.Value.Equals("Stl"))
    //            {
    //                //Debug.Log("Is Stl file");
    //                var pProxy = child.RelatedObjects.FirstOrDefault(p => p is IIfcProxy);
    //                //Vector3 originPoint = getAttachedLocation(Model, pProxy.EntityLabel, showOrigin : false);

    //                GameObject StlGeometryImport = new GameObject();
    //                //StlGeometryImport.transform.localPosition = originPoint;
    //                var StlHandler = StlGeometryImport.AddComponent<StlImport>();

    //                StlHandler.offOrigin();
    //                //STLDocumentData stlDocData = new STLDocumentData();
    //                //stlDocData.readData();
    //                //Debug.Log(stlDocData.getStlColor());
    //                //StlHandler.readSTL(DocumentRef.Location, stlDocData.getLengthUnit(), stlDocData.getStlColor());
    //                //StlGeometryImport.transform.localRotation = stlDocData.getLocalTransformation();

    //            }
    //        }
    //    }
    //}
}
