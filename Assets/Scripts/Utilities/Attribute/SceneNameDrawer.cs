using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]

public class SceneNameDrawer: PropertyDrawer
{
    int sceneIndex = -1;
    GUIContent[] sceneNames;
    readonly string[] scenePathSplit = { "/", ".unity" };               //readonly 字段只在声明时或在构造函数中进行初始化。不能在函数中进行使用
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorBuildSettings.scenes.Length == 0) return;
        if(sceneIndex == -1)
        {
            GetSceneNameArray(property);
        }

        // 实时更新下拉列表里面的值，如果更新了的话
        int oldIndex = sceneIndex;
        sceneIndex =  EditorGUI.Popup(position, label, sceneIndex, sceneNames);

        if (oldIndex != sceneIndex)
            property.stringValue = sceneNames[sceneIndex].text;
    }
    private void GetSceneNameArray(SerializedProperty property)
    {
        var scenes = EditorBuildSettings.scenes;
        // 初始化数值
        sceneNames = new GUIContent[scenes.Length];

        for(int i = 0; i < sceneNames.Length; i++)
        {
            string path = scenes[i].path;
            var splitPath = path.Split(scenePathSplit, StringSplitOptions.RemoveEmptyEntries);

            string sceneName = "";

            if(splitPath.Length > 0)
            {
                sceneName = splitPath[splitPath.Length - 1];
            }
            else
            {
                sceneName = "(Deleted Scene)";
            }

            sceneNames[i] = new GUIContent(sceneName);
        }

        if(sceneNames.Length == 0)
        {
            sceneNames = new[] { new GUIContent("Check your build settings!") }; 
        }


        // 初始化显示下拉列表里面的值
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool nameFound = false;
            for(int i = 0; i < sceneNames.Length; i++)
            {
                if(property.stringValue == sceneNames[i].text)
                {
                    sceneIndex = i;
                    nameFound = true;
                    break;
                }
            }
            if (!nameFound)
            {
                sceneIndex = 0;
            }
        }
        else
        {
            sceneIndex = 0;
        }

        property.stringValue = sceneNames[sceneIndex].text;
    }
}