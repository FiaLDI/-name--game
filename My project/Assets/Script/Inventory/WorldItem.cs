using Mirror;
using UnityEngine;

public class WorldItem : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnItemIdChanged))]
    public int itemId;
    [SyncVar] public int quantity;

    [SerializeField] private Transform visualRoot; // ������ ��� �������

    public void Initialize(int id, int qty)
    {
        itemId = id;
        quantity = qty;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetupVisual();
    }

    private void OnItemIdChanged(int oldId, int newId)
    {
        SetupVisual();
    }

    private void SetupVisual()
    {
        // ������� ������ ������ (���� ����)
        foreach (Transform child in visualRoot)
            Destroy(child.gameObject);

        var item = ItemDatabase.Instance.GetItemById(itemId);
        if (item != null && item.prefab != null)
        {
            // ���������, ���� �� MeshFilter � �������, ����� ������, ��� ��� 3D ������
            if (item.prefab.TryGetComponent<MeshFilter>(out _))
            {
                var meshInstance = Instantiate(item.prefab, visualRoot);
                meshInstance.transform.localPosition = Vector3.zero;
                meshInstance.transform.localRotation = Quaternion.identity;
                meshInstance.transform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogWarning($"WorldItem: ������ �������� {item.itemName} �� �������� MeshFilter.");
            }
        }
        else
        {
            Debug.LogWarning($"WorldItem: ��� ������� ��� itemId = {itemId}");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (!other.CompareTag("Player")) return;

        var inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        // ���������� �����, ������� ���������� bool
        bool added = inventory.ServerAddItem(itemId, quantity);

        if (added)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    public void Pickup(PlayerInventory player)
    {
        if (!isServer) return;

        // ���������� �����, ������� ���������� bool
        if (player.ServerAddItem(itemId, quantity))
        {
            NetworkServer.Destroy(gameObject);
        }
    }


}
