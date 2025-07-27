using UnityEngine;

public class ReachLocationObjective : IObjectiveMechanic
{
    private ObjectiveRuntime runtime;

    public bool IsCompleted { get; private set; }

    public void Initialize(ObjectiveRuntime objective)
    {
        runtime = objective;
        IsCompleted = false; // Явный сброс состояния при инициализации

        // Проверка обязательных параметров
        if (runtime.data == null)
        {
            Debug.LogError("ObjectiveData not assigned!", runtime.gameObject);
            return;
        }
    }

    public void Update()
    {
        // Проверяем все возможные условия выхода
        if (IsCompleted || runtime.isCompleted || runtime.player == null)
            return;

        // Проверяем достижение цели
        float distance = Vector3.Distance(
            runtime.player.position,
            runtime.data.targetPosition
        );

        if (distance <= runtime.data.activationRange)
        {
            CompleteObjective();
        }
    }

    private void CompleteObjective()
    {
        IsCompleted = true;

        // Вызываем завершение цели на сервере
        if (runtime.isServer)
        {
            runtime.CompleteObjective();
        }
        else
        {
            Debug.LogWarning("Trying to complete objective from client!");
        }
    }
}