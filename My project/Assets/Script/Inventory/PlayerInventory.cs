using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    // Максимум 20 слотов для примера
    private const int MaxSlots = 8;
    public event System.Action OnInventoryUIChanged;
    public static event System.Action<PlayerInventory> OnLocalPlayerInventoryReady;


    // Синхронизируемый список предметов — каждый слот ItemStack (itemId + quantity)
    [SyncVar] // SyncVar нельзя для коллекций, поэтому делаем так:
    private SyncListItemStack items = new SyncListItemStack();

    // Пользовательский SyncList для ItemStack
    public class SyncListItemStack : SyncList<ItemStack> { }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        OnLocalPlayerInventoryReady?.Invoke(this);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitializeEmptyInventory();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        items.Callback += OnInventoryChanged;

        Debug.Log(items);
    }

    void OnInventoryChanged(SyncListItemStack.Operation op, int index, ItemStack oldItem, ItemStack newItem)
    {
        Debug.Log($"Inventory changed: op={op}, index={index}, newItemId={newItem.itemId}, qty={newItem.quantity}");
        OnInventoryUIChanged?.Invoke();
        // Здесь можно обновить UI
        Debug.Log(items);
    }



    // Пример метода для отображения инвентаря (только локально)
    public void PrintInventory()
    {
        foreach (var stack in items)
        {
            var item = ItemDatabase.Instance.GetItemById(stack.itemId);
            if (item != null)
                Debug.Log($"Item: {item.itemName} x{stack.quantity}");
            else
                Debug.LogWarning($"Unknown item id {stack.itemId}");
        }
    }

    public List<ItemStack> GetItems()
    {
        return new List<ItemStack>(items); // безопасная копия
    }


    [Server]
    public bool AddItem(int itemId, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == itemId)
            {
                var updated = items[i];
                updated.quantity += amount;
                items[i] = updated;
                return true;
            }
        }

        if (items.Count < MaxSlots)
        {
            items.Add(new ItemStack(itemId, amount));
            return true;
        }

        Debug.LogWarning("Inventory full!");
        return false;
    }

    [Server]
    public bool RemoveItem(int itemId, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == itemId)
            {
                var updated = items[i];
                if (updated.quantity >= amount)
                {
                    updated.quantity -= amount;
                    if (updated.quantity <= 0)
                        items.RemoveAt(i);
                    else
                        items[i] = updated;

                    return true;
                }
            }
        }

        return false; // Недостаточно предметов
    }

    [Server]
    private void InitializeEmptyInventory()
    {
        // Очистим список на всякий случай
        items.Clear();

        // Заполним 8 пустыми слотами itemId = 0, quantity = 0
        for (int i = 0; i < MaxSlots; i++)
        {
            items.Add(new ItemStack(0, 0));
        }
    }

    [Command]
    public void CmdAddItem(int itemId, int amount)
    {
        AddItem(itemId, amount);
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

    [ClientRpc]
    void RpcOnUseItem(int itemId)
    {
        Debug.Log($"Client: used item {itemId}");
    }


}
