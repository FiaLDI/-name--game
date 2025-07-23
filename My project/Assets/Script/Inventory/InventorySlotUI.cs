using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text quantityText;
    public Sprite emptySlotSprite;

    public void Setup(Item item, int quantity)
    {
        if (icon != null)
            icon.sprite = item.icon;

        if (quantityText != null)
            quantityText.text = quantity > 1 ? $"x{quantity}" : "";
    }

    public void SetupEmpty()
    {
        if (icon != null && emptySlotSprite != null)
            icon.sprite = emptySlotSprite;

        if (quantityText != null)
            quantityText.text = "";
    }
}
