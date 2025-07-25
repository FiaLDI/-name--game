using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject inventoryCanvas; // Твой Canvas с инвентарём

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            // В меню инвентарь не нужен — выключаем Canvas
            if (inventoryCanvas != null)
                inventoryCanvas.SetActive(false);
        }
        else
        {
            // В других сценах — включаем инвентарь
            if (inventoryCanvas != null)
                inventoryCanvas.SetActive(true);
        }
    }
}
