
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Rendering;
using VRC.Udon;

namespace HX2xianglong90.HXIME
{
public class PinyinLM : UdonSharpBehaviour
{
    private const int ScoreStride = 5000000;

    [Header("GPU 输入 / 输出")]
    [SerializeField] private PinyinEngine pinyinEngine;
    [SerializeField] private Renderer previewRenderer;
    [SerializeField] private Renderer scoreRenderer;
    [SerializeField] private Camera scoreCamera;
    [SerializeField] private RenderTexture scoreTexture;
    [SerializeField] private HXIMEUI imeUI;
    [SerializeField] private UdonBehaviour readbackReceiver;
    [SerializeField, Range(0, ScoreStride - 1), Tooltip("低于该词频的匹配不参与候选排序，并在 shader 输出中置零。默认 0 保留完整词库。")]
    private int minimumBaseWeight;

    [Header("简体数据")]
    [SerializeField] private Texture2D simpTexA;
    [SerializeField] private Texture2D simpTexB;
    [SerializeField] private Texture2D simpFullTexA;
    [SerializeField] private Texture2D simpFullTexB;
    [SerializeField] private Texture2D simpMetaTex;
    [Header("繁体数据")]
    [SerializeField] private Texture2D tradTexA;
    [SerializeField] private Texture2D tradTexB;
    [SerializeField] private Texture2D tradFullTexA;
    [SerializeField] private Texture2D tradFullTexB;
    [SerializeField] private Texture2D tradMetaTex;

    private Material previewMaterial;
    private Material scoreMaterial;
    private PinyinDict activeDictionary;
    private bool renderersInitialized;
    private float[] readbackData;
    private int[] bucketCounts;
    private int[] bucketIndices;
    private int resultLimit;
    private int inputRevision;
    private int requestRevision;
    private bool readbackInFlight;
    private bool readbackScheduled;
    private bool queryDirty;
    private string pendingInput = "";
    private bool pendingAccurateMode;

    private void Start()
    {
        InitializeRenderers();
    }

    private void InitializeRenderers()
    {
        if (renderersInitialized) return;
        if (previewRenderer != null) previewMaterial = previewRenderer.material;
        if (scoreRenderer != null) scoreMaterial = scoreRenderer.material;
        if (scoreCamera != null){
            if (scoreTexture != null) scoreCamera.targetTexture = scoreTexture;
            scoreCamera.enabled = true;
        }
        if (scoreRenderer != null) scoreRenderer.enabled = true;
        if (scoreMaterial != null) scoreMaterial.SetFloat("_RawScore", 1f);
        if (previewMaterial != null) previewMaterial.SetFloat("_RawScore", 0f);
        ApplyMinimumWeight();
        renderersInitialized = true;
    }

    private void ApplyMinimumWeight()
    {
        float threshold = Mathf.Max(0, minimumBaseWeight);
        if (previewMaterial != null) previewMaterial.SetFloat("_MinBaseWeight", threshold);
        if (scoreMaterial != null) scoreMaterial.SetFloat("_MinBaseWeight", threshold);
    }

    private void EnsureReadbackBuffers(int limit)
    {
        if (scoreTexture == null) return;
        int pixelCount = scoreTexture.width * scoreTexture.height;
        if (readbackData == null || readbackData.Length != pixelCount)
            readbackData = new float[pixelCount];
        if (bucketCounts == null || bucketIndices == null || bucketIndices.Length < 100 * limit)
        {
            bucketCounts = new int[100];
            bucketIndices = new int[100 * limit];
        }
    }

