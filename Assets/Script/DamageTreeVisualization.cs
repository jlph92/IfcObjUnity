using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.UIControls;

public class DamageTreeVisualization : DimView, IIFCDataVisualization
{
    protected TreeView treeView;

    // Temporary store selectedItem
    protected DamageModel selectedItem = null;

    public DamageTreeVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        (controller as IFCProvider).setDamageView(this);
        setupView();
    }

    protected virtual void setupView()
    {
        assignEditBtn();
        assignTreeView();
    }

    void assignEditBtn()
    {
        this.app.Notify(controller: controller, message: DimNotification.ShowEditButton, parameters: null);
        GameObject AnnotateButton = GameObject.Find("/UI/Edit");
        Button annotateLoadBtn = AnnotateButton.GetComponent<Button>();
        annotateLoadBtn.onClick.AddListener(EditDamageData);
        this.app.Notify(controller: controller, message: DimNotification.HideEditButton, parameters: null);
    }

    protected virtual void assignTreeView()
    {
        GameObject TreeViewObject = GameObject.Find("/UI/Damage_TreeView");
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
        DamageModel dataItem = e.Item as DamageModel;

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
        // get list box item and tranlate to entity

        if (e.NewItems.Length <= 0)
            return;

        var p = treeView.SelectedItem as DamageModel;
        var p2 = selectedItem;


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
            selectedItem = p;
        }

        // Check if selected is annotable with defect
        if (selectedItem.is_Editable)
        {
            this.app.Notify(controller: controller, message: DimNotification.ShowEditButton, parameters: null);
        }
        else
        {
            this.app.Notify(controller: controller, message: DimNotification.HideEditButton, parameters: null);
        }

        // Retrieve property of selected
        getProperty();
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
        DamageModel node = e.Item as DamageModel;
        e.Children = node.Children;
    }

    public void insertIfcData2Tree(DamageViewModel damageViewModel)
    {
        //Bind data items
        treeView.Items = damageViewModel.DamageModels;
    }

    void EditDamageData()
    {
        this.app.Notify(controller: controller, message: DimNotification.EditDim, parameters: selectedItem);
    }

    void getProperty()
    {
        this.app.Notify(controller: controller, message: DimNotification.LoadDamageProperty, parameters: selectedItem);
    }
}
