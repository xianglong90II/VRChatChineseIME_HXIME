#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UdonSharpEditor;

namespace HX2xianglong90.HXIME.Editor
{
    internal static class PinyinEncoderSetup
    {
        private const int Width = 512;
        private const int Height = 256;
        private const int Capacity = Width * Height;
        private const int ScoreStride = 5000000;
        private const string GeneratedRoot = "Assets/HX2xianglong90/HXIME/Generated";

        public static void BakeAndWireCurrentScene(TextAsset simplifiedDictionary, TextAsset traditionalDictionary, Renderer capturePlaneRenderer, Renderer previewRenderer)
        {
            try
            {
                if (simplifiedDictionary == null || traditionalDictionary == null)
                    throw new InvalidOperationException("请分别指定简体和繁体词库 TextAsset。");
                if (capturePlaneRenderer == null)
                    throw new InvalidOperationException("请指定相机拍摄平面的 Renderer。");

                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string simpPath = Path.Combine(projectRoot, AssetDatabase.GetAssetPath(simplifiedDictionary));
                string tradPath = Path.Combine(projectRoot, AssetDatabase.GetAssetPath(traditionalDictionary));
                DictionaryAssets simp = Encode(simpPath, GeneratedRoot + "/Simp");
                DictionaryAssets trad = Encode(tradPath, GeneratedRoot + "/Trad");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                WireCurrentScene(simp, trad, capturePlaneRenderer, previewRenderer);
                EditorUtility.DisplayDialog("HXIME LM", "简繁词库已烘焙并接入当前场景。请保存场景后进入 Play Mode 测试。", "确定");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("HXIME LM 设置失败", exception.Message, "确定");
            }
        }

        private sealed class DictionaryAssets
        {
            public Texture2D texA;
            public Texture2D texB;
            public Texture2D fullTexA;
            public Texture2D fullTexB;
            public Texture2D metadata;
            public int count;
        }

        private static DictionaryAssets Encode(string sourcePath, string relativeFolder)
        {
            byte[] dataA = new byte[Capacity * 4];
            byte[] dataB = new byte[Capacity * 4];
            byte[] fullDataA = new byte[Capacity * 4];
            byte[] fullDataB = new byte[Capacity * 4];
            byte[] metadata = new byte[Capacity * 4];
            string[] lines = File.ReadAllLines(sourcePath, Encoding.UTF8);
            int count = 0;

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] fields = line.Split('\t');
                if (fields.Length != 3)
                    throw new FormatException($"第 {lineNumber + 1} 行需要恰好 3 列（词条、拼音、词频）。");
                if (count >= Capacity)
                    throw new InvalidOperationException($"词条数量超过贴图容量 {Capacity:N0}。");

                string pinyin = fields[1].Trim();
                if (pinyin.Length == 0)
                    throw new FormatException($"第 {lineNumber + 1} 行拼音为空。");
                if (!int.TryParse(fields[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int weight) ||
                    weight < 0 || weight >= ScoreStride)
                    throw new FormatException($"第 {lineNumber + 1} 行词频必须是 0 到 {ScoreStride - 1} 的整数。");

                int offset = count * 4;
                int charIndex = 0;
                int fullCharIndex = 0;
                string[] syllables = pinyin.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (string syllable in syllables)
                {
                    char initial = char.ToLowerInvariant(syllable[0]);
                    if (initial < 'a' || initial > 'z')
                        throw new FormatException($"第 {lineNumber + 1} 行拼音只能包含 ASCII 字母音节。");
                    int id = initial - 'a' + 1;
                    if (charIndex >= 8) throw new FormatException($"第 {lineNumber + 1} 行超过 8 个拼音音节，不能无损烘焙。");
                    if (charIndex < 4) dataA[offset + charIndex] = (byte)id;
                    else dataB[offset + charIndex - 4] = (byte)id;
                    charIndex++;

                    for (int letterIndex = 0; letterIndex < syllable.Length; letterIndex++)
                    {
                        char letter = char.ToLowerInvariant(syllable[letterIndex]);
                        if (letter < 'a' || letter > 'z')
                            throw new FormatException($"第 {lineNumber + 1} 行拼音只能包含 ASCII 字母。");
                        if (fullCharIndex < 8)
                        {
                            int letterId = letter - 'a' + 1;
                            if (fullCharIndex < 4) fullDataA[offset + fullCharIndex] = (byte)letterId;
                            else fullDataB[offset + fullCharIndex - 4] = (byte)letterId;
                        }
                        fullCharIndex++;
                    }
                }
                if (charIndex == 0) throw new FormatException($"第 {lineNumber + 1} 行没有有效拼音音节。");

                int metadataOffset = count * 4;
                metadata[metadataOffset] = (byte)(weight & 0xFF);
                metadata[metadataOffset + 1] = (byte)((weight >> 8) & 0xFF);
                metadata[metadataOffset + 2] = (byte)((weight >> 16) & 0xFF);
                metadata[metadataOffset + 3] = (byte)Mathf.Min(fullCharIndex, 255);
                count++;
            }

