using System;
using UnityEngine;
using MixedReality.Toolkit;

namespace CalibrationApp
{
    public enum RecordingMode { LeftEye, RightEye, Binocular }

    /// <summary>
    /// Three toggle PressableButtons select the recording mode. The active button is
    /// highlighted via the MRTK3 toggle state. Fires OnModeChanged when the mode changes.
    /// </summary>
    public class ModeSelector : MonoBehaviour
    {
        [Header("Mode buttons (PressableButton roots)")]
        [SerializeField] private GameObject leftEyeButton;
        [SerializeField] private GameObject rightEyeButton;
        [SerializeField] private GameObject binocularButton;

        [SerializeField] private RecordingMode defaultMode = RecordingMode.LeftEye;

        public event Action<RecordingMode> OnModeChanged;
        public RecordingMode CurrentMode { get; private set; }

        void Start()
        {
            Hook(leftEyeButton, RecordingMode.LeftEye);
            Hook(rightEyeButton, RecordingMode.RightEye);
            Hook(binocularButton, RecordingMode.Binocular);
            SetMode(defaultMode);
        }

        private void Hook(GameObject buttonObj, RecordingMode mode)
        {
            if (buttonObj == null) return;
            var interactable = buttonObj.GetComponent<StatefulInteractable>();
            if (interactable == null) return;
            interactable.ToggleMode = StatefulInteractable.ToggleType.Toggle;
            interactable.OnClicked.AddListener(() => SetMode(mode));
        }

        public void SetMode(RecordingMode mode)
        {
            CurrentMode = mode;
            Highlight(leftEyeButton, mode == RecordingMode.LeftEye);
            Highlight(rightEyeButton, mode == RecordingMode.RightEye);
            Highlight(binocularButton, mode == RecordingMode.Binocular);
            OnModeChanged?.Invoke(mode);
        }

        private static void Highlight(GameObject buttonObj, bool active)
        {
            if (buttonObj == null) return;
            var interactable = buttonObj.GetComponent<StatefulInteractable>();
            if (interactable != null) interactable.ForceSetToggled(active, false);
        }
    }
}