    public void _SetDictionary(bool simplified, PinyinDict dictionary)
    {
        InitializeRenderers();
        activeDictionary = dictionary;
        Texture2D texA = simplified ? simpTexA : tradTexA;
        Texture2D texB = simplified ? simpTexB : tradTexB;
        Texture2D fullTexA = simplified ? simpFullTexA : tradFullTexA;
        Texture2D fullTexB = simplified ? simpFullTexB : tradFullTexB;
        Texture2D meta = simplified ? simpMetaTex : tradMetaTex;
        if (previewMaterial != null)
        {
            previewMaterial.SetTexture("_PinyinTexA", texA);
            previewMaterial.SetTexture("_PinyinTexB", texB);
            previewMaterial.SetTexture("_PinyinFullTexA", fullTexA);
            previewMaterial.SetTexture("_PinyinFullTexB", fullTexB);
            previewMaterial.SetTexture("_MetaTex", meta);
        }
        if (scoreMaterial != null)
        {
            scoreMaterial.SetTexture("_PinyinTexA", texA);
            scoreMaterial.SetTexture("_PinyinTexB", texB);
            scoreMaterial.SetTexture("_PinyinFullTexA", fullTexA);
            scoreMaterial.SetTexture("_PinyinFullTexB", fullTexB);
            scoreMaterial.SetTexture("_MetaTex", meta);
        }
        ApplyMinimumWeight();
    }

    public void _SubmitInput(string input, int limit, bool accurateMode)
    {
        InitializeRenderers();
        if (pinyinEngine == null) return;
        if (activeDictionary == null) activeDictionary = pinyinEngine._GetCurrentDictionary();
        pendingInput = input == null ? "" : input;
        pendingAccurateMode = accurateMode;
        resultLimit = Mathf.Clamp(limit, 1, 100);
        inputRevision++;
        if (pendingInput.Trim().Length == 0){
            queryDirty = false;
            if (imeUI != null) imeUI._OnLMCandidatesReady(new string[0]);
            return;
        }
        if (accurateMode){
            queryDirty = false;
            if (imeUI != null) imeUI._OnLMCandidatesReady(_Match(pendingInput, resultLimit, true));
            return;
        }
        if (scoreTexture == null || scoreCamera == null || scoreMaterial == null || imeUI == null || readbackReceiver == null)
        {
            queryDirty = false;
            imeUI._OnLMCandidatesReady(_Match(pendingInput, resultLimit, false));
            return;
        }

        queryDirty = true;
        string initials = pinyinEngine._GetInitialsForInput(pendingInput);
        if (initials.Length > 8) initials = initials.Substring(0, 8);
        Vector4 first = Vector4.zero;
        Vector4 second = Vector4.zero;
        for (int i = 0; i < initials.Length; i++)
        {
            int id = initials[i] - 'a' + 1;
            if (i == 0) first.x = id;
            else if (i == 1) first.y = id;
            else if (i == 2) first.z = id;
            else if (i == 3) first.w = id;
            else if (i == 4) second.x = id;
            else if (i == 5) second.y = id;
            else if (i == 6) second.z = id;
            else second.w = id;
        }
        SetInputOnMaterial(previewMaterial, first, second, initials.Length);
        SetInputOnMaterial(scoreMaterial, first, second, initials.Length);
        string fullPinyin = pinyinEngine._GetNormalizedFullPinyin(pendingInput);
        if (fullPinyin.Length > 8) fullPinyin = "";
        Vector4 fullFirst = Vector4.zero;
        Vector4 fullSecond = Vector4.zero;
        for (int i = 0; i < fullPinyin.Length; i++)
        {
            int id = fullPinyin[i] - 'a' + 1;
            if (i == 0) fullFirst.x = id;
            else if (i == 1) fullFirst.y = id;
            else if (i == 2) fullFirst.z = id;
            else if (i == 3) fullFirst.w = id;
            else if (i == 4) fullSecond.x = id;
            else if (i == 5) fullSecond.y = id;
            else if (i == 6) fullSecond.z = id;
            else fullSecond.w = id;
        }
        SetFullInputOnMaterial(previewMaterial, fullFirst, fullSecond, fullPinyin.Length);
        SetFullInputOnMaterial(scoreMaterial, fullFirst, fullSecond, fullPinyin.Length);
        EnsureReadbackBuffers(resultLimit);
        _ScheduleGpuReadback();
    }

    private void _ScheduleGpuReadback()
    {
        if (readbackScheduled || readbackInFlight || !queryDirty) return;
        readbackScheduled = true;
        SendCustomEventDelayedSeconds(nameof(_RequestGpuReadback), 0.05f);
    }

