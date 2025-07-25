using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerItemPicker : NetworkBehaviour
{
    private WorldItem itemInRange;
    private PlayerInventory inventory;
    private PlayerInputActions controls;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        inventory = GetComponent<PlayerInventory>();

        controls = new PlayerInputActions();                // Создание только для локального игрока
        controls.Enable();                                  // Активация схемы

        controls.Player.Interact.performed += OnInteract;   // Подписка на нажатие
    }

    public override void OnStopLocalPlayer()
    {
        if (controls != null)
        {
            controls.Player.Interact.performed -= OnInteract;
            controls.Disable();
            controls.Dispose(); // важно для корректной очистки
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (itemInRange != null && inventory != null)
        {
            inventory.CmdPickupItem(itemInRange.netId);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.TryGetComponent<WorldItem>(out var worldItem))
        {
            itemInRange = worldItem;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.TryGetComponent<WorldItem>(out var worldItem) && worldItem == itemInRange)
        {
            itemInRange = null;
        }
    }
}
