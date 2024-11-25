using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]

public class SceneNameDrawer: PropertyDrawer
{
    int sceneIndex = -1;
    GUIContent[] sceneNames;
    readonly string[] scenePathSplit = { "/", ".unity" };               //readonly �ֶ�ֻ������ʱ���ڹ��캯���н��г�ʼ���������ں����н���ʹ��
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorBuildSettings.scenes.Length == 0) return;
        if(sceneIndex == -1)
        {
            GetSceneNameArray(property);
        }

        // ʵʱ���������б������ֵ����������˵Ļ�
        int oldIndex = sceneIndex;
        sceneIndex =  EditorGUI.Popup(position, label, sceneIndex, sceneNames);

        if (oldIndex != sceneIndex)
            property.stringValue = sceneNames[sceneIndex].text;
    }
    private void GetSceneNameArray(SerializedProperty property)
    {
        var scenes = EditorBuildSettings.scenes;
        // ��ʼ����ֵ
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


        // ��ʼ����ʾ�����б������ֵ
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