    public void _RequestGpuReadback()
    {
        readbackScheduled = false;
        if (readbackInFlight || !queryDirty || pendingAccurateMode || scoreCamera == null || scoreTexture == null) return;
        queryDirty = false;
        requestRevision = inputRevision;
        scoreCamera.Render();
        readbackInFlight = true;
        VRCAsyncGPUReadback.Request(scoreTexture, 0, TextureFormat.RFloat, readbackReceiver);
    }

    public override void OnAsyncGpuReadbackComplete(VRCAsyncGPUReadbackRequest request)
    {
        readbackInFlight = false;
        if (request.hasError || readbackData == null || !request.TryGetData(readbackData))
        {
            if (imeUI != null) imeUI._OnLMCandidatesReady(_Match(pendingInput, resultLimit, pendingAccurateMode));
            if (queryDirty) _ScheduleGpuReadback();
            return;
        }

        if (requestRevision != inputRevision)
        {
            if (pendingInput.Trim().Length > 0 && !pendingAccurateMode){
                queryDirty = true;
                _ScheduleGpuReadback();
            }
            return;
        }

        string[] candidates = CollectBucketedCandidates();
        if (imeUI != null) imeUI._OnLMCandidatesReady(candidates);
    }

    private string[] CollectBucketedCandidates()
    {
        if (activeDictionary == null || activeDictionary.entries == null) return new string[0];
        for (int i = 0; i < bucketCounts.Length; i++) bucketCounts[i] = 0;

        int pixelCount = readbackData.Length;
        if (pixelCount > activeDictionary.entries.Length) pixelCount = activeDictionary.entries.Length;
        for (int index = 0; index < pixelCount; index++)
        {
            int bucket = Mathf.RoundToInt(Mathf.Clamp01(readbackData[index]) * 100f);
            if (bucket <= 0 || bucket > 100) continue;
            int bucketOffset = (bucket - 1) * resultLimit;
            int count = bucketCounts[bucket - 1];
            if (count >= resultLimit) continue;
            bucketIndices[bucketOffset + count] = index;
            bucketCounts[bucket - 1] = count + 1;
        }

        int resultCount = 0;
        for (int bucket = 99; bucket >= 0 && resultCount < resultLimit; bucket--)
            resultCount += Mathf.Min(bucketCounts[bucket], resultLimit - resultCount);

        string[] results = new string[resultCount];
        int outputIndex = 0;
        for (int bucket = 99; bucket >= 0 && outputIndex < resultCount; bucket--)
        {
            int count = Mathf.Min(bucketCounts[bucket], resultCount - outputIndex);
            int bucketOffset = bucket * resultLimit;
            for (int i = 0; i < count; i++)
            {
                int entryIndex = bucketIndices[bucketOffset + i];
                results[outputIndex++] = activeDictionary.entries[entryIndex];
            }
        }
        return results;
    }

    private void SetInputOnMaterial(Material material, Vector4 first, Vector4 second, int length)
    {
        if (material == null) return;
        material.SetVector("_InputCharsA", first);
        material.SetVector("_InputCharsB", second);
        material.SetFloat("_InputLen", length);
    }

    private void SetFullInputOnMaterial(Material material, Vector4 first, Vector4 second, int length)
    {
        if (material == null) return;
        material.SetVector("_FullInputCharsA", first);
        material.SetVector("_FullInputCharsB", second);
        material.SetFloat("_FullInputLen", length);
    }

