using Mirror;
using UnityEngine;
using System.Collections;

public class LevelManager : NetworkBehaviour
{
    [SerializeField] private string nextSceneName = "Level1";
    [SerializeField] private float delayBeforeSwitch = 3f;

    [Server]
    public void LoadNextLevel()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    [Server]
    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeSwitch);
        NetworkManager.singleton.ServerChangeScene(nextSceneName);
    }
}
