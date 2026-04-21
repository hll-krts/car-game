using UnityEngine;

[CreateAssetMenu(fileName = "LevelsDataScriptableObject", menuName = "Scriptable Objects/LevelsDataScriptableObject")]
public class LevelsDataScriptableObject : ScriptableObject
{
    public Texture2D[] _sceneImages;

    public int SceneCount
    {
        get { return _sceneImages.Length; }
    }
}
