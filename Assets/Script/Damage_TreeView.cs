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
        var DamageModel = Model.Instances.OfType<IIfcProduct>()
            .Where(itm => itm is IIfcProxy || itm is IIfcAnnotation || itm is IIfcVoidingFeature)
            .Select(itm => itm.GetType())
            .ToArray();

        var DamageList = DamageModel.Select(type => new TypeViewModel(type, Model)).Cast<IXbimViewModel>().ToList();
        if (DamageList != null)
        {
            dataItems = DamageList;

            foreach (var child in DamageList)
                //Debug.Log(child.Entity);
                LazyLoadAll(child);
        }
        else
        {
            Debug.Log("None");
        }
    }

    private void createDamageLocalPoint()
    {
        XbimPlacementTree tree = new XbimPlacementTree(Model);

    }
}
