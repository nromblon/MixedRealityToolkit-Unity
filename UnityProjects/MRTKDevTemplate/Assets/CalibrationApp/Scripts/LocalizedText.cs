using TMPro;
using UnityEngine;

namespace CalibrationApp
{
    /// <summary>
    /// Drop this on any GameObject that has a TextMeshPro (UGUI or 3D) component to
    /// make its text follow the global <see cref="LanguageManager"/>.
    ///
    /// Fill in the English and Japanese strings in the inspector. Rich-text tags
    /// (e.g. &lt;size&gt;, &lt;alpha&gt;) are fully supported - just include them in
    /// both fields. If <see cref="english"/> is left blank, whatever text the TMP
    /// already contains at Awake is captured as the English baseline, so existing
    /// labels can be localised by only supplying the Japanese string.
    ///
    /// Japanese glyphs render through the JP Dynamic SDF font, which is registered
    /// as a global TMP fallback. Optionally assign <see cref="japaneseFont"/> to use
    /// that font directly while Japanese is active.
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("English text. Leave blank to capture the TMP's current text at startup.")]
        [TextArea(1, 4)]
        [SerializeField] private string english;

        [Tooltip("Japanese text shown when the language is Japanese.")]
        [TextArea(1, 4)]
        [SerializeField] private string japanese;

        [Tooltip("Optional font to apply while Japanese is active (e.g. JP Dynamic SDF). " +
                 "If left empty, the original font is kept and the global TMP fallback supplies glyphs.")]
        [SerializeField] private TMP_FontAsset japaneseFont;

        private TMP_Text label;
        private TMP_FontAsset originalFont;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            if (label == null)
            {
                Debug.LogWarning("[LocalizedText] No TMP_Text on " + name + "; component disabled.", this);
                enabled = false;
                return;
            }

            originalFont = label.font;
            if (string.IsNullOrEmpty(english))
                english = label.text;
        }

        private void OnEnable()
        {
            if (label == null) return;
            LanguageManager.Instance.LanguageChanged += Apply;
            Apply(LanguageManager.Instance.Current);
        }

        private void OnDisable()
        {
            // Instance may be torn down during shutdown; guard against that.
            if (LanguageManager.Instance != null)
                LanguageManager.Instance.LanguageChanged -= Apply;
        }

        private void Apply(AppLanguage language)
        {
            if (label == null) return;

            bool jp = language == AppLanguage.Japanese;
            label.text = jp ? japanese : english;

            if (japaneseFont != null)
                label.font = jp ? japaneseFont : originalFont;
        }

        /// <summary>Set the localized strings from code (useful for dynamic content).</summary>
        public void SetStrings(string englishText, string japaneseText)
        {
            english = englishText;
            japanese = japaneseText;
            if (isActiveAndEnabled)
                Apply(LanguageManager.Instance.Current);
        }
    }
}
