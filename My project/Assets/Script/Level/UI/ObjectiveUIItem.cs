using TMPro;
using UnityEngine;

public class ObjectiveUIItem : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI progressText;

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void SetProgress(int completed, int total)
    {
        progressText.text = $"{completed} / {total}";
    }
}
