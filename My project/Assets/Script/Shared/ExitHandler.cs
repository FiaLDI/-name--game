using UnityEngine;
using Mirror;

public class ExitHandler : MonoBehaviour
{
    public void OnExitButtonPressed()
    {
        if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

#if UNITY_EDITOR
        // ��� ��������� Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ��� ������
        Application.Quit();
#endif
    }
}
