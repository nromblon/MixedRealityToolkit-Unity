using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;
using MixedReality.Toolkit;
using TMPro;

namespace CalibrationApp
{
    /// <summary>
    /// Writes the current calibration session to a timestamped JSON file in
    /// Application.persistentDataPath and shows a confirmation label.
    /// </summary>
    public class ExportController : MonoBehaviour
    {
        [SerializeField] private ModeSelector modeSelector;
        [SerializeField] private GameObject exportButton;
        [SerializeField] private TMP_Text confirmText;

        void Start()
        {
            if (exportButton == null) return;
            var interactable = exportButton.GetComponent<StatefulInteractable>();
            if (interactable != null) interactable.OnClicked.AddListener(Export);
        }

        public void Export()
        {
            RecordingMode mode = modeSelector != null ? modeSelector.CurrentMode : RecordingMode.LeftEye;
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = "frustum_session_" + timestamp + ".json";
            string path = Path.Combine(Application.persistentDataPath, fileName);

            CultureInfo ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"recordingMode\": \"" + mode + "\",");
            sb.AppendLine("  \"markerDistance\": " + CalibrationMarkers.MarkerDistance.ToString("0.0", ci) + ",");
            sb.AppendLine("  \"markers\": [");

            MarkerDef[] arr = CalibrationMarkers.All;
            for (int i = 0; i < arr.Length; i++)
            {
                MarkerDef m = arr[i];
                sb.Append("    { \"id\": " + m.id +
                          ", \"az\": " + m.az.ToString(ci) +
                          ", \"el\": " + m.el.ToString(ci) + " }");
                sb.AppendLine(i < arr.Length - 1 ? "," : "");
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");

            File.WriteAllText(path, sb.ToString());
            Debug.Log("[ExportController] Exported calibration session to: " + path);

            if (confirmText != null) confirmText.text = "Saved: " + path + fileName;
        }
    }
}
