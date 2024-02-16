using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;


[CustomEditor(typeof(LuBanDataTableComponent))]
public class LuBanDataTableComponentEditor : Editor
{
    private SerializedProperty _fileNameList;
    private SerializedProperty _sizeList;

    private void OnEnable()
    {
        _fileNameList = serializedObject.FindProperty("fileNameList");
        _sizeList = serializedObject.FindProperty("sizeList");
    }

    private const string DataTablePath = "Assets/AssetRaw/DataTable";

    const int GB = 1024 * 1024 * 1024; //定义GB的计算常量
    const int MB = 1024 * 1024; //定义MB的计算常量
    const int KB = 1024; //定义KB的计算常量

    private string ByteConversionGBMBKB(long KSize)
    {
        if (KSize / GB >= 1) //如果当前Byte的值大于等于1GB
            return (Math.Round(KSize / (float)GB, 2)).ToString() + "GB"; //将其转换成GB
        else if (KSize / MB >= 1) //如果当前Byte的值大于等于1MB
            return (Math.Round(KSize / (float)MB, 2)).ToString() + "MB"; //将其转换成MB
        else if (KSize / KB >= 1) //如果当前Byte的值大于等于1KB
            return (Math.Round(KSize / (float)KB, 2)).ToString() + "KB"; //将其转换成KGB
        else
            return KSize.ToString() + "Byte"; //显示Byte值
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (Application.isPlaying)
        {
            GUILayout.BeginVertical();
            {
                if (_fileNameList != null && _sizeList != null)
                {
                    for (int i = 0; i < _fileNameList.arraySize; i++)
                    {
                        GUILayout.BeginHorizontal("Box");
                        {
                            EditorGUILayout.LabelField(_fileNameList.GetArrayElementAtIndex(i).stringValue,
                                ByteConversionGBMBKB(_sizeList.GetArrayElementAtIndex(i).longValue));
                            if (GUILayout.Button("选择"))
                            {
                                string path = Utility.Text.Format("{0}/{1}.json", DataTablePath,
                                    _fileNameList.GetArrayElementAtIndex(i).stringValue);
                                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                                EditorGUIUtility.PingObject(obj);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Not Find DataTable", MessageType.Info);
                }
            }
            GUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
        }


        serializedObject.ApplyModifiedProperties();
    }
}