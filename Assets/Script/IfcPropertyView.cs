using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.UIControls;
using UnityEngine.UI;

public class IfcPropertyView : VirtualizingTreeViewDemo_AddItems
{

    // Start is called before the first frame update
    protected override void Start()
    {
        TreeView.ItemDataBinding += OnItemDataBinding;
        TreeView.SelectionChanged += OnSelectionChanged;
        TreeView.ItemsRemoved += OnItemsRemoved;
        TreeView.ItemExpanding += OnItemExpanding;
    }

    protected override void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
    {
        PropertyItems dataItem = e.Item as PropertyItems;
        if (dataItem != null)
        {
            //We display dataItem.name using UI.Text 
            Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
            text.text = dataItem.GetValue();
        }
    }

    public void writeProperties(IEnumerable<IfcInteract.PropertyItem> _properties)
    {
        TreeView.Items = PropertyItems.register(_properties);
    }
}

public class PropertyItems
{
    private IfcInteract.PropertyItem _property;

    public string GetValue()
    {
        return System.String.Format("{0}: {1}", _property.Name, _property.Value);
    }

    private PropertyItems(IfcInteract.PropertyItem _property)
    {
        this._property = _property;
    }

    public static List<PropertyItems> register(IEnumerable<IfcInteract.PropertyItem> _properties)
    {
        List <PropertyItems> registered_properties = new List<PropertyItems>();

        foreach (var property in _properties)
            registered_properties.Add(new PropertyItems(property));

        return registered_properties;
    }
}
