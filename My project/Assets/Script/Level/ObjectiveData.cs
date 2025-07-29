using UnityEngine;

public enum ObjectiveType
{
    HoldKeyNearObject,
    ReachLocation,
    // Можно добавить другие типы
}

[CreateAssetMenu(fileName = "NewObjective", menuName = "Objectives/ObjectiveData")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveID;
    public string objectiveName;
    public ObjectiveType objectiveType;

    public float holdTime; // для HoldKey
    public KeyCode key = KeyCode.E;

    public GameObject targetPrefab; // объект для взаимодействия
    public Vector3 targetPosition; // для ReachLocation

    public float activationRange = 3f;
}
