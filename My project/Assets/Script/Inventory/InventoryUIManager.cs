using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject inventoryCanvas; // ���� Canvas � ���������

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            // � ���� ��������� �� ����� � ��������� Canvas
            if (inventoryCanvas != null)
                inventoryCanvas.SetActive(false);
        }
        else
        {
            // � ������ ������ � �������� ���������
            if (inventoryCanvas != null)
                inventoryCanvas.SetActive(true);
        }
    }
}
