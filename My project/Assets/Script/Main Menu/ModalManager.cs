using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject modalPanel;

    public void ShowModal()
    {
        modalPanel.SetActive(true);
    }

    public void HideModal()
    {
        modalPanel.SetActive(false);
    }
}
