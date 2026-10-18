
using System;
using System.Linq;
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace HX2xianglong90.HXIME{
public class PinyinEngine : UdonSharpBehaviour
{
    // Segmenter 分词器
    private string[] pinyin_dict = {
            "a", "ai", "an", "ang", "ao",
            "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi", "bian", 
            "biao", "bie", "bin", "bing", "bo", "bu",
            "ca", "cai", "can", "cang", "cao", "ce", "cen", "ceng", "cha", "chai", 
            "chan", "chang", "chao", "che", "chen", "cheng", "chi", "chong", 
            "chou", "chu", "chua", "chuai", "chuan", "chuang", "chui", "chun", 
            "chuo", "ci", "cong", "cou", "cu", "cuan", "cui", "cun", "cuo",
            "da", "dai", "dan", "dang", "dao", "de", "dei", "den", "deng", "di", 
            "dian", "diao", "die", "ding", "diu", "dong", "dou", "du", "duan", 
            "dui", "dun", "duo",
            "e", "ei", "en", "er",
            "fa", "fan", "fang", "fei", "fen", "feng", "fo", "fou", "fu",
            "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", 
            "gong", "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", "guo",
            "ha", "hai", "han", "hang", "hao", "he", "hei", "hen", "heng", 
            "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun", "huo",
            "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", 
            "jiu", "ju", "juan", "jue", "jun",
            "ka", "kai", "kan", "kang", "kao", "ke", "kei", "ken", "keng", 
            "kong", "kou", "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo",
            "la", "lai", "lan", "lang", "lao", "le", "lei", "leng", "li", "lia", 
            "lian", "liang", "liao", "lie", "lin", "ling", "liu", "lo", "long", 
            "lou", "lu", "luan", "lue", "lun", "luo", "lv",
            "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", 
            "mi", "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu",
            "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", 
            "ni", "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", 
            "nou", "nu", "nuan", "nue", "nuo", "nv", "nve",
            "o", "ou",
            "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng", "pi", 
            "pian", "piao", "pie", "pin", "ping", "po", "pou", "pu",
            "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", 
            "qiong", "qiu", "qu", "quan", "que", "qun",
            "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", 
            "ru", "ruan", "rui", "run", "ruo",
            "sa", "sai", "san", "sang", "sao", "se", "sen", "seng", "sha", 
            "shai", "shan", "shang", "shao", "she", "shei", "shen", "sheng", 
            "shi", "shou", "shu", "shua", "shuai", "shuan", "shuang", "shui", 
            "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", "sun", "suo",
            "ta", "tai", "tan", "tang", "tao", "te", "teng", "ti", "tian", 
            "tiao", "tie", "ting", "tong", "tou", "tu", "tuan", "tui", "tun", "tuo",
            "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu",
            "xi", "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", 
            "xiu", "xu", "xuan", "xue", "xun",
            "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", 
            "yong", "you", "yu", "yuan", "yue", "yun",
            "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng", 
            "zha", "zhai", "zhan", "zhang", "zhao", "zhe", "zhei", "zhen", 
            "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai", "zhuan", 
            "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", 
            "zuan", "zui", "zun", "zuo"
    };
    private int max_pinyin_len = 6;
    private string[] short_pinyin_dict = {"g", "h", "m", "p", "b", "w", "k", "a", "e", "s", "x", "y", "c", "t", "l", "j", "d", "o", "n", "q", "f", "z", "r"};
    private string[] pinyin_syllable_h = {"hun", "huo", "hang", "hong", "heng", "huai", "he", "ha", "hen", "hua", "han", "hai", "huang", "hei", "hu", "hui", "hao", "huan", "hou"};
    private string[] pinyin_syllable_z = {"zhuai", "zhi", "ze", "zhuan", "zha", "zu", "zhong", "zhuang", "za", "zhen", "zei", "zhan", "zai", "zun", "zong", "zhe", "zheng", "zhu", "zhun", "zan", "zeng", "zhai", "zuan", "zou", "zhei", "zi", "zhou", "zhuo", "zang", "zhao", "zen", "zao", "zhua", "zui", "zhui", "zuo", "zhang"};
    private string[] pinyin_syllable_j = {"ju", "jia", "ji", "jie", "jiang", "jin", "juan", "jing", "jun", "jiao", "jiu", "jian", "jue", "jiong"};
    private string[] pinyin_syllable_p = {"pou", "pi", "po", "ping", "pian", "pan", "pang", "pin", "peng", "pa", "pai", "pao", "pei", "pen", "pu", "pie", "piao"};
    private string[] pinyin_syllable_s = {"sa", "san", "shen", "shou", "sha", "shuo", "sen", "shun", "sang", "sai", "su", "shao", "sun", "shu", "shai", "shuan", "sao", "shan", "shei", "shuang", "se", "song", "shui", "shuai", "si", "shang", "shi", "suan", "she", "suo", "sui", "sou", "seng", "shua", "sheng"};
    private string[] pinyin_syllable_q = {"qun", "qin", "quan", "que", "qie", "qi", "qia", "qing", "qiang", "qian", "qu", "qiu", "qiong", "qiao"};
    private string[] pinyin_syllable_c = {"chou", "chuai", "cai", "chang", "ce", "cen", "che", "chong", "chao", "chuo", "chu", "chen", "ceng", "cong", "chuang", "chuan", "chui", "chan", "cu", "cou", "chun", "cao", "cha", "cun", "chai", "cheng", "can", "chi", "cuan", "ca", "chua", "cang", "cui", "ci", "cuo"};
    private string[] pinyin_syllable_d = {"dang", "dun", "dou", "duo", "die", "den", "diu", "dian", "de", "dan", "da", "di", "duan", "dai", "dei", "deng", "du", "dao", "diao", "ding", "dui", "dong"};
    private string[] pinyin_syllable_t = {"tou", "ta", "tai", "tuo", "tan", "tang", "tian", "tun", "teng", "tu", "tao", "tong", "te", "tie", "tui", "ting", "tuan", "tiao", "ti"};
    private string[] pinyin_syllable_f = {"fou", "feng", "fan", "fa", "fu", "fei", "fen", "fo", "fang"};
    private string[] pinyin_syllable_g = {"ga", "gong", "gai", "guang", "gou", "gao", "guo", "gen", "gui", "geng", "ge", "gang", "gu", "gan", "guan", "gei", "gun", "gua", "guai"};
    private string[] pinyin_syllable_l = {"liu", "lou", "lo", "le", "lai", "liao", "long", "lang", "lian", "lao", "liang", "lu", "luo", "lv", "lan", "lie", "ling", "lia", "lun", "lin", "la", "luan", "lei", "li", "lue", "leng"};
    private string[] pinyin_syllable_x = {"xin", "xiang", "xing", "xu", "xia", "xun", "xian", "xi", "xuan", "xiao", "xiong", "xue", "xiu", "xie"};
    private string[] pinyin_syllable_o = {"o", "ou"};
    private string[] pinyin_syllable_m = {"miao", "me", "min", "mai", "mou", "mi", "ming", "miu", "ma", "meng", "men", "man", "mao", "mang", "mian", "mei", "mie", "mu", "mo"};
    private string[] pinyin_syllable_y = {"yue", "ye", "you", "yi", "yan", "ya", "ying", "yang", "yao", "yin", "yuan", "yong", "yu", "yo", "yun"};
    private string[] pinyin_syllable_b = {"ban", "ben", "bian", "bang", "beng", "bi", "bei", "bu", "bo", "bing", "biao", "bai", "bin", "bie", "ba", "bao"};
    private string[] pinyin_syllable_k = {"kao", "kong", "ka", "ken", "kun", "ku", "kai", "kuang", "kua", "kuo", "kei", "kan", "kang", "kou", "kui", "kuai", "keng", "ke", "kuan"};
    private string[] pinyin_syllable_w = {"wa", "wan", "wai", "wei", "weng", "wu", "wen", "wang", "wo"};
    private string[] pinyin_syllable_n = {"nuan", "ning", "nian", "neng", "nie", "nao", "nang", "nai", "nou", "nue", "niang", "niao", "nu", "nan", "ne", "nuo", "nve", "nin", "niu", "nong", "ni", "nei", "nen", "nv", "na"};
    private string[] pinyin_syllable_a = {"ai", "an", "ang", "a", "ao"};
    private string[] pinyin_syllable_r = {"rong", "rou", "reng", "rui", "ran", "rang", "ri", "ruan", "rao", "ruo", "re", "ru", "run", "ren"};
    private string[] pinyin_syllable_e = {"ei", "er", "en", "e"};
    // Candidate Matcher 匹配器
    // Dict Pool 拼音字典数据池
    [SerializeField] private PinyinDict[] dicts;
    private PinyinDict dict_pool;
    // 数组大小
    private int max_candidates;
    private string[] candidate_words; //长度为 maxcandidates
    private double[] candidate_weights; //长度为 maxcandidates

    //匹配分数数组
    private float[] match_scores; //长度为 maxcandidates
    // 创建拼音数组和对应的索引数组
    private string[] pinyins;
    private int[] indices;
    private void Start()
    {
        dict_pool = dicts[0];
        max_candidates = dict_pool.entries.Length;
        pinyins = dict_pool.pinyins;
        indices = dict_pool.indices;
    }
    public void SwitchSimp(){ // Excute when dict pool changed
        dict_pool = dicts[0];
        max_candidates = dict_pool.entries.Length;
        pinyins = dict_pool.pinyins;
        indices = dict_pool.indices;
    }
    public void SwitchTrad(){
        dict_pool = dicts[1];
        max_candidates = dict_pool.entries.Length;
        pinyins = dict_pool.pinyins;
        indices = dict_pool.indices;
    }
    private string[] Segment(string pinyinString,string mode="mixed"){
        string pinyin_str = pinyinString.ToLower();
        string[] result = new string[pinyin_str.Length];
        int resultCount = 0; // 当前有效元素数量
        int i = 0;
        int n = pinyin_str.Length;
        
        while (i < n){
            bool matched = false;
            
            // 1. 全拼匹配
            for (int l = Math.Min(max_pinyin_len, n - i); l > 0; l--)
            {
                string substr = pinyin_str.Substring(i, l);
                int idx = Array.IndexOf(pinyin_dict, substr);

                if (idx >= 0 && idx < pinyin_dict.Length && pinyin_dict[idx] == substr)
                {
                    result[resultCount++] = substr; // 直接存入数组
                    i += l;
                    matched = true;
                    break;
                }
            }
            // 2. 短拼匹配
            if (!matched && (mode == "short" || mode == "mixed"))
            {
                if(Array.IndexOf(short_pinyin_dict,pinyin_str[i])!=-1)
                {
                    string pinyinStrCase = pinyin_str[i].ToString();
                    string[] possiblePinyins;
                    switch (pinyinStrCase)
                    {
                        case "h":
                            possiblePinyins = pinyin_syllable_h;
                            break;
                        case "z":
                            possiblePinyins = pinyin_syllable_z;
                            break;
                        case "j":
                            possiblePinyins = pinyin_syllable_j;
                            break;
                        case "p":
                            possiblePinyins = pinyin_syllable_p;
                            break;
                        case "s":
                            possiblePinyins = pinyin_syllable_s;
                            break;
                        case "q":
                            possiblePinyins = pinyin_syllable_q;
                            break;
                        case "c":
                            possiblePinyins = pinyin_syllable_c;
                            break;
                        case "d":
                            possiblePinyins = pinyin_syllable_d;
                            break;
                        case "t":
                            possiblePinyins = pinyin_syllable_t;
                            break;
                        case "f":
                            possiblePinyins = pinyin_syllable_f;
                            break;
                        case "g":
                            possiblePinyins = pinyin_syllable_g;
                            break;
                        case "l":
                            possiblePinyins = pinyin_syllable_l;
                            break;
                        case "x":
                            possiblePinyins = pinyin_syllable_x;
                            break;
                        case "o":
                            possiblePinyins = pinyin_syllable_o;
                            break;
                        case "m":
                            possiblePinyins = pinyin_syllable_m;
                            break;
                        case "y":
                            possiblePinyins = pinyin_syllable_y;
                            break;
                        case "b":
                            possiblePinyins = pinyin_syllable_b;
                            break;
                        case "k":
                            possiblePinyins = pinyin_syllable_k;
                            break;
                        case "w":
                            possiblePinyins = pinyin_syllable_w;
                            break;
                        case "n":
                            possiblePinyins = pinyin_syllable_n;
                            break;
                        case "a":
                            possiblePinyins = pinyin_syllable_a;
                            break;
                        case "r":
                            possiblePinyins = pinyin_syllable_r;
                            break;
                        case "e":
                            possiblePinyins = pinyin_syllable_e;
                            break;
                        default:
                            // 如果有未匹配的情况，可以在这里处理
                            possiblePinyins = null;
                            break;
                    }

                    if (possiblePinyins != null && i + 1 < n) // At least 2 characters needed
                    {
                        // Find maximum length of possible pinyins without using LINQ
                        int maxPossibleLen = 0;
                        foreach (string py in possiblePinyins)
                        {
                            if (py.Length > maxPossibleLen)
                            {
                                maxPossibleLen = py.Length;
                            }
                        }
                        
                        // Determine the maximum length we can check
                        int maxLengthToCheck = Math.Min(maxPossibleLen, n - i);
                        
                        // Check from longest to shortest possible matches
                        for (int l = maxLengthToCheck; l > 1; l--)
                        {
                            string substr = pinyin_str.Substring(i, l);
                            
                            // Perform binary search
                            int idx = Array.IndexOf(possiblePinyins, substr);
                            
                            // Verify the match
                            if (idx >= 0 && idx < possiblePinyins.Length && possiblePinyins[idx] == substr)
                            {
                                result[resultCount++] = pinyin_str[i].ToString();
                                i += l;
                                matched = true;
                                break;
                            }
                        }
                    }

                    if (!matched)
                    {
                        result[resultCount++] = pinyin_str[i].ToString();
                        i += 1;
                        matched = true;
                    }
                }
            }
            
            // 3. 都不匹配，直接取当前字符
            if (!matched)
            {
                result[resultCount++] = pinyin_str[i].ToString();
                i += 1;
            }
        }
        // 返回有效部分（去除未使用的空间）
        string[] finalResult = new string[resultCount];
        Array.Copy(result, finalResult, resultCount);
        return finalResult;
    }

    //匹配器

    private string MapSingleUlpbToPinyin(string ulpb)
    {
        if (ulpb.Length == 0)
        {
            return "";
        }
        switch (ulpb)
        {
            case "aa": return "a";
            case "oo": return "o";
        }
        var result = new StringBuilder();
        switch (ulpb[0])
        {
            case 'u': result.Append("sh"); break;
            case 'i': result.Append("ch"); break;
            case 'v': result.Append("zh"); break;
            default: result.Append(ulpb[0]); break;
        }
        if (ulpb.Length == 1)
        {
            return result.ToString();
        }

        var pending1 = "";
        var pending2 = "";
        switch (ulpb[1])
        {
            case 'q': result.Append("iu"); break;
            case 'w': result.Append("ei"); break;
            case 'e': result.Append("e"); break;
            case 'r': result.Append("uan"); break;
            case 't': result.Append("ue"); break;
            case 'y': result.Append("un"); break;
            case 'u': result.Append("u"); break;
            case 'i': result.Append("i"); break;
            case 'p': result.Append("ie"); break;
            case 'a': result.Append("a"); break;
            case 'd': result.Append("ai"); break;
            case 'f': result.Append("en"); break;
            case 'g': result.Append("eng"); break;
            case 'h': result.Append("ang"); break;
            case 'j': result.Append("an"); break;
            case 'z': result.Append("ou"); break;
            case 'c': result.Append("ao"); break;
            case 'b': result.Append("in"); break;
            case 'n': result.Append("iao"); break;
            case 'm': result.Append("ian"); break;

            case 'o': 
                pending1 = "o";
                pending2 = "uo";
                break;
            case 's': 
                pending1 = "ong";
                pending2 = "iong";
                break;
            case 'k': 
                pending1 = "uai";
                pending2 = "ing";
                break;
            case 'l': 
                pending1 = "uang";
                pending2 = "iang";
                break;
            case 'x': 
                pending1 = "ua";
                pending2 = "ia";
                break;
            case 'v': 
                pending1 = "ui";
                pending2 = "v";
                break;
        }
        
        if (pending1 == "") return result.ToString();
        if (Array.IndexOf(pinyin_dict, result.ToString() + pending1) != -1)
        {
            result.Append(pending1);
        }
        else
        {
            result.Append(pending2);
        }
        return result.ToString();
    }

    private string MapUlpbToPinyin(string ulpb)
    {
        var ori = ulpb.Split(' ');
        var result = new string[ori.Length];
        for (int i = 0; i < ori.Length; i++)
        {
            result[i] = MapSingleUlpbToPinyin(ori[i]);
        }
        return string.Join(" ", result);
    }

private int _match_pinyin(string input_pinyin, string dict_pinyin)
{
    // 1. 完全匹配（100分）
    if (input_pinyin == dict_pinyin)
    {
        return 100;
    }

    // 2. 开头匹配（50分）
    if (StartsWithCheck(input_pinyin, dict_pinyin))
    {
        return 50;
    }

    // 3. 简拼匹配（30分）
    if (InitialsMatch(input_pinyin, dict_pinyin))
    {
        return 30;
    }

    // 4. 不匹配
    return 0;
}

// 封装函数：检查开头匹配
private bool StartsWithCheck(string a, string b)
{
    return b.StartsWith(a) || a.StartsWith(b);
}

// 封装函数：简拼匹配检查
private bool InitialsMatch(string input, string dict)
{
    return GetInitials(input) == GetInitials(dict);
}

// 封装函数：获取拼音首字母
private string GetInitials(string pinyin)
{
    string[] syllables = pinyin.Split(' ');
    StringBuilder initials = new StringBuilder();
    
    foreach (string s in syllables)
    {
        if (s.Length > 0)
        {
            initials.Append(s[0]);
        }
    }
    
    return initials.ToString();
}

    private int[] _find_exact_matches(string input_pinyin)
    {

        // 找到第一个匹配的位置
        int left = Array.IndexOf(pinyins,input_pinyin);

        // 如果没有找到匹配项，返回空数组
        if (left < 0 || pinyins[left] != input_pinyin)
        {
            return new int[0];
        }

        // 找到最后一个匹配的位置
        int right = Array.LastIndexOf(pinyins,input_pinyin)+1;
        
        // 返回所有匹配项的索引
        Debug.Log(left);
        Debug.Log(right);
        // 使用Array.Copy来获取子数组
        int[] result = new int[right - left];
        Array.Copy(indices, left, result, 0, right - left);
        return result;
    }
private int[] _find_fuzzy_matches(string inputPinyin, int startIndex, int endIndex)
{
    // 预分配足够大的数组空间
    int[] matches = new int[endIndex - startIndex];
    int matchCount = 0;
    
    for (int i = startIndex; i < endIndex; i++)
    {
        if (matchCount >= matches.Length) break;
        
        string pinyin = pinyins[i];
        int idx = indices[i];
        
        int score = _match_pinyin(inputPinyin, pinyin);
        if (score > 0)
        {
            matches[matchCount++] = idx;
        }
    }
    
    // 返回实际匹配的部分
    int[] result = new int[matchCount];
    Array.Copy(matches, result, matchCount);
    return result;
}

public string[] Match(string inputPinyin, int limit = 20,bool accurateMode = false, bool ulpb = false)
{
    Debug.Log($"Match {inputPinyin}");
    // 初始化候选数组
    string[] finalCandidates = new string[limit];
    double[] finalWeights = new double[limit];
    int finalCount = 0;

    // 临时存储所有候选（假设最多max_candidates个）
    string[] allCandidates = new string[max_candidates];
    double[] allWeights = new double[max_candidates];
    int totalCount = 0;

    if (ulpb)
    {
        inputPinyin = MapUlpbToPinyin(inputPinyin);
    }

    // 阶段1：精确匹配
    int[] exactMatches = _find_exact_matches(inputPinyin);
    for (int i = 0; i < exactMatches.Length && totalCount < max_candidates; i++)
    {
        int idx = exactMatches[i];
        allCandidates[totalCount] = dict_pool.entries[idx];
        allWeights[totalCount] = Math.Log10(dict_pool.weights[idx] + 1) + 100 * 1000000;
        totalCount++;
    }
    if (!accurateMode){
    // 阶段2：模糊匹配
    if (totalCount < limit && inputPinyin.Length > 0)
    {
        char firstChar = inputPinyin[0];
        for (int i = 0; i < pinyins.Length && totalCount < max_candidates; i++)
        {
            if (pinyins[i].Length > 0 && pinyins[i][0] == firstChar)
            {
                int score = _match_pinyin(inputPinyin, pinyins[i]);
                if (score > 0)
                {
                    // 检查是否已存在于精确匹配中
                    bool exists = false;
                    for (int j = 0; j < exactMatches.Length; j++)
                    {
                        if (indices[i] == exactMatches[j])
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        allCandidates[totalCount] = dict_pool.entries[indices[i]];
                        allWeights[totalCount] = Math.Log10(dict_pool.weights[indices[i]] + 1) + score * 1000000;
                        totalCount++;
                    }
                }
            }
        }
    }
    }
    // 筛选TopN结果（使用插入排序思想）
    for (int i = 0; i < totalCount; i++)
    {
        // 如果结果数组未满，直接添加
        if (finalCount < limit)
        {
            // 找到插入位置
            int insertPos = finalCount;
            while (insertPos > 0 && allWeights[i] > finalWeights[insertPos - 1])
            {
                insertPos--;
            }

            // 移动元素腾出位置
            for (int j = finalCount; j > insertPos; j--)
            {
                if (j < limit)
                {
                    finalCandidates[j] = finalCandidates[j - 1];
                    finalWeights[j] = finalWeights[j - 1];
                }
            }

            // 插入新元素
            if (insertPos < limit)
            {
                finalCandidates[insertPos] = allCandidates[i];
                finalWeights[insertPos] = allWeights[i];
                finalCount++;
            }
        }
        else
        {
            // 结果数组已满，只替换比当前最小权重大的元素
            if (allWeights[i] > finalWeights[limit - 1])
            {
                // 找到插入位置
                int insertPos = limit - 1;
                while (insertPos > 0 && allWeights[i] > finalWeights[insertPos - 1])
                {
                    insertPos--;
                }

                // 移动元素腾出位置
                for (int j = limit - 1; j > insertPos; j--)
                {
                    finalCandidates[j] = finalCandidates[j - 1];
                    finalWeights[j] = finalWeights[j - 1];
                }

                // 插入新元素
                finalCandidates[insertPos] = allCandidates[i];
                finalWeights[insertPos] = allWeights[i];
            }
        }
    }

    // 返回结果（去除可能的空位）
    string[] result = new string[finalCount];
    for (int i = 0; i < finalCount; i++)
    {
        result[i] = finalCandidates[i];
    }
    return result;
}
}
}