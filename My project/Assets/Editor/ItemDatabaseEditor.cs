#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    private string filter = "";
    private bool sortById = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var db = (ItemDatabase)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚙️ Item Database Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("➕ Add New Item"))
        {
            db.AddNewItem();
        }
        if (GUILayout.Button("🔄 Auto-Assign IDs"))
        {
            db.AutoAssignIds();
        }
        if (GUILayout.Button("📀 Spawn All Prefabs"))
        {
            db.SpawnAllItems();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        filter = EditorGUILayout.TextField("Filter by name:", filter);

        sortById = EditorGUILayout.Toggle("Sort by ID (else name)", sortById);

        var items = db.Items.ToList()
            .Where(i => i != null && (i.itemName?.ToLower().Contains(filter.ToLower()) ?? false));



        items = sortById
            ? items.OrderBy(i => i.id)
            : items.OrderBy(i => i.itemName);

        foreach (var item in items)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{item.id:000}] {item.itemName}", GUILayout.MaxWidth(200));
            if (GUILayout.Button("Spawn Prefab"))
            {
                if (item.prefab != null)
                {
                    var inst = PrefabUtility.InstantiatePrefab(item.prefab) as GameObject;
                    inst.name = $"Item_{item.id}_{item.itemName}";
                }
            }
            if (GUILayout.Button("🗑 Remove", GUILayout.MaxWidth(60)))
            {
                if (EditorUtility.DisplayDialog("Confirm remove",
                    $"Delete '{item.itemName}' and its asset?", "Yes", "No"))
                {
                    db.RemoveItem(item);
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
