using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class ObjectiveRuntime : NetworkBehaviour
{
    public ObjectiveData data;
    [SyncVar] public bool isCompleted = false;
    public bool isGlobal = false;

    public Transform player;

    private float timer = 0f;

    private PlayerInputActions inputActions;
    private InputAction interactAction;

    public void Initialize(Transform playerTransform, PlayerInputActions playerInputActions)
    {
        Debug.Log("ObjectiveRuntime Initialize called");
        player = playerTransform;
        inputActions = playerInputActions;
        interactAction = inputActions.Player.Interact;
        Debug.Log($"Interact action assigned: {interactAction != null}");
        interactAction.Enable();
    }


    void Start()
    {

        Debug.Log($"ObjectiveRuntime Start. player = {player}, interactAction = {interactAction}");
    }



    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.Disable();
    }

    void Update()
    {
        if (player == null)
            return;

        if (interactAction == null)
        {
            Debug.LogWarning("Interact action is null, waiting for Initialize");
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= data.activationRange)
        {
            if (interactAction.WasPressedThisFrame() || interactAction.ReadValue<float>() > 0)
            {
                timer += Time.deltaTime;
                if (timer >= data.holdTime)
                {
                    if (isGlobal)
                        CmdCompleteGlobalObjective();
                    else
                        CmdComplete();
                    isCompleted = true;
                }
            }
            else
            {
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    [Command(requiresAuthority = false)]
    void CmdComplete()
    {
        isCompleted = true;
        RpcOnCompleted();
    }

    [Command(requiresAuthority = false)]
    void CmdCompleteGlobalObjective()
    {
        isCompleted = true;
        GlobalObjectiveManager.Instance.MarkGlobalObjectiveComplete();
    }

    [ClientRpc]
    public void RpcOnCompleted()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
        Debug.Log($"Objective completed: {data.objectiveName}");
    }
}
