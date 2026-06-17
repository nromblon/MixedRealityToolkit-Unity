using System;
using UnityEngine;

namespace CalibrationApp
{
    /// <summary>
    /// Supported UI languages.
    /// </summary>
    public enum AppLanguage
    {
        English = 0,
        Japanese = 1
    }

    /// <summary>
    /// Global, lazily-created holder for the current UI language.
    ///
    /// Nothing needs to place this in the scene: the first time any code touches
    /// <see cref="Instance"/> a hidden, DontDestroyOnLoad GameObject is created.
    /// The selected language is persisted in PlayerPrefs so it survives scene
    /// loads and app restarts.
    ///
    /// UI elements localise themselves by subscribing to <see cref="LanguageChanged"/>
    /// (see <see cref="LocalizedText"/>). Call <see cref="Toggle"/> to cycle between
    /// English and Japanese.
    /// </summary>
    public class LanguageManager : MonoBehaviour
    {
        private const string PrefKey = "CalibrationApp.Language";

        private static LanguageManager instance;

        /// <summary>
        /// The single, always-available manager instance. Created on demand.
        /// </summary>
        public static LanguageManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // Reuse one that may already exist in the scene.
                    instance = FindAnyObjectByType<LanguageManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("[LanguageManager]");
                        instance = go.AddComponent<LanguageManager>();
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private AppLanguage current = AppLanguage.English;

        /// <summary>
        /// Raised whenever the language changes. The argument is the new language.
        /// Fired immediately on subscribe is NOT automatic; new subscribers should
        /// read <see cref="Current"/> once to initialise themselves.
        /// </summary>
        public event Action<AppLanguage> LanguageChanged;

        /// <summary>
        /// The current UI language.
        /// </summary>
        public AppLanguage Current
        {
            get => current;
            set
            {
                if (current == value) return;
                current = value;
                PlayerPrefs.SetInt(PrefKey, (int)current);
                PlayerPrefs.Save();
                LanguageChanged?.Invoke(current);
            }
        }

        /// <summary>True when the current language is Japanese.</summary>
        public bool IsJapanese => current == AppLanguage.Japanese;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            current = (AppLanguage)PlayerPrefs.GetInt(PrefKey, (int)AppLanguage.English);
        }

        /// <summary>
        /// Switch between English and Japanese.
        /// </summary>
        public void Toggle()
        {
            Current = IsJapanese ? AppLanguage.English : AppLanguage.Japanese;
        }

        /// <summary>
        /// Explicitly set the language (e.g. from a dropdown).
        /// </summary>
        public void SetLanguage(AppLanguage language) => Current = language;
    }
}
