using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveManager : NetworkBehaviour
{
    [SyncVar]
    private string objectiveID;

    public ObjectiveData currentObjective;

    public override void OnStartLocalPlayer()
    {
        tag = "Player";
        CmdRequestObjective();
    }

    [Command]
    void CmdRequestObjective()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string chosenObjective = GetObjectiveIDByScene(sceneName);

        RpcSetObjective(chosenObjective);
    }

    string GetObjectiveIDByScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Level_1":
                return "Objective_Level1"; // פאיכ המכזום בûעü: Resources/Objective_Level1.asset
            case "Level_2":
                return "Objective_Level2";
            default:
                return "NewObjective";
        }
    }


    [ClientRpc]
    void RpcSetObjective(string id)
    {
        objectiveID = id;
        currentObjective = Resources.Load<ObjectiveData>(id);

        if (currentObjective == null)
        {
            Debug.LogError("Objective not found: " + id);
            return;
        }

        SpawnObjective();
    }

    void SpawnObjective()
    {
        GameObject prefab = Resources.Load<GameObject>("ObjectiveRuntimePrefab");

        if (prefab == null)
        {
            Debug.LogError("Prefab not found in Resources");
            return;
        }

        GameObject instance = Instantiate(prefab, transform.position + Vector3.forward * 2, Quaternion.identity);
        var runtime = instance.GetComponent<ObjectiveRuntime>();
        runtime.data = currentObjective;
        runtime.player = this.transform;
        runtime.isGlobal = true;

        NetworkServer.Spawn(instance);
    }
}
