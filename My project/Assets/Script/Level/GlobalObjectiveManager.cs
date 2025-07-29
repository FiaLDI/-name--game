using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System;

public class GlobalObjectiveManager : NetworkBehaviour
{
    public static GlobalObjectiveManager Instance;

    [SyncVar(hook = nameof(OnObjectivesChanged))]
    private string serializedObjectives;

    // Храним список целей локально как List
    private List<GlobalObjectiveEntry> objectives = new List<GlobalObjectiveEntry>();

    // Выдаём только для чтения наружу
    public IReadOnlyList<GlobalObjectiveEntry> Objectives => objectives;

    public event Action<IReadOnlyList<GlobalObjectiveEntry>> OnObjectivesUpdated;
    public event Action<bool> OnGlobalObjectiveStatusChanged;
    public bool IsCompleted => objectives.TrueForAll(o => o.isCompleted);


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Server]
    public void AddGlobalObjective(string id, string name)
    {
        if (objectives.Exists(o => o.objectiveID == id))
            return;

        objectives.Add(new GlobalObjectiveEntry
        {
            objectiveID = id,
            objectiveName = name,
            isCompleted = false
        });

        SyncObjectives();
    }

    [Server]
    public void MarkGlobalObjectiveComplete(string id)
    {
        var obj = objectives.Find(o => o.objectiveID == id);
        if (obj != null && !obj.isCompleted)
        {
            obj.isCompleted = true;
            SyncObjectives();
            Debug.Log($"Глобальная цель выполнена: {obj.objectiveName}");
        }
    }

    [Server]
    private void SyncObjectives()
    {
        serializedObjectives = JsonUtility.ToJson(new ObjectiveListWrapper { list = objectives });
    }

    void OnObjectivesChanged(string oldValue, string newValue)
    {
        ObjectiveListWrapper wrapper = JsonUtility.FromJson<ObjectiveListWrapper>(newValue);
        if (wrapper != null && wrapper.list != null)
            objectives = wrapper.list;
        else
            objectives = new List<GlobalObjectiveEntry>();

        OnObjectivesUpdated?.Invoke(objectives.AsReadOnly());

        bool isCompleted = objectives.TrueForAll(o => o.isCompleted);
        OnGlobalObjectiveStatusChanged?.Invoke(isCompleted);

    }

    [System.Serializable]
    public class GlobalObjectiveEntry
    {
        public string objectiveID;
        public string objectiveName;
        public bool isCompleted;
    }

    [System.Serializable]
    private class ObjectiveListWrapper
    {
        public List<GlobalObjectiveEntry> list = new List<GlobalObjectiveEntry>();
    }
}
