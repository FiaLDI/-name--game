using UnityEngine;

public abstract class Objective : MonoBehaviour
{
    public string objectiveName;
    public bool isCompleted;

    public abstract void StartObjective();
    public abstract void UpdateObjective();
}
