using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using MixedReality.Toolkit;

namespace CalibrationApp
{
    /// <summary>
    /// Main scene flow: shows the instructions canvas, dismisses it to reveal the
    /// navigation canvas, and loads the two calibration scenes on button press.
    /// Button clicks are wired at runtime via MRTK3 StatefulInteractable.OnClicked.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Canvases")]
        [SerializeField] private GameObject instructionsCanvas;
        [SerializeField] private GameObject navigationCanvas;

        [Header("Buttons (PressableButton roots)")]
        [SerializeField] private GameObject dismissButton;
        [SerializeField] private GameObject sensorOffsetButton;
        [SerializeField] private GameObject recordingFrustumButton;
        [SerializeField] private GameObject gaze360TestButton;

        [Header("Scene names (must be in Build Settings)")]
        [SerializeField] private string sensorOffsetScene = "CalibSensorOffset";
        [SerializeField] private string recordingFrustumScene = "CalibRecordingFrustum";
        [SerializeField] private string gaze360TestScene = "PL_360";

        void Start()
        {
            if (instructionsCanvas != null) instructionsCanvas.SetActive(true);
            if (navigationCanvas != null) navigationCanvas.SetActive(false);

            Hook(dismissButton, DismissInstructions);
            Hook(sensorOffsetButton, () => SceneManager.LoadScene(sensorOffsetScene));
            Hook(recordingFrustumButton, () => SceneManager.LoadScene(recordingFrustumScene));
            Hook(gaze360TestButton, () => SceneManager.LoadScene(gaze360TestScene));
        }

        private static void Hook(GameObject buttonObj, UnityAction action)
        {
            if (buttonObj == null) return;
            var interactable = buttonObj.GetComponent<StatefulInteractable>();
            if (interactable != null) interactable.OnClicked.AddListener(action);
        }

        public void DismissInstructions()
        {
            if (instructionsCanvas != null) instructionsCanvas.SetActive(false);
            if (navigationCanvas != null) navigationCanvas.SetActive(true);
        }
    }
}