            string folder = relativeFolder.TrimEnd('/');
            string fullFolder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, folder);
            Directory.CreateDirectory(fullFolder);
            string dataAPath = Path.Combine(fullFolder, "Pinyin_DataTex_A.png");
            string dataBPath = Path.Combine(fullFolder, "Pinyin_DataTex_B.png");
            string fullDataAPath = Path.Combine(fullFolder, "Pinyin_FullTex_A.png");
            string fullDataBPath = Path.Combine(fullFolder, "Pinyin_FullTex_B.png");
            string metadataPath = Path.Combine(fullFolder, "MetaDataTex.png");

            WritePng(dataA, dataAPath);
            WritePng(dataB, dataBPath);
            WritePng(fullDataA, fullDataAPath);
            WritePng(fullDataB, fullDataBPath);
            WritePng(metadata, metadataPath);
            AssetDatabase.Refresh();
            ConfigurePngImporter(folder + "/Pinyin_DataTex_A.png");
            ConfigurePngImporter(folder + "/Pinyin_DataTex_B.png");
            ConfigurePngImporter(folder + "/Pinyin_FullTex_A.png");
            ConfigurePngImporter(folder + "/Pinyin_FullTex_B.png");
            ConfigurePngImporter(folder + "/MetaDataTex.png");
            AssetDatabase.DeleteAsset(folder + "/MetaDataTex.asset");
            AssetDatabase.Refresh();
            Debug.Log($"HXIME GPU 词库烘焙完成：{count:N0} 个词条，输出到 {folder}。");
            return new DictionaryAssets
            {
                texA = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/Pinyin_DataTex_A.png"),
                texB = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/Pinyin_DataTex_B.png"),
                fullTexA = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/Pinyin_FullTex_A.png"),
                fullTexB = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/Pinyin_FullTex_B.png"),
                metadata = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/MetaDataTex.png"),
                count = count
            };
        }

        private static void WritePng(byte[] bytes, string path)
        {
            Color32[] pixels = new Color32[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                int offset = i * 4;
                pixels[i] = new Color32(bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]);
            }

            Texture2D texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false, true);
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static void ConfigurePngImporter(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"无法导入贴图：{assetPath}");

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = Width;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }

        private static void WireCurrentScene(DictionaryAssets simp, DictionaryAssets trad, Renderer capturePlaneRenderer, Renderer previewRenderer)
        {
            HXIMEUI ui = UnityEngine.Object.FindObjectOfType<HXIMEUI>();
            if (ui == null) throw new InvalidOperationException("当前场景找不到 HXIMEUI。请打开包含 HXIME_Pinyin 的场景后重试。");
            PinyinEngine engine = (PinyinEngine)typeof(HXIMEUI).GetField("engine", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(ui);
            if (engine == null) throw new InvalidOperationException("HXIMEUI 尚未配置传统 PinyinEngine 引用。");

            GameObject root = ui.gameObject;
            PinyinLM lm = root.GetComponent<PinyinLM>();
            if (lm == null) lm = root.AddUdonSharpComponent<PinyinLM>();
            MeshRenderer capturePlane = capturePlaneRenderer as MeshRenderer;
            if (capturePlane == null) throw new InvalidOperationException("相机拍摄平面的 Renderer 必须是 MeshRenderer。");
            if (capturePlane.sharedMaterial == null) throw new InvalidOperationException("相机拍摄平面必须配置材质。");

            const string rtPath = GeneratedRoot + "/PinyinScoresRT.renderTexture";
            RenderTexture scoreTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            if (scoreTexture == null)
            {
                scoreTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
                {
                    name = "PinyinScoresRT",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    antiAliasing = 1,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                scoreTexture.Create();
                AssetDatabase.CreateAsset(scoreTexture, rtPath);
            }

            GameObject scoreObject = capturePlane.gameObject;
            scoreObject.layer = 31;
            PrefabUtility.RecordPrefabInstancePropertyModifications(scoreObject);
            Collider scoreCollider = scoreObject.GetComponent<Collider>();
            if (scoreCollider != null)
            {
                scoreCollider.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(scoreCollider);
            }
            MeshRenderer scoreRenderer = capturePlane;
            const string scoreMaterialPath = GeneratedRoot + "/HXIME_LM_ScoreCapture.mat";
            Material scoreMaterial = AssetDatabase.LoadAssetAtPath<Material>(scoreMaterialPath);
            if (scoreMaterial == null)
            {
                scoreMaterial = new Material(capturePlane.sharedMaterial) { name = "HXIME_LM_ScoreCapture" };
                AssetDatabase.CreateAsset(scoreMaterial, scoreMaterialPath);
            }
            else
            {
                scoreMaterial.CopyPropertiesFromMaterial(capturePlane.sharedMaterial);
            }
            scoreMaterial.SetFloat("_RawScore", 1f);
            scoreRenderer.sharedMaterial = scoreMaterial;
            EditorUtility.SetDirty(scoreRenderer);
            PrefabUtility.RecordPrefabInstancePropertyModifications(scoreRenderer);

            GameObject cameraObject = GameObject.Find("HXIME_LM_ScoreCamera");
            if (cameraObject == null) cameraObject = new GameObject("HXIME_LM_ScoreCamera");
            Camera scoreCamera = cameraObject.GetComponent<Camera>();
            if (scoreCamera == null) scoreCamera = cameraObject.AddComponent<Camera>();
            cameraObject.transform.SetPositionAndRotation(scoreObject.transform.position - scoreObject.transform.forward, scoreObject.transform.rotation);
            scoreCamera.enabled = true;
            scoreCamera.orthographic = true;
            scoreCamera.orthographicSize = 0.5f * capturePlane.transform.localScale.y;
            scoreCamera.aspect = 2f;
            scoreCamera.clearFlags = CameraClearFlags.SolidColor;
            scoreCamera.backgroundColor = Color.black;
            scoreCamera.nearClipPlane = 0.01f;
            scoreCamera.farClipPlane = 5f;
            scoreCamera.cullingMask = 1 << 31;
            scoreCamera.allowHDR = false;
            scoreCamera.allowMSAA = false;
            scoreCamera.targetTexture = scoreTexture;
            Camera mainCamera = Camera.main;
            if (mainCamera != null) mainCamera.cullingMask &= ~(1 << 31);

            SerializedObject lmSerialized = new SerializedObject(lm);
            lmSerialized.FindProperty("pinyinEngine").objectReferenceValue = engine;
            lmSerialized.FindProperty("previewRenderer").objectReferenceValue = previewRenderer;
            lmSerialized.FindProperty("scoreRenderer").objectReferenceValue = scoreRenderer;
            lmSerialized.FindProperty("scoreCamera").objectReferenceValue = scoreCamera;
            lmSerialized.FindProperty("scoreTexture").objectReferenceValue = scoreTexture;
            lmSerialized.FindProperty("imeUI").objectReferenceValue = ui;
            lmSerialized.FindProperty("readbackReceiver").objectReferenceValue = UdonSharpEditorUtility.GetBackingUdonBehaviour(lm);
            lmSerialized.FindProperty("simpTexA").objectReferenceValue = simp.texA;
            lmSerialized.FindProperty("simpTexB").objectReferenceValue = simp.texB;
            lmSerialized.FindProperty("simpFullTexA").objectReferenceValue = simp.fullTexA;
            lmSerialized.FindProperty("simpFullTexB").objectReferenceValue = simp.fullTexB;
            lmSerialized.FindProperty("simpMetaTex").objectReferenceValue = simp.metadata;
            lmSerialized.FindProperty("tradTexA").objectReferenceValue = trad.texA;
            lmSerialized.FindProperty("tradTexB").objectReferenceValue = trad.texB;
            lmSerialized.FindProperty("tradFullTexA").objectReferenceValue = trad.fullTexA;
            lmSerialized.FindProperty("tradFullTexB").objectReferenceValue = trad.fullTexB;
            lmSerialized.FindProperty("tradMetaTex").objectReferenceValue = trad.metadata;
            lmSerialized.ApplyModifiedPropertiesWithoutUndo();
            UdonSharpEditorUtility.CopyProxyToUdon(lm);
            scoreRenderer.enabled = true;

            if (scoreMaterial.HasProperty("_MetaTex")) scoreMaterial.SetTexture("_MetaTex", simp.metadata);
            if (previewRenderer != null && previewRenderer.sharedMaterial != null && previewRenderer.sharedMaterial.HasProperty("_MetaTex"))
                previewRenderer.sharedMaterial.SetTexture("_MetaTex", simp.metadata);

            Transform settingsButtons = root.transform.Find("SwitchBarHandle/SettingsPanel/Buttons");
            if (settingsButtons != null)
            {
                Transform oldButton = settingsButtons.Find("EngineMode");
                if (oldButton != null) UnityEngine.Object.DestroyImmediate(oldButton.gameObject);
                Transform template = settingsButtons.Find("KeyBoardPosReset");
                if (template == null) template = settingsButtons.GetChild(0);
                GameObject modeButtonObject = UnityEngine.Object.Instantiate(template.gameObject, settingsButtons);
                modeButtonObject.name = "EngineMode";
                Button modeButton = modeButtonObject.GetComponent<Button>();
                for (int i = modeButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                    UnityEventTools.RemovePersistentListener(modeButton.onClick, i);
                VRC.Udon.UdonBehaviour uiUdon = UdonSharpEditorUtility.GetBackingUdonBehaviour(ui);
                UnityEventTools.AddStringPersistentListener(modeButton.onClick, uiUdon.SendCustomEvent, "_ToggleEngineMode");
                TMP_Text modeLabel = modeButtonObject.GetComponentInChildren<TMP_Text>(true);
                if (modeLabel != null) modeLabel.text = "模式：传统";
                SerializedObject uiSerialized = new SerializedObject(ui);
                uiSerialized.FindProperty("lmEngine").objectReferenceValue = lm;
                uiSerialized.FindProperty("engineModeLabel").objectReferenceValue = modeLabel;
                uiSerialized.FindProperty("useLmMode").boolValue = false;
                uiSerialized.ApplyModifiedPropertiesWithoutUndo();
                UdonSharpEditorUtility.CopyProxyToUdon(ui);
            }

            EditorUtility.SetDirty(lm);
            EditorUtility.SetDirty(ui);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log($"HXIME LM 已接入当前场景：简体 {simp.count:N0} 条，繁体 {trad.count:N0} 条；RenderTexture {Width}x{Height} RFloat。");
        }
    }
}
#endif
