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
    public IfcPropertyView ifcPropertyView;
    protected string filePath;
    protected IEnumerable<IXbimViewModel> dataItems;
    private IfcInteract ifcInteract= new IfcInteract();
    protected ObjectBinding ObjectBindingProperty = new ObjectBinding();

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
        IXbimViewModel node = e.Item as IXbimViewModel;
        e.Children = node.Children.Cast<IXbimViewModel>().ToArray();
    }

    protected IXbimViewModel selectedItem = null;

    protected virtual void OnSelectionChanged(object sender, SelectionChangedArgs e)
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
            ObjectBindingProperty.unselect(p2 as IXbimViewModel);
            selectedItem = p;
        }

        ObjectBindingProperty.select(TreeView.SelectedItem as IXbimViewModel);

        var selected = TreeView.SelectedItem as IXbimViewModel;
        var prop = ifcInteract.getProperties(selected.Entity);
        ifcPropertyView.writeProperties(prop);
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

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.J))
    //    {
    //        TreeView.SelectedItems = TreeView.Items.OfType<object>().Take(5).ToArray();
    //    }
    //    else if (Input.GetKeyDown(KeyCode.K))
    //    {
    //        TreeView.SelectedItem = null;
    //    }
    //}

  

    public IfcStore Model
    {
        get { return ModelProperty; }
        set { ModelProperty = value; }
    }

    private IfcStore ModelProperty = null;


    protected virtual void ViewModel()
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

    protected void LazyLoadAll(IXbimViewModel parent)
    {
        ifcInteract.FillPropertyData(parent.Entity);

        foreach (var child in parent.Children)
        {
            ObjectBindingProperty.Register(child);
            LazyLoadAll(child);
        }
    }
}

