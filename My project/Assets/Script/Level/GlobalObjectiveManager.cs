using Mirror;
using UnityEngine;
using System;

public class GlobalObjectiveManager : NetworkBehaviour
{
    public static GlobalObjectiveManager Instance;

    public event Action<bool> OnGlobalObjectiveStatusChanged;

    [SyncVar(hook = nameof(OnObjectiveCompletedChanged))]
    private bool objectiveCompleted = false;

    public bool IsCompleted => objectiveCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RefreshUI()
    {
        OnGlobalObjectiveStatusChanged?.Invoke(objectiveCompleted);
    }


    [Server]
    public void MarkGlobalObjectiveComplete()
    {
        if (objectiveCompleted) return;

        objectiveCompleted = true;
        Debug.Log("Глобальная цель выполнена на сервере!");
    }

    void OnObjectiveCompletedChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"Глобальная цель изменилась: {newValue}");
        OnGlobalObjectiveStatusChanged?.Invoke(newValue);
    }
}
