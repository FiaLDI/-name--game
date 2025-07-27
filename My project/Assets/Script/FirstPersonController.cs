using Mirror;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : NetworkBehaviour
{
    public static FirstPersonController Instance;
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Look")]
    public Transform cameraTransform;
    public float lookSensitivity = 1.5f;

    //[Header("Animation")]
    //public Animator modelAnimator; // Animator на дочернем объекте Model

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;


    private float pitch = 0f;

    private float verticalVelocity = 0f;
    public float jumpGravity = -15.54f;
    public float fallGravity = -9.81f;
    public GameObject pauseMenuUI;
    public PlayerInventory inventory;

    private bool canMove = true;
    private bool isPaused = false;

    public static event Action<Transform, FirstPersonController> OnLocalPlayerReady;
    public PlayerInputActions InputActions => inputActions;


    // Для синхронизации позиции и поворота
    [SyncVar] private Vector3 syncPosition;
    [SyncVar] private Quaternion syncRotation;
    [SyncVar] private float syncPitch;

    
    public void EnableInput()
    {
        inputActions.Enable();
    }

    public void DisableInput()
    {
        inputActions.Disable();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        inputActions = new PlayerInputActions();
        inputActions.Enable();

        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Pause.performed += OnPausePerformed;
        Instance = this;

        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogError("PlayerInventory component not found!");
                return;
            }
        }

        if (cameraTransform != null)
        {
            cameraTransform.gameObject.SetActive(true);
            Debug.Log("Camera enabled for local player");
        }
        else
        {
            Debug.LogWarning("cameraTransform is null!");
        }

        InitializeObjectives();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnLocalPlayerReady?.Invoke(transform, this);
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();

        if (inputActions != null)
        {
            inputActions.Player.Jump.performed -= OnJumpPerformed;
            inputActions.Player.Pause.performed -= OnPausePerformed;
            inputActions.Disable();
        }

        if (cameraTransform != null && !isServer)
        {
            cameraTransform.gameObject.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform != null)
        {
            cameraTransform.gameObject.SetActive(false);
        }

    }
        private void Update()
    {
        // Если это НЕ локальный игрок, применяем синхронизированную позицию
        if (!isLocalPlayer)
        {
            if (syncRotation != Quaternion.identity && syncRotation.w != 0)
            {
                transform.position = Vector3.Lerp(transform.position, syncPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, syncRotation, Time.deltaTime * 10f);
            }
            return;
        }

        // Дальше только локальный игрок (и хост тоже!)
        if (!canMove) return;

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleLook();
        HandleMovement();

        // Отправляем позицию на сервер (если мы клиент)
        if (!isServer) // Хост не должен отправлять сам себе
        {
            CmdSendTransform(transform.position, transform.rotation);
        }
        else // Если это хост, обновляем синхронные переменные напрямую
        {
            syncPosition = transform.position;
            syncRotation = transform.rotation;
            RpcUpdateTransform(transform.position, transform.rotation); // Рассылаем клиентам
        }
    }
    void OnDisable()
    {
        if (inputActions != null)
        { 
            inputActions.Player.Jump.performed -= OnJumpPerformed;
            inputActions.Disable();
        }    
            
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }

    private void InitializeObjectives()
    {
        var objectives = FindObjectsByType<ObjectiveRuntime>(FindObjectsSortMode.None);

        foreach (var objective in objectives)
        {
            objective.Initialize(transform, inputActions);

            if (GlobalObjectiveListUIController.Instance != null)
            {
                GlobalObjectiveListUIController.Instance.RegisterObjective(objective);
            }
        }
    }

    void HandleLook()
    {
        if (!isLocalPlayer) return; // Только локальный игрок управляет камерой

        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

        pitch -= lookInput.y * lookSensitivity * 0.1f;
        pitch = Mathf.Clamp(pitch, -25f, 25f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Отправляем угол на сервер (если мы клиент)
        if (!isServer)
        {
            CmdSendPitch(pitch);
        }
        else // Если хост, обновляем сразу
        {
            syncPitch = pitch;
            RpcUpdatePitch(pitch);
        }
    }

    void HandleMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f; // удерживаем прижатие к земле
        }
        else
        {
            // Более сильная гравитация при падении
            float gravityToApply = verticalVelocity > 0 ? jumpGravity : fallGravity;
            verticalVelocity += gravityToApply * Time.deltaTime;
        }

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * moveInput.y + right * moveInput.x;
        move = move.normalized * moveSpeed;

        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0)
        {
            verticalVelocity = 0f;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isLocalPlayer && controller.isGrounded)
        {
            CmdRequestJump();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    [Command]
    void CmdSendPitch(float newPitch)
    {
        syncPitch = newPitch; // Сервер обновляет угол
        RpcUpdatePitch(newPitch); // Рассылаем всем клиентам
    }


    [Command]
    void CmdSendTransform(Vector3 pos, Quaternion rot)
    {
        syncPosition = pos;
        syncRotation = rot;
        RpcUpdateTransform(pos, rot);
    }

    [Command]
    void CmdRequestJump()
    {
        RpcDoJump(); // Триггерим прыжок на всех клиентах
    }

    [Command]
    public void CmdAddItem(int itemId, int amount)
    {
        if (inventory != null)
        {
            //bool success = inventory.AddItem(itemId, amount);
            // Можно уведомить клиента о результате, через TargetRpc, если нужно
        }
    }

    // Использовать предмет
    [Command]
    public void CmdUseItem(int itemId)
    {
        if (inventory != null)
        {
            bool success = inventory.RemoveItem(itemId, 1);
            if (success)
            {
                // Логика использования предмета, например, восстановление здоровья
                RpcOnUseItem(itemId);
            }
        }
    }

    [ClientRpc]
    void RpcOnUseItem(int itemId)
    {
        // Клиентская логика: показать анимацию, звук и т.д.
        Debug.Log($"Used item {itemId}");
    }

    [ClientRpc]
    void RpcUpdateTransform(Vector3 pos, Quaternion rot)
    {
        if (!isLocalPlayer) // Только нелокальные игроки должны получать обновления
        {
            syncPosition = pos;
            syncRotation = rot;
        }
    }

    [ClientRpc]
    void RpcUpdatePitch(float newPitch)
    {
        if (!isLocalPlayer && cameraTransform != null) // ✅ Обновляем только для других игроков
        {
            cameraTransform.localRotation = Quaternion.Euler(newPitch, 0f, 0f);
        }
    }

    [ClientRpc]
    void RpcDoJump()
    {
        if (!canMove) return;

        if (controller.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    public void FreezeMovement()
    {
        canMove = false;
    }

    public void UnfreezeMovement()
    {
        canMove = true;
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        FreezeMovement();
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UnfreezeMovement();
    }


}
