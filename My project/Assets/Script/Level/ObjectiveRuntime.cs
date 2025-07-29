using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectiveRuntime : NetworkBehaviour
{
    public ObjectiveData data;
    [SyncVar(hook = nameof(OnIsCompletedChanged))]
    public bool isCompleted = false;
    public bool isGlobal = false;

    [Header("Player References")]
    public Transform player;
    public PlayerInputActions inputActions;
    public InputAction interactAction;

    public System.Action OnCompleted;

    [Header("Mechanics")]
    private IObjectiveMechanic mechanic;

    public event System.Action OnObjectiveCompleted;

    [Header("Visuals (Optional)")]
    public Renderer targetRenderer; // Например, MeshRenderer или SkinnedMeshRenderer
    public Color completedColor = Color.green;
    public Material completedMaterial;
    public bool changeColorOnComplete = false;
    public bool changeMaterialOnComplete = false;

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError("ObjectiveData not assigned!", this);
            return;
        }

        InitializeMechanic();
    }

    private void InitializeMechanic()
    {
        if (mechanic != null) return;
        switch (data.objectiveType)
        {
            case ObjectiveType.HoldKeyNearObject:
                mechanic = new HoldKeyObjective();
                break;
            case ObjectiveType.ReachLocation:
                mechanic = new ReachLocationObjective();
                break;
        }

        if (mechanic != null)
        {
            mechanic.Initialize(this);
        }
    }

    public void Initialize(Transform playerTransform, PlayerInputActions inputs)
    {
        player = playerTransform;
        inputActions = inputs;
        interactAction = inputActions.Player.Interact;
        interactAction.Enable();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (GlobalObjectiveListUIController.Instance != null)
        {
            GlobalObjectiveListUIController.Instance.RegisterObjective(this);
        }
    }

    private void Update()
    {
        if (isCompleted || mechanic == null || player == null)
            return;

        mechanic.Update();
    }

    private void OnIsCompletedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            OnCompleted?.Invoke();
            if (isLocalPlayer)
            {
                GlobalObjectiveListUIController.Instance?.RpcUpdateAllObjectives();
            }

            if (changeColorOnComplete  && targetRenderer != null)
            {
                // Создаем копию материала, чтобы не менять sharedMaterial
                var newMaterial = new Material(targetRenderer.material);
                newMaterial.color = completedColor;
                targetRenderer.material = newMaterial;
            }


            if (changeMaterialOnComplete && completedMaterial != null)
            {
                targetRenderer.material = completedMaterial;
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CompleteObjective()
    {
        if (isCompleted) return;

        if (isGlobal)
        {
            CmdCompleteGlobalObjective();
        }
        else
        {
            CmdComplete();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdComplete()
    {
        isCompleted = true;
    }

    [Command(requiresAuthority = false)]
    private void CmdCompleteGlobalObjective()
    {
        isCompleted = true;
        GlobalObjectiveManager.Instance?.MarkGlobalObjectiveComplete(data.objectiveID);
    }

    private void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.Disable();
        }
    }

    [ServerCallback]
    public void Complete()
    {
        isCompleted = true;
    }

    public InputAction GetInteractAction() => interactAction;
}