using UnityEngine;

public class Unpause : MonoBehaviour
{
    public void onUnpauseCliclHandler()
    {
        if (FirstPersonController.Instance != null)
        {
            FirstPersonController.Instance.ResumeGame();
        }
    }
}
