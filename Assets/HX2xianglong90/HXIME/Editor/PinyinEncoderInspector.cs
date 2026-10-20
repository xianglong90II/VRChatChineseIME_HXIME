#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HX2xianglong90.HXIME.Editor
{
    [CustomEditor(typeof(PinyinEncoder))]
    public sealed class PinyinEncoderInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("simplifiedDictionary"), new GUIContent("简体源词库"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("traditionalDictionary"), new GUIContent("繁体源词库"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("capturePlaneRenderer"), new GUIContent("相机拍摄平面 Renderer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("previewRenderer"), new GUIContent("词库预览 Renderer（可选）"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.HelpBox("构建时会从场景中移除该编辑器工具组件，不会删除其所在物体或其他组件。请指定相机拍摄平面；词库预览平面可选。简繁词库需为“词条<TAB>拼音<TAB>词频”格式。", MessageType.Info);
            if (GUILayout.Button("烘焙词库并接入当前场景"))
            {
                PinyinEncoder encoder = (PinyinEncoder)target;
                PinyinEncoderSetup.BakeAndWireCurrentScene(encoder.simplifiedDictionary, encoder.traditionalDictionary, encoder.capturePlaneRenderer, encoder.previewRenderer);
            }
        }
    }

    public sealed class PinyinEncoderBuildStripper : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null) return;
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                PinyinEncoder[] encoders = root.GetComponentsInChildren<PinyinEncoder>(true);
                foreach (PinyinEncoder encoder in encoders)
                    if (encoder != null) Object.DestroyImmediate(encoder);
            }
        }
    }
}
#endif
