﻿using System.Collections;
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
    private DamageInteract DamageInteract = new DamageInteract();
    protected override void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {
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
            selectedItem = p;
        }
        ObjectBindingProperty.damageSelect(TreeView.SelectedItem as IXbimViewModel);

        var selected = TreeView.SelectedItem as IXbimViewModel;
        DamageInteract.FillTypeData(selected.Entity);
        ifcPropertyView.writeProperties(DamageInteract.typeProperties);
        //using (var model = IfcStore.Open(filePath))
        //{
        //    var id = (TreeView.SelectedItem as IXbimViewModel).EntityLabel;
        //    IIfcObjectDefinition selected = model.Instances.FirstOrDefault<IIfcObjectDefinition>(d => d.EntityLabel == id);
        //    DamageInteract.FillTypeData(model, selected);
        //    ifcPropertyView.writeProperties(DamageInteract.typeProperties);
        //}
    }


    protected override void ViewModel()
    {
        Debug.Log("Damage Tree View start");
        var DamageModel = Model.Instances.OfType<IIfcProduct>()
            //.Where (itm => typeof(IIfcProxy).IsInstanceOfType(itm) || typeof(IIfcAnnotation).IsInstanceOfType(itm) || typeof(IIfcVoidingFeature).IsInstanceOfType(itm))
            .Where(itm => itm is IIfcProxy || itm is IIfcAnnotation || itm is IIfcVoidingFeature)
            .Select(itm => itm.GetType());

        var DamageList = DamageModel.Select(type => new TypeViewModel(type, Model)).Cast<IXbimViewModel>().ToList();
        if (DamageList != null)
        {
            dataItems = DamageList;

            foreach (var child in DamageList)
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
