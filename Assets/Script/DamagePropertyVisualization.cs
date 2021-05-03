using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.UIControls;

public class DamagePropertyVisualization : IfcPropertyVisualization
{
    public DamagePropertyVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    protected override void setupView()
    {
        (controller as IFCProvider).setDamagePropertyView(this);
        assignTreeView();
    }

    public void writeProperties(DamageModel damageItem)
    {
        treeView.Items = damageItem.Properties;
    }

    protected override void assignTreeView()
    {
        GameObject TreeViewObject = GameObject.Find("/UI/Damage_Property");
        treeView = TreeViewObject.GetComponent<VirtualizingTreeView>();

        //subscribe to events
        treeView.ItemDataBinding += OnItemDataBinding;
    }
}
