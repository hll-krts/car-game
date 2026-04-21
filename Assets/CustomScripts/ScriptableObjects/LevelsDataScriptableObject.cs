using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsDataScriptableObject", menuName = "Scriptable Objects/LevelsDataScriptableObject")]
public class LevelsDataScriptableObject : ScriptableObject
{
    public SceneAsset[] _scenes;
    public Texture2D[] _sceneImages;

    public int SceneCount
    {
        get { return _scenes.Length; }
    }
}
