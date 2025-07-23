using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [HideInInspector] public int id; // скрываем в инспекторе, чтобы не меняли вручную
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public bool isStackable;
}
