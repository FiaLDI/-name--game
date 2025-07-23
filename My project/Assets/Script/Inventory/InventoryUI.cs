using Mirror;
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
    }

    private void OnDisable()
    {
        PlayerInventory.OnLocalPlayerInventoryReady -= SetupInventory;
    }

    private void SetupInventory(PlayerInventory inventory)
    {
        playerInventory = inventory;
        playerInventory.OnInventoryUIChanged += UpdateUI;
        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        foreach (var stack in playerInventory.GetItems())
        {
            Item item = null;
            if (stack.itemId != 0)
                item = ItemDatabase.Instance.GetItemById(stack.itemId);

            var go = Instantiate(slotPrefab, contentPanel);
            var ui = go.GetComponent<InventorySlotUI>();

            if (item != null)
                ui.Setup(item, stack.quantity);
            else
                ui.SetupEmpty();
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryUIChanged -= UpdateUI;
    }
}
