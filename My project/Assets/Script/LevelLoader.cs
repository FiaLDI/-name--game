using UnityEngine;
using Mirror;

public class LevelLoader : MonoBehaviour
{
    void Start()
    {
        Debug.Log("LevelLoader script started");
    }

    public void OnClick()
    {
        Debug.Log("Button clicked!");
    }

    // Этот метод вызовется при нажатии кнопки
    public void OnStartLevelButtonPressed()
    {
        Debug.Log("Button pressed, trying to change scene...");
        if (NetworkServer.active)
        {
            Debug.Log("Server active, changing scene to Level1");
            NetworkManager.singleton.ServerChangeScene("1 LEVEL");
        }
        else
        {
            Debug.LogWarning("Not server, can't change scene!");
        }
    }

}
