using UnityEngine;

public class HoldKeyObjective : IObjectiveMechanic
{
    private ObjectiveRuntime runtime;
    private float holdTimer;

    // Реализация свойства из интерфейса
    public bool IsCompleted { get; private set; }

    public void Initialize(ObjectiveRuntime objective)
    {
        runtime = objective;
        holdTimer = 0f;
        IsCompleted = false;
    }

    public void Update()
    {
        if (IsCompleted || runtime.isCompleted) return;

        float distance = Vector3.Distance(runtime.player.position, runtime.transform.position);
        if (distance <= runtime.data.activationRange)
        {
            var interactAction = runtime.GetInteractAction();
            if (interactAction.IsPressed())
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= runtime.data.holdTime)
                {
                    IsCompleted = true;
                    runtime.CompleteObjective();
                }
            }
            else
            {
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }
}