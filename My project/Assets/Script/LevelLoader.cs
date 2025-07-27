using UnityEngine;
using Mirror;

public class LevelLoader : MonoBehaviour
{
    public void OnStartLevelButtonPressed(string levelName)
    {
        Debug.Log($"Changing scene to {levelName}...");

        if (NetworkServer.active)
        {
            // Очищаем старые цели перед сменой сцены
            if (GlobalObjectiveListUIController.Instance != null)
            {
                GlobalObjectiveListUIController.Instance.ClearAllObjectives();
            }

            NetworkManager.singleton.ServerChangeScene(levelName);
        }
        else
        {
            Debug.LogWarning("Only server can change scenes!");
        }
    }
}