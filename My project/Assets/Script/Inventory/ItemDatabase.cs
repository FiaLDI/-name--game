// ItemDatabase.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> items = new List<Item>();
    private Dictionary<int, Item> itemLookup;

    public IEnumerable<Item> Items => items;
    public static ItemDatabase Instance
    {
        get; private set;
    }

    private void OnEnable()
    {
        Instance = this;
        Init();
    }

    public void Init()
    {
        if (itemLookup != null) return;
        itemLookup = items
            .Where(i => i != null)
            .ToDictionary(i => i.id, i => i);
    }

    public Item GetItemById(int id)
    {
        Init();
        return itemLookup.TryGetValue(id, out var item) ? item : null;
    }

#if UNITY_EDITOR
    public void AutoAssignIds()
    {
        int nextId = 1;
        var used = new HashSet<int>();

        foreach (var item in items)
        {
            if (item == null) continue;
            if (item.id == 0 || used.Contains(item.id))
                item.id = nextId++;
            else
                nextId = Mathf.Max(nextId, item.id + 1);

            used.Add(item.id);
            EditorUtility.SetDirty(item);
        }
        EditorUtility.SetDirty(this);
        Debug.Log("ItemDatabase: IDs auto-assigned.");
    }

    public void AddNewItem()
    {
        var newItem = CreateInstance<Item>();

        AssetDatabase.AddObjectToAsset(newItem, this);
        items.Add(newItem);

        EditorUtility.SetDirty(newItem);
        EditorUtility.SetDirty(this);

        AutoAssignIds();
        AssetDatabase.SaveAssets();
    }

    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            // Удаляем вложенный ассет из базы
            DestroyImmediate(item, true);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }

    public void SpawnAllItems()
    {
        foreach (var item in items)
        {
            if (item != null && item.prefab != null)
            {
                var inst = PrefabUtility.InstantiatePrefab(item.prefab) as GameObject;
                inst.name = $"Item_{item.id}_{item.itemName}";
            }
        }
    }

#endif
}
