using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Xbim.Ifc.ViewModels;
using Battlehub.UIControls;

public class IFCTreeVisualization : DimView, IIFCDataVisualization
{
    protected TreeView treeView;
    protected Text SelectedText;

    // Temporary store selectedItem
    protected IfcModel selectedItem = null;
    protected bool fullyExpand = false;

    public IFCTreeVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        (controller as IFCProvider).setTreeView(this);
        setupView();
    }

    protected virtual void setupView()
    {
        assignAnnotateBtn();
        assignTreeView();
    }

    void assignAnnotateBtn()
    {
        this.app.Notify(controller: controller, message: DimNotification.ShowAnnotateButton, parameters: null);
        GameObject AnnotateButton = GameObject.Find("/UI/Annotate");
        Button annotateLoadBtn = AnnotateButton.GetComponent<Button>();
        annotateLoadBtn.onClick.AddListener(AddDamageData);
        this.app.Notify(controller: controller, message: DimNotification.HideAnnotateButton, parameters: null);
    }

    protected virtual void assignTreeView()
    {
        GameObject EntityText = GameObject.Find("/UI/Entity");
        SelectedText = EntityText.GetComponent<Text>();

        GameObject TreeViewObject = GameObject.Find("/UI/IFC_TreeView");
        treeView = TreeViewObject.GetComponent<TreeView>();

        //subscribe to events
        treeView.ItemDataBinding += OnItemDataBinding;
        treeView.SelectionChanged += OnSelectionChanged;
        treeView.ItemExpanding += OnItemExpanding;
    }

    /// <summary>
    /// This method called for each data item during databinding operation
    /// You have to bind data item properties to ui elements in order to display them.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnItemDataBinding(object sender, TreeViewItemDataBindingArgs e)
    {
        IfcModel dataItem = e.Item as IfcModel;
        if (!dataItem.is_bind)
        {
            dataItem.OnSelectChanged += selectItem;
            dataItem.is_bind = true;
        }

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
            e.HasChildren = dataItem.Children.Length > 0;
        }
    }

    /// <summary>
    /// This method called for each data item selection change
    /// Every selection change will trigger the method.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {
        if (e.NewItems.Length <= 0)
            return;

        var p = treeView.SelectedItem as IfcModel;
        var p2 = selectedItem;

        Debug.LogFormat("Current selected: {0}", p.Name);

        // Comparing if current_selected and prev_selected is similar
        if (p2 == null)
        {
            // If no prev_selected, set prev_select to current_select
            selectedItem = p;
        }
        else if (p.EntityLabel == p2.EntityLabel)
        {
            // If prev_select similar to current_select
            // exit process
            return;
        }
        else
        {
            // If prev_select not equal to current_select
            // set prev_select to current_select
            if (selectedItem.is_Annotatable) selectedItem.Selected = false;
            selectedItem = p;
        }

        if (selectedItem.is_Annotatable) selectedItem.Selected = true;

        // Check if selected is annotable with defect
        if (selectedItem.is_Annotatable)
        {
            this.app.Notify(controller: controller, message: DimNotification.ShowAnnotateButton, parameters: null);
        }
        else
        {
            this.app.Notify(controller: controller, message: DimNotification.HideAnnotateButton, parameters: null);
        }

        // Retrieve property of selected
        getProperty();

        SelectedText.text = selectedItem.Name;
    }

    /// <summary>
    /// This method called for each data item for expanding operation
    /// Insert in children node under entities.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnItemExpanding(object sender, ItemExpandingArgs e)
    {
        //Explore children node
        IfcModel node = e.Item as IfcModel;
        e.Children = node.Children;

        treeView.SelectedItem = node;
    }

    private void selectItem(object sender, System.EventArgs e)
    {
        var selectIfcModel = sender as IfcModel;
        Debug.LogFormat("{0} is selected.", selectIfcModel.Name);

        if (treeView != null)
        {
            treeView.SelectedItem = selectIfcModel;
            expandParent(selectIfcModel);
            selectedItem = selectIfcModel;
        }
    }

    private void expandParent(IfcModel selectIfcModel)
    {
        if (selectIfcModel.Parent != null)
        {
            //Debug.LogFormat("Expand Item {0}", selectIfcModel.Parent.Name);
            var treeViewItem = treeView.GetTreeViewItem(selectIfcModel.Parent as object);
            expandParent(selectIfcModel.Parent);

            if (treeViewItem != null) treeViewItem.IsExpanded = true;
        }
        else
        {
            return;
        }
    }

    void AddDamageData()
    {
        this.app.Notify(controller: controller, message: DimNotification.AddDim, parameters: selectedItem);
    }

    public void insertIfcData2Tree(IfcModel ifcItem)
    {
        // Debug.Log(ifcItem.Name);

        List<IfcModel> ifcItems = new List<IfcModel>();
        ifcItems.Add(ifcItem);

        //Bind data items
        treeView.Items = ifcItems;
        treeView.AutoExpand = true;
    }

    void getProperty()
    {
        this.app.Notify(controller: controller, message: DimNotification.LoadIFCProperty, parameters: selectedItem);
    }
}