    public string[] _Match(string input, int limit, bool accurateMode)
    {
        if (activeDictionary == null || activeDictionary.pinyins == null || activeDictionary.entries == null || limit <= 0)
            return new string[0];

        string query = input.Trim().ToLower();
        if (query.Length == 0) return new string[0];
        string queryInitials = pinyinEngine == null ? query : pinyinEngine._GetInitialsForInput(query);
        if (queryInitials.Length > 8) queryInitials = queryInitials.Substring(0, 8);
        string fullQuery = pinyinEngine == null ? "" : pinyinEngine._GetNormalizedFullPinyin(query);
        if (fullQuery.Length > 8) fullQuery = "";

        string[] bestWords = new string[limit];
        int[] bestScores = new int[limit];
        int bestCount = 0;
        int count = activeDictionary.pinyins.Length;
        if (count > activeDictionary.entries.Length) count = activeDictionary.entries.Length;

        for (int i = 0; i < count; i++)
        {
            string pinyin = activeDictionary.pinyins[i];
            if (string.IsNullOrEmpty(pinyin) || pinyin[0] != query[0]) continue;
            int entryIndex = i;
            if (activeDictionary.indices != null && i < activeDictionary.indices.Length)
                entryIndex = activeDictionary.indices[i];
            if (entryIndex < 0 || entryIndex >= activeDictionary.entries.Length) continue;
            int weight = 0;
            if (activeDictionary.weights != null && entryIndex < activeDictionary.weights.Length)
                weight = Mathf.Clamp(activeDictionary.weights[entryIndex], 0, ScoreStride - 1);
            if (weight < minimumBaseWeight) continue;

            string candidatePinyin = pinyin;
            int tier = -1;
            string normalizedCandidate = candidatePinyin.Replace(" ", "").Replace("\t", "");
            if (fullQuery.Length > 0 && fullQuery == normalizedCandidate) tier = 4;
            else if (fullQuery.Length > 0 && normalizedCandidate.StartsWith(fullQuery)) tier = 3;
            else if (!accurateMode) tier = GetInitialMatchTier(queryInitials, candidatePinyin);
            if (tier < 0) continue;

            int score = tier * ScoreStride + weight + 1;
            string word = activeDictionary.entries[entryIndex];

            if (bestCount < limit)
            {
                int child = bestCount++;
                while (child > 0)
                {
                    int parent = (child - 1) / 2;
                    if (bestScores[parent] <= score) break;
                    bestScores[child] = bestScores[parent];
                    bestWords[child] = bestWords[parent];
                    child = parent;
                }
                bestScores[child] = score;
                bestWords[child] = word;
            }
            else if (score > bestScores[0])
            {
                int parent = 0;
                while (true)
                {
                    int child = parent * 2 + 1;
                    if (child >= bestCount) break;
                    if (child + 1 < bestCount && bestScores[child + 1] < bestScores[child]) child++;
                    if (bestScores[child] >= score) break;
                    bestScores[parent] = bestScores[child];
                    bestWords[parent] = bestWords[child];
                    parent = child;
                }
                bestScores[parent] = score;
                bestWords[parent] = word;
            }
        }

        string[] result = new string[bestCount];
        for (int resultIndex = bestCount - 1; resultIndex >= 0; resultIndex--)
        {
            result[resultIndex] = bestWords[0];
            bestCount--;
            if (bestCount == 0) break;

            int score = bestScores[bestCount];
            string word = bestWords[bestCount];
            int parent = 0;
            while (true)
            {
                int child = parent * 2 + 1;
                if (child >= bestCount) break;
                if (child + 1 < bestCount && bestScores[child + 1] < bestScores[child]) child++;
                if (bestScores[child] >= score) break;
                bestScores[parent] = bestScores[child];
                bestWords[parent] = bestWords[child];
                parent = child;
            }
            bestScores[parent] = score;
            bestWords[parent] = word;
        }
        return result;
    }

    private int GetInitialMatchTier(string input, string pinyin)
    {
        if (input.Length == 0 || input.Length > 8) return -1;
        int inputIndex = 0;
        int syllableIndex = 0;
        bool startsWithInput = true;
        bool atSyllableStart = true;
        for (int i = 0; i < pinyin.Length; i++)
        {
            char c = pinyin[i];
            if (c == ' ' || c == '\t')
            {
                atSyllableStart = true;
            }
            else if (atSyllableStart)
            {
                if (syllableIndex < input.Length && pinyin[i] != input[syllableIndex])
                    startsWithInput = false;
                if (inputIndex < input.Length && input[inputIndex] == c) inputIndex++;
                syllableIndex++;
                atSyllableStart = false;
            }
        }
        if (inputIndex < input.Length) return -1;
        if (!startsWithInput) return 0;
        return syllableIndex == input.Length ? 2 : 1;
    }
}
}