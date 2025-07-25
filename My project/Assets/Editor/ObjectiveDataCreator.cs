#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ObjectiveDataCreator
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/ObjectiveData Asset")]
    public static void CreateAsset()
    {
        ObjectiveData asset = ScriptableObject.CreateInstance<ObjectiveData>();
        AssetDatabase.CreateAsset(asset, "Assets/NewObjectiveData.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}
