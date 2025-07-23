using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [HideInInspector] public int id; // �������� � ����������, ����� �� ������ �������
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public bool isStackable;
}
