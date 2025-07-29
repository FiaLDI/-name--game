using UnityEngine;
using TMPro;
using Mirror;

public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField ipInputField;

    // Приведение singleton к CustomNetworkManager для удобства
    private CustomNetworkManager customNetworkManager;

    private void Start()
    {
        Debug.Log("MainMenuUI.Start called");

        if (NetworkManager.singleton == null)
        {
            Debug.LogError("❌ NetworkManager.singleton is NULL!");
            return;
        }

        customNetworkManager = NetworkManager.singleton as CustomNetworkManager;

        if (customNetworkManager == null)
        {
            Debug.LogError("❌ NetworkManager singleton is NOT CustomNetworkManager!");
        }
        else
        {
            Debug.Log("✅ CustomNetworkManager assigned in Start");
        }
    }


    public void OnClickHost()
    {
        Debug.Log($"NetworkManager.singleton: {NetworkManager.singleton}");

        if (customNetworkManager != null)
        {
            if (customNetworkManager.isNetworkActive)
            {
                Debug.LogWarning("Network is already active!");
                return;
            }

            Debug.Log("Host button pressed");
            customNetworkManager.StartHost();
            Debug.Log("Started as Host");
        }
    }



    public void OnClickClient()
    {
        if (customNetworkManager != null)
        {
            string ip = ipInputField.text;
            if (!string.IsNullOrEmpty(ip))
            {
                customNetworkManager.networkAddress = ip;
                customNetworkManager.StartClient();
                Debug.Log("Connecting to " + ip);
            }
            else
            {
                Debug.LogWarning("IP address is empty!");
            }
        }
    }

    public void OnClickServerOnly()
    {
        if (customNetworkManager != null)
        {
            customNetworkManager.StartServer();
            Debug.Log("Started Server Only");
            // Переключение сцены можно добавить аналогично OnStartHost()
            customNetworkManager.ServerChangeScene(customNetworkManager.hubSceneName);
        }
    }
}
