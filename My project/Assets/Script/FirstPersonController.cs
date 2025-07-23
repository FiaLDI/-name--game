using Mirror;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private bool canMove = true;
    private bool isPaused = false;

    public static event Action<Transform, FirstPersonController> OnLocalPlayerReady;

    // Для синхронизации позиции и поворота
    [SyncVar] private Vector3 syncPosition;
    [SyncVar] private Quaternion syncRotation;

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
        Debug.Log("OnStartLocalPlayer called");
        inputActions = new PlayerInputActions();
        inputActions.Enable();
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Pause.performed += OnPausePerformed;
        Instance = this;

        if (cameraTransform != null)
        {
            cameraTransform.gameObject.SetActive(true);
            Debug.Log("Camera enabled for local player");
        }
        else
        {
            Debug.LogWarning("cameraTransform is null!");
        }

        // Временно закомментируй
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnLocalPlayerReady?.Invoke(transform, this);
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        inputActions.Player.Jump.performed -= OnJumpPerformed;
        inputActions.Player.Pause.performed -= OnPausePerformed;

        inputActions.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (isLocalPlayer)
        {
            inputActions = new PlayerInputActions();
            inputActions.Enable();
        }
        else
        {
            if (cameraTransform != null)
                cameraTransform.gameObject.SetActive(false);
        }
    }


    void Update()
    {
        if (!isLocalPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, syncPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, syncRotation, 360f * Time.deltaTime);
            return;
        }

        if (!canMove)
            return;

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleLook();
        HandleMovement();

        CmdSendTransform(transform.position, transform.rotation);
    }
    void OnDisable()
    {
        if (inputActions != null)
        { 
            inputActions.Player.Jump.performed -= OnJumpPerformed;
            inputActions.Disable();
        }    
            
    }

    void HandleLook()
    {
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

        pitch -= lookInput.y * lookSensitivity * 0.1f;
        pitch = Mathf.Clamp(pitch, -25f, 25f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
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
    void CmdSendTransform(Vector3 pos, Quaternion rot)
    {
        syncPosition = pos;
        syncRotation = rot;
    }

    [Command]
    void CmdRequestJump()
    {
        RpcDoJump(); // Триггерим прыжок на всех клиентах
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
