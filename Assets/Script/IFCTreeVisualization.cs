using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xbim.Ifc.ViewModels;
using Battlehub.UIControls;

public class IFCTreeVisualization : DimView, IIFCDataVisualization
{
    private GameObject AnnotateButton;
    private ObjectBinding ObjectBindingList;

    protected TreeView treeView;

    // Temporary store selectedItem
    protected IXbimViewModel selectedItem = null;

    public IFCTreeVisualization(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        (controller as IFCProvider).setView(this);
        setupView();
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void setupView()
    {
        assignAnnotateBtn();
        assignTreeView();
        LoadIfcItem();
    }

    public void assignObjectBinding(ObjectBinding ObjectBindingList)
    {
        this.ObjectBindingList = ObjectBindingList;
    }

    void assignAnnotateBtn()
    {
        AnnotateButton = GameObject.Find("/UI/Annotate");
        AnnotateButton.SetActive(false);
        Button annotateLoadBtn = AnnotateButton.GetComponent<Button>();
        annotateLoadBtn.onClick.AddListener(AddDamageData);
    }

    protected virtual void assignTreeView()
    {
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

    /// <summary>
    /// This method called for each data item selection change
    /// Every selection change will trigger the method.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {
        // deActivate Annotate Button
        if (AnnotateButton != null) AnnotateButton.SetActive(false);

        // get list box item and tranlate to entity

        if (e.NewItems.Length <= 0)
            return;

        var p = treeView.SelectedItem as IXbimViewModel;
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
            //ObjectBindingProperty.unselect(p2 as IXbimViewModel);
            selectedItem = p;
        }

        if (ObjectBindingList != null)
        {
            if (ObjectBindingList.select(treeView.SelectedItem as IXbimViewModel))
            {
                //ProductLabel = (TreeView.SelectedItem as IXbimViewModel).EntityLabel;
                AnnotateButton.SetActive(true);
            }
        }

        var selected = treeView.SelectedItem as IXbimViewModel;
        //var prop = ifcInteract.getProperties(selected.Entity);
        //ifcPropertyView.writeProperties(prop);

        //if (TreeView.SelectedItem is null) SelectedText.text = "null";
        //else SelectedText.text = (TreeView.SelectedItem as IXbimViewModel).Name;
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
        IXbimViewModel node = e.Item as IXbimViewModel;
        e.Children = node.Children.Cast<IXbimViewModel>().ToArray();
    }

    void LoadIfcItem()
    {
        this.app.Notify(controller: controller, message: DimNotification.LoadIFCData);
    }

    void AddDamageData()
    {
        this.app.Notify(controller: controller, message: DimNotification.AddDim, parameters: selectedItem.EntityLabel);
    }

    public void insertIfcData2Tree(IEnumerable<IXbimViewModel> ifcItems)
    {
        //Bind data items
        treeView.Items = ifcItems;
    }

    void getProperty(IXbimViewModel selected)
    {
        this.app.Notify(controller: controller, message: DimNotification.LoadIFCProperty, parameters: selected.Entity);
    }
}
