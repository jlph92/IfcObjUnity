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

public class Damage_TreeView : IFCTreeView
{
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
        if (DamageModel != null)
        {
            //dataItems = DamageList;
            ObservableCollection<IXbimViewModel> svList = new ObservableCollection<IXbimViewModel>();
            svList.Add(DamageModel[0]);
            dataItems = svList;

            foreach (var child in DamageModel)
            {
                //Debug.Log(child.Name);
                LazyLoadAll(child);
            }
               
        }
        else
        {
            Debug.Log("None");
        }

        checkDocument(Model);
    }

    private void checkDocument(IfcStore Model)
    {
        var RelAssociatesDocument = Model.Instances.OfType<IIfcRelAssociatesDocument>()
            .Where(itm => itm.RelatedObjects.Any(obj => obj is IIfcProxy))
            .ToArray();

        if (RelAssociatesDocument != null)
        {
            foreach (var child in RelAssociatesDocument)
            {
                var DocumentRef = child.RelatingDocument as IIfcDocumentReference;
                if (DocumentRef.ReferencedDocument.ElectronicFormat.Value.Equals("Stl"))
                {
                    //Debug.Log("Is Stl file");
                    var pProxy = child.RelatedObjects.FirstOrDefault(p => p is IIfcProxy);
                    Vector3 originPoint = getAttachedLocation(Model, pProxy.EntityLabel, showOrigin : false);

                    GameObject StlGeometryImport = new GameObject();
                    StlGeometryImport.transform.localPosition = originPoint;
                    var StlHandler = StlGeometryImport.AddComponent<StlImport>();

                    StlHandler.offOrigin();
                    StlHandler.openSTL(DocumentRef.Location, Parabox.Stl.Unit.Milimeter);

                }
            }
        }
    }
}
