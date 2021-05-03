using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewContent : MonoBehaviour
{
    public GameObject ListItem;
    public Transform contentBox;

    private DamageModel _DamageInstance;
    private List<GameObject> ListObjects = new List<GameObject>();

    public void clearList()
    {
        ListObjects.Clear();
    }

    public void AddList(DamageModel _DamageInstance)
    {
        this._DamageInstance = _DamageInstance;

        foreach (var property in _DamageInstance.NewProperties)
        {
            GameObject listItem = Instantiate(ListItem, contentBox);
            listItem.GetComponentInChildren<Text>().text = property.ToString();
            ListObjects.Add(listItem);
        }
    }

    private void Remove(GameObject removedObject)
    {
        Destroy(removedObject);
    }

    //public IEnumerable<DamageProperty> ToRemove()
    //{
    //    List<DamageProperty> RemovableItems = new List<DamageProperty>();
    //    foreach (DamageProperty removeItem in targetField.Keys)
    //    {
    //        if (targetField[removeItem].GetComponent<Toggle>().isOn)
    //            RemovableItems.Add(removeItem);
    //    }

    //    foreach (DamageProperty removeItem in RemovableItems)
    //    {
    //        Remove(targetField[removeItem]);
    //        targetField.Remove(removeItem);
    //    }

    //    return RemovableItems;
    //}
}
