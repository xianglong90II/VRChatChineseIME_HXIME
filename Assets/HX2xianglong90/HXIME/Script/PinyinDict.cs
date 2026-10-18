
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endif

namespace HX2xianglong90.HXIME
{
    public class PinyinDict : UdonSharpBehaviour
    {
        public string[] entries;
        public string[] pinyins;
        public int[] weights;
        public int[] indices;
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR 
    [CustomEditor(typeof(PinyinDict))]
    public class PinyinDictEditor : Editor
    {
        private PinyinDict targetScript;
        private static string filePath = "";

        private void OnEnable()
        {
            targetScript = (PinyinDict)target;
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            // 显示当前数据统计
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("当前字典数据", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"词条数量: {targetScript.entries?.Length ?? 0}");
            EditorGUILayout.LabelField($"拼音数量: {targetScript.pinyins?.Length ?? 0}");
            EditorGUILayout.LabelField($"权重数量: {targetScript.weights?.Length ?? 0}");

            // 文件加载区域
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("从文件加载字典", EditorStyles.boldLabel);

            // 文件路径输入
            filePath = EditorGUILayout.TextField("字典文件路径", filePath);
            
            // 浏览文件按钮
            if (GUILayout.Button("浏览文件..."))
            {
                string newPath = EditorUtility.OpenFilePanel("选择字典文件", "", "txt");
                if (!string.IsNullOrEmpty(newPath))
                {
                    filePath = newPath;
                }
            }

            // 加载并应用按钮
            if (GUILayout.Button("加载并应用字典"))
            {
                if (File.Exists(filePath))
                {
                    LoadAndApplyDictionary(filePath);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "文件不存在！", "确定");
                }
            }
        }

        private void LoadAndApplyDictionary(string path)
        {
            try
            {
                string[] allLines = File.ReadAllLines(path);
                var entriesList = new List<string>();
                var pinyinsList = new List<string>();
                var weightsList = new List<int>();
                var indicesList = new List<int>();
                int counter =0;

                foreach (string line in allLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string word = parts[0].Trim();
                    string pinyin = parts[1].Trim();
                    
                    // 处理权重 - 如果没有权重列，默认为0
                    int weight = 0;
                    if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2]))
                    {
                        int.TryParse(parts[2].Trim(), out weight);
                    }

                    entriesList.Add(word);
                    pinyinsList.Add(pinyin);
                    weightsList.Add(weight);
                    indicesList.Add(counter);
                    counter++;
                }

                // 直接应用到目标组件
                targetScript.entries = entriesList.ToArray();
                targetScript.weights = weightsList.ToArray();
                targetScript.pinyins = pinyinsList.ToArray();
                targetScript.indices = indicesList.ToArray();

                EditorUtility.SetDirty(targetScript);
                EditorUtility.DisplayDialog("成功", 
                    $"字典加载并应用成功！\n词条数量: {entriesList.Count}", 
                    "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"加载字典失败: {e.Message}", "确定");
            }
        }
    }
#endif
}