using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject slotPrefab;

    private PlayerInventory playerInventory;

    private void OnEnable()
    {
        PlayerInventory.OnLocalPlayerInventoryReady += SetupInventory;

        if (PlayerInventory.localPlayerInventory != null)
            SetupInventory(PlayerInventory.localPlayerInventory);
    }

    private void OnDisable()
    {
        PlayerInventory.OnLocalPlayerInventoryReady -= SetupInventory;
    }

    private void SetupInventory(PlayerInventory inventory)
    {
        if (!inventory.isLocalPlayer) return;

        if (playerInventory != null)
            playerInventory.OnInventoryUIChanged -= UpdateUI;

        playerInventory = inventory;
        playerInventory.OnInventoryUIChanged += UpdateUI;

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerInventory == null || contentPanel == null || slotPrefab == null)
        {
            Debug.LogError("UI Update skipped: missing references!");
            return;
        }

        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        var items = playerInventory.GetItems();
        Debug.Log($"Updating UI with {items.Count} items");

        for (int i = 0; i < items.Count; i++)
        {
            var slot = Instantiate(slotPrefab, contentPanel);
            var slotUI = slot.GetComponent<InventorySlotUI>();
            var stack = items[i];

            if (stack.itemId != 0)
            {
                var item = ItemDatabase.Instance.GetItemById(stack.itemId);
                if (item != null)
                {
                    slotUI.Setup(item, stack.quantity);
                    continue;
                }
            }
            slotUI.SetupEmpty();
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryUIChanged -= UpdateUI;
    }
}
