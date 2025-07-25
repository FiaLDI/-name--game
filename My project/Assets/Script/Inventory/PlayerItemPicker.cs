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

        controls = new PlayerInputActions();                // �������� ������ ��� ���������� ������
        controls.Enable();                                  // ��������� �����

        controls.Player.Interact.performed += OnInteract;   // �������� �� �������
    }

    public override void OnStopLocalPlayer()
    {
        if (controls != null)
        {
            controls.Player.Interact.performed -= OnInteract;
            controls.Disable();
            controls.Dispose(); // ����� ��� ���������� �������
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
