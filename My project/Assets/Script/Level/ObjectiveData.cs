using UnityEngine;

public enum ObjectiveType
{
    HoldKeyNearObject,
    ReachLocation,
    // ����� �������� ������ ����
}

[CreateAssetMenu(fileName = "NewObjective", menuName = "Objectives/ObjectiveData")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveID;
    public string objectiveName;
    public ObjectiveType objectiveType;

    public float holdTime; // ��� HoldKey
    public KeyCode key = KeyCode.E;

    public GameObject targetPrefab; // ������ ��� ��������������
    public Vector3 targetPosition; // ��� ReachLocation

    public float activationRange = 3f;
}
