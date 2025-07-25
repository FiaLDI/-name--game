using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    private const int MaxSlots = 8;
    public event System.Action OnInventoryUIChanged;
    public static event System.Action<PlayerInventory> OnLocalPlayerInventoryReady;
    public static PlayerInventory localPlayerInventory { get; private set; }

    // Главный синхронизированный список
    private readonly SyncList<ItemStack> syncItems = new SyncList<ItemStack>();

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"Local player started: {netId}");
        localPlayerInventory = this;
        OnLocalPlayerInventoryReady?.Invoke(this);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"Server initializing inventory for {netId}");

        // Инициализация пустого инвентаря
        syncItems.Clear();
        for (int i = 0; i < MaxSlots; i++)
        {
            syncItems.Add(new ItemStack(0, 0));
        }

        // Добавляем тестовые предметы
        ServerAddItem(1, 3);
        ServerAddItem(2, 1);
    }

    [Client]
    public void AddItem(int itemId, int amount)
    {
        if (isLocalPlayer)
        {
            CmdAddItem(itemId, amount);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"Client started for {netId}, isLocal: {isLocalPlayer}");

        syncItems.Callback += OnSyncItemsChanged;

        if (isLocalPlayer)
        {
            StartCoroutine(RequestInitialSync());
        }
    }

    private IEnumerator RequestInitialSync()
    {
        while (!NetworkClient.ready)
            yield return null;

        yield return new WaitForSeconds(0.2f);

        if (isLocalPlayer)
        {
            //CmdRequestFullInventory();
        }
    }


    private void OnSyncItemsChanged(SyncList<ItemStack>.Operation op, int index, ItemStack oldItem, ItemStack newItem)
    {
        Debug.Log($"Inventory sync changed: {op} at {index}, new: {newItem.itemId}x{newItem.quantity}");
        OnInventoryUIChanged?.Invoke();
    }

    [Server]
    public bool ServerAddItem(int itemId, int amount)
    {
        var itemData = ItemDatabase.Instance.GetItemById(itemId);
        if (itemData == null) return false;

        // Попытка добавить в существующий стек
        for (int i = 0; i < syncItems.Count; i++)
        {
            if (syncItems[i].itemId == itemId && itemData.isStackable)
            {
                var stack = syncItems[i];
                stack.quantity += amount;
                syncItems[i] = stack;
                return true;
            }
        }

        // Поиск пустого слота
        for (int i = 0; i < syncItems.Count; i++)
        {
            if (syncItems[i].itemId == 0)
            {
                syncItems[i] = new ItemStack(itemId, amount);
                return true;
            }
        }

        return false;
    }

    public List<ItemStack> GetItems()
    {
        return new List<ItemStack>(syncItems);
    }

    public void PrintInventory()
    {
        foreach (var stack in syncItems)
        {
            var item = ItemDatabase.Instance.GetItemById(stack.itemId);
            Debug.Log(item != null
                ? $"Item: {item.itemName} x{stack.quantity}"
                : $"Empty slot");
        }
    }

    [Command]
    public void CmdAddItem(int itemId, int amount)
    {
        ServerAddItem(itemId, amount);
    }

    [Server]
    public bool RemoveItem(int itemId, int amount)
    {
        for (int i = 0; i < syncItems.Count; i++)
        {
            if (syncItems[i].itemId == itemId)
            {
                if (syncItems[i].quantity >= amount)
                {
                    if (syncItems[i].quantity == amount)
                    {
                        syncItems[i] = new ItemStack(0, 0);
                    }
                    else
                    {
                        var stack = syncItems[i];
                        stack.quantity -= amount;
                        syncItems[i] = stack;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    [Command]
    public void CmdUseItem(int itemId)
    {
        bool success = RemoveItem(itemId, 1);
        if (success)
        {
            RpcOnUseItem(itemId);
        }
    }

    [Command]
    public void CmdDropItem(int itemId, int quantity)
    {
        if (RemoveItem(itemId, quantity))
        {
            var item = ItemDatabase.Instance.GetItemById(itemId);
            if (item != null && item.prefab != null)
            {
                Vector3 pos = transform.position + transform.forward;
                GameObject go = Instantiate(item.prefab, pos, Quaternion.identity);
                var worldItem = go.GetComponent<WorldItem>();
                worldItem.Initialize(itemId, quantity);
                NetworkServer.Spawn(go);
            }
        }
    }

    [Command]
    public void CmdPickupItem(uint worldItemNetId)
    {
        Debug.Log($"Сервер: попытка подбора предмета {worldItemNetId}");

        if (!NetworkServer.spawned.TryGetValue(worldItemNetId, out NetworkIdentity identity)) return;

        WorldItem worldItem = identity.GetComponent<WorldItem>();
        if (worldItem == null) return;

        if (ServerAddItem(worldItem.itemId, worldItem.quantity))
        {
            NetworkServer.Destroy(identity.gameObject);
        }
    }

    [ClientRpc]
    void RpcOnUseItem(int itemId)
    {
        Debug.Log($"Client: used item {itemId}");
    }
}