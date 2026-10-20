#if UNITY_EDITOR
using UnityEngine;

namespace HX2xianglong90.HXIME
{
    [AddComponentMenu("HXIME/LM Dictionary Setup")]
    [DisallowMultipleComponent]
    public sealed class PinyinEncoder : MonoBehaviour
    {
        public TextAsset simplifiedDictionary;
        public TextAsset traditionalDictionary;
        public Renderer capturePlaneRenderer;
        public Renderer previewRenderer;

        private void OnValidate()
        {
            if (gameObject.tag == "EditorOnly" && gameObject.GetComponent(typeof(VRC.Udon.UdonBehaviour)) != null)
                gameObject.tag = "Untagged";
        }
    }
}
#endif
