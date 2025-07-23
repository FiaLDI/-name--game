using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;

    void Start()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(() => NetworkManager.singleton.StartHost());
        else
            Debug.LogError("hostButton is not assigned in the inspector!");

        if (clientButton != null)
            clientButton.onClick.AddListener(() => NetworkManager.singleton.StartClient());
        else
            Debug.LogError("clientButton is not assigned in the inspector!");

        if (serverButton != null)
            serverButton.onClick.AddListener(() => NetworkManager.singleton.StartServer());
        else
            Debug.LogError("serverButton is not assigned in the inspector!");
    }

}
