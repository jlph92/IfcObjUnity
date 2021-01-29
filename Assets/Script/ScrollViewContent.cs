using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewContent : MonoBehaviour
{
    public GameObject damagePropertyList;
    public Transform contentBox;

    private Dictionary<DamageProperty, GameObject> targetField = new Dictionary<DamageProperty, GameObject>();

    public void clearList()
    {
        targetField.Clear();
    }

    public void Add(DamageProperty dmgProp, string contentText)
    {
        if (targetField.ContainsKey(dmgProp)) return;

        GameObject listItem = Instantiate(damagePropertyList, contentBox);
        listItem.GetComponentInChildren<Text>().text = contentText;
        
        targetField.Add(dmgProp, listItem);
    }

    private void Remove(GameObject removedObject)
    {
        Destroy(removedObject);
    }

    public IEnumerable<DamageProperty> ToRemove()
    {
        List<DamageProperty> RemovableItems = new List<DamageProperty>();
        foreach (DamageProperty removeItem in targetField.Keys)
        {
            if (targetField[removeItem].GetComponent<Toggle>().isOn)
                RemovableItems.Add(removeItem);
        }

        foreach (DamageProperty removeItem in RemovableItems)
        {
            Remove(targetField[removeItem]);
            targetField.Remove(removeItem);
        }

        return RemovableItems;
    }
}
