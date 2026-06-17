using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

namespace CalibrationApp
{
    /// <summary>
    /// Put this on an MRTK3 action button (PressableButton, ToggleMode = Button) to
    /// turn it into an English/Japanese language switch.
    ///
    /// Each press cycles the global <see cref="LanguageManager"/> between English and
    /// Japanese. The button's own caption always shows the language that is currently
    /// active, written in that language (e.g. "Language / English" -> "言語 / 日本語").
    ///
    /// Wiring is done at runtime via PressableButton.OnClicked (the same pattern used
    /// elsewhere in this project), so no serialized UnityEvent setup is required.
    /// </summary>
    [RequireComponent(typeof(PressableButton))]
    public class LanguageToggleButton : MonoBehaviour
    {
        [Tooltip("Caption to update. If left empty, 'Frontplate/AnimatedContent/Text' " +
                 "is resolved automatically.")]
        [SerializeField] private TMP_Text caption;

        [Tooltip("Optional font for the caption (e.g. JP Dynamic SDF) so Japanese " +
                 "renders crisply. If empty, the existing font + global fallback is used.")]
        [SerializeField] private TMP_FontAsset captionFont;

        private PressableButton button;

        private void Awake()
        {
            button = GetComponent<PressableButton>();
            if (caption == null)
            {
                var t = transform.Find("Frontplate/AnimatedContent/Text");
                if (t != null) caption = t.GetComponent<TMP_Text>();
                if (caption == null) caption = GetComponentInChildren<TMP_Text>(true);
            }
            if (caption != null && captionFont != null)
                caption.font = captionFont;
        }

        private void OnEnable()
        {
            if (button != null) button.OnClicked.AddListener(OnPressed);
            LanguageManager.Instance.LanguageChanged += OnLanguageChanged;
            Refresh(LanguageManager.Instance.Current);
        }

        private void OnDisable()
        {
            if (button != null) button.OnClicked.RemoveListener(OnPressed);
            if (LanguageManager.Instance != null)
                LanguageManager.Instance.LanguageChanged -= OnLanguageChanged;
        }

        private void OnPressed() => LanguageManager.Instance.Toggle();

        private void OnLanguageChanged(AppLanguage language) => Refresh(language);

        private void Refresh(AppLanguage language)
        {
            if (caption == null) return;

            // Title localises too; subtitle shows the active language's native name.
            string title = language == AppLanguage.Japanese ? "言語" : "Language";
            string nativeName = language == AppLanguage.Japanese ? "日本語" : "English";

            caption.text = $"<size=8>{title}</size>\n<size=6><alpha=#88>{nativeName}</size>";
        }
    }
}
