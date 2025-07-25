using UnityEngine;
using TMPro;
using System.Collections;

public class GlobalObjectiveUIController : MonoBehaviour
{
    public TMP_Text objectiveStatusText;

    private void OnEnable()
    {
        StartCoroutine(WaitForManager());
    }

    private IEnumerator WaitForManager()
    {
        while (GlobalObjectiveManager.Instance == null)
            yield return null;

        GlobalObjectiveManager.Instance.OnGlobalObjectiveStatusChanged += UpdateUI;
        UpdateUI(GlobalObjectiveManager.Instance.IsCompleted);
    }

    private void OnDisable()
    {
        if (GlobalObjectiveManager.Instance != null)
            GlobalObjectiveManager.Instance.OnGlobalObjectiveStatusChanged -= UpdateUI;
    }

    public void UpdateUI(bool isCompleted)
    {
        if (objectiveStatusText == null)
        {
            Debug.LogWarning("ObjectiveStatusText не назначен!");
            return;
        }

        objectiveStatusText.text = isCompleted
            ? "Глобальная цель выполнена!"
            : "Глобальная цель в процессе...";
    }
}
