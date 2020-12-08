using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc.ViewModels;
using Xbim.Ifc4.Interfaces;
using Battlehub.UIControls;

public class IFCTreeView : MonoBehaviour
{
    public TreeView TreeView;
    private string filePath;
    IEnumerable<IXbimViewModel> dataItems;
    private IfcInteract ifcInteract= new IfcInteract();
    private ObjectBinding ObjectBindingProperty = new ObjectBinding();

    private void Start()
    {
        if (!TreeView)
        {
            Debug.LogError("Set TreeView field");
            return;
        }
    }

    public void openFile(string filename)
    {
        filePath = filename;
        if (filePath.Length != 0)
        {
            using (var model = IfcStore.Open(filePath))
            {
                ObjectBindingProperty.setModel(model);
                setup(model);
            }
        }
    }

    private void setup(IfcStore Model)
    {
        this.Model = Model;

        ViewModel();

        //subscribe to events
        TreeView.ItemDataBinding += OnItemDataBinding;
        TreeView.SelectionChanged += OnSelectionChanged;
        TreeView.ItemExpanding += OnItemExpanding;

        //Bind data items
        TreeView.Items = dataItems;
    }

    private void OnItemExpanding(object sender, ItemExpandingArgs e)
    {
        //get parent data item (game object in our case)
        IXbimViewModel node = (IXbimViewModel)e.Item;
        e.Children = node.Children.Cast<IXbimViewModel>().ToArray();
    }

    private void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {
        // get list box item and tranlate to entity
        ifcInteract.Clear();

        if (e.NewItems.Length <= 0)
            return;
        var p = e.NewItems[0] as IXbimViewModel;
        var p2 = TreeView.SelectedItem as IXbimViewModel;
        if (p2 == null)
            TreeView.SelectedItem = p;
        else if (!(Equals(p.Model, p2.Model) && p.EntityLabel == p2.EntityLabel))
            TreeView.SelectedItem = p;

        var go = ObjectBindingProperty.GetValue(TreeView.SelectedItem as IXbimViewModel);
        if ( go != null)
        {
            go.GetComponent<MouseHighlight>().Select();
        }

        using (var model = IfcStore.Open(filePath))
        {
            var id = (TreeView.SelectedItem as IXbimViewModel).EntityLabel;
            IIfcObjectDefinition selected = model.Instances.FirstOrDefault<IIfcObjectDefinition>(d => d.EntityLabel == id);
            ifcInteract.FillPropertyData(selected);
            var _properties = ifcInteract.Properties;
            /*foreach (var _property in _properties)
            {
                Debug.Log(String.Format("{0}: {1}", _property.Name, _property.Value));
            }*/
        }
    }


    /// <summary>
    /// This method called for each data item during databinding operation
    /// You have to bind data item properties to ui elements in order to display them.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnItemDataBinding(object sender, TreeViewItemDataBindingArgs e)
    {
        IXbimViewModel dataItem = e.Item as IXbimViewModel;
        if (dataItem != null)
        {
            //We display dataItem.name using UI.Text 
            Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
            text.text = dataItem.Name;

            //Load icon from resources
            //Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
            //icon.sprite = Resources.Load<Sprite>("cube");

            //Debug.Log(dataItem.Name + ": " + dataItem.Children.Count());

            //And specify whether data item has children (to display expander arrow if needed)
            e.HasChildren = dataItem.Children.Count() > 0;
        }
    }

    //private IXbimViewModel FindUnderContainingSpace(TreeViewItemDataBindingArgs newVal, IIfcProduct p)
    //{
    //    var containingSpace = p.IsContainedIn;
    //    if (containingSpace != null)
    //    {
    //        var containingSpaceView = FindItemBreadthFirst(containingSpace);
    //        if (containingSpaceView != null)
    //        {
    //            var found = FindItemDepthFirst(containingSpaceView, newVal);
    //            if (found != null)
    //            {
    //                return found;
    //            }
    //        }
    //    }
    //    return null;
    //}

    //private IXbimViewModel FindItemDepthFirst(IXbimViewModel node, TreeViewItemDataBindingArgs entity)
    //{
    //    if (IsMatch(node, entity.Item))
    //    {
    //        // node.IsExpanded = true; // commented because of new Highlighting mechanisms
    //        return node;
    //    }

    //    foreach (var child in node.Children)
    //    {
    //        IXbimViewModel res = FindItemDepthFirst(child, entity);
    //        if (res != null)
    //        {
    //            // node.IsExpanded = true; //commented because of new Highlighting mechanisms
    //            return res;
    //        }
    //    }
    //    return null;
    //}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TreeView.SelectedItems = TreeView.Items.OfType<object>().Take(5).ToArray();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            TreeView.SelectedItem = null;
        }
    }

  

    public IfcStore Model
    {
        get { return ModelProperty; }
        set { ModelProperty = value; }
    }

    private IfcStore ModelProperty = null;


    private void ViewModel()
    {
        var project = Model.Instances.OfType<IIfcProject>().FirstOrDefault();
        if (project != null)
        {
            ObservableCollection<XbimModelViewModel> svList = new ObservableCollection<XbimModelViewModel>();
            svList.Add(new XbimModelViewModel(project, null));
            dataItems = svList;

            foreach (var child in svList)
                LazyLoadAll(child);
        }
    }

    private void LazyLoadAll(IXbimViewModel parent)
    {
        foreach (var child in parent.Children)
        {
            ObjectBindingProperty.Register(child as IXbimViewModel);
            LazyLoadAll(child);
        }
    }
}

