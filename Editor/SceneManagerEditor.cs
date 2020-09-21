using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SceneManagerEditor : EditorWindow
{
    List<SceneAsset> _scenes = new List<SceneAsset>();

    [MenuItem("Window/Scene Managment Tool")]
    public static void ShowWindow()
    {
        GetWindow<SceneManagerEditor>("Scene Managment Tool");
    }

    void ReloadScenes()
    {
        _scenes = Resources.LoadAll<SceneAsset>("Scenes").ToList();
    }

    bool _beginEnd = false;

    private void OnGUI()
    {
        ReloadScenes();

        foreach(SceneAsset _scene in _scenes)
        {
            if(!_beginEnd)
                GUILayout.BeginHorizontal();

            var _sceneName = _scene.name;

            if (_sceneName.StartsWith("Game_Map"))
                _sceneName = _sceneName.Split('_')[2];

            if (GUILayout.Button(_sceneName))
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_scene));
            }

            if(_beginEnd)
                GUILayout.EndHorizontal();

            _beginEnd = !_beginEnd;
        }
    }
}
