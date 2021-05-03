using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.UIControls;
using UnityEngine.UI;

public class IfcPropertyVisualization : DimView
{
    protected VirtualizingTreeView treeView;

    public IfcPropertyVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        setupView();
    }

    protected virtual void setupView()
    {
        (controller as IFCProvider).setPropertyView(this);
        assignTreeView();
    }

    public void writeProperties(IfcModel ifcItem)
    {
        treeView.Items = ifcItem.Properties;
    }

    protected virtual void assignTreeView()
    {
        GameObject TreeViewObject = GameObject.Find("/UI/IFC_Property");
        treeView = TreeViewObject.GetComponent<VirtualizingTreeView>();

        //subscribe to events
        treeView.ItemDataBinding += OnItemDataBinding;
    }

    protected void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
    {
        PropertyItem dataItem = e.Item as PropertyItem;
        if (dataItem != null)
        {
            //We display dataItem.name using UI.Text 
            Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
            text.text = dataItem.ToString();
        }
    }
}
