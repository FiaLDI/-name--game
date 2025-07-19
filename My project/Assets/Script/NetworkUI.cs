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
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.singleton.StartHost();
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.singleton.StartClient();
        });

        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.singleton.StartServer();
        });
    }
}
