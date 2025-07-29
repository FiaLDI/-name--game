using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public class GlobalObjectiveListUIController : NetworkBehaviour
{
    public static GlobalObjectiveListUIController Instance { get; private set; }

    [SerializeField] private GameObject objectiveUIPrefab;
    [SerializeField] private Transform contentParent;

    private Dictionary<string, ObjectiveUIItem> uiItems = new Dictionary<string, ObjectiveUIItem>();
    private Dictionary<string, List<ObjectiveRuntime>> objectives = new Dictionary<string, List<ObjectiveRuntime>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RegisterObjective(ObjectiveRuntime objective)
    {
        if (objective.data == null) return;

        string id = objective.data.objectiveID;

        // Инициализация списка для этого типа цели
        if (!objectives.ContainsKey(id))
        {
            objectives[id] = new List<ObjectiveRuntime>();
        }
        objectives[id].Add(objective);

        // Создаем UI элемент если нужно
        if (!uiItems.ContainsKey(id))
        {
            var uiItem = Instantiate(objectiveUIPrefab, contentParent).GetComponent<ObjectiveUIItem>();
            uiItems[id] = uiItem;
            uiItem.SetName(objective.data.objectiveName);
        }

        objective.OnCompleted += () => UpdateObjectiveUI(id);
        UpdateObjectiveUI(id);
    }

    private void UpdateObjectiveUI(string id)
    {
        if (!objectives.TryGetValue(id, out var objectiveList) || !uiItems.TryGetValue(id, out var uiItem))
            return;

        int completed = objectiveList.Count(o => o.isCompleted);
        int total = objectiveList.Count;

        uiItem.SetProgress(completed, total);
        Debug.Log($"UI updated for {id}: {completed}/{total}");
    }

    [ClientRpc]
    public void RpcUpdateAllObjectives()
    {
        foreach (var id in objectives.Keys.ToList())
        {
            UpdateObjectiveUI(id);
        }
    }

    public void ClearAllObjectives()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        objectives.Clear();
        uiItems.Clear();
    }
}