using Mirror;
using UnityEngine;

public class ObjectiveManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] objectivePrefabs;

    public override void OnStartLocalPlayer()
    {
        if (GlobalObjectiveListUIController.Instance == null)
        {
            Debug.LogError("GlobalObjectiveListUIController not found!");
            return;
        }

        if (isServer)
        {
            CmdSpawnObjectives();
        }
    }

    [Command]
    private void CmdSpawnObjectives()
    {
        if (objectivePrefabs == null || objectivePrefabs.Length == 0)
        {
            //Debug.LogError("Objective prefabs not assigned!");
            return;
        }

        foreach (var prefab in objectivePrefabs)
        {
            GameObject instance = Instantiate(prefab);
            NetworkServer.Spawn(instance);

            // Регистрируем цель на сервере
            var objective = instance.GetComponent<ObjectiveRuntime>();
            if (objective != null)
            {
                TargetRegisterObjective(connectionToClient, objective.netId);
            }
        }
    }

    [TargetRpc]
    private void TargetRegisterObjective(NetworkConnection target, uint netId)
    {
        if (NetworkClient.spawned.TryGetValue(netId, out var obj))
        {
            var objective = obj.GetComponent<ObjectiveRuntime>();
            if (objective != null && GlobalObjectiveListUIController.Instance != null)
            {
                GlobalObjectiveListUIController.Instance.RegisterObjective(objective);
            }
        }
    }
}