using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

namespace CalibrationApp
{
    /// <summary>
    /// Rebuilds the 13 calibration marker dots under MarkerRig whenever the recording
    /// mode changes. Markers are placed at fixed (az, el) offsets relative to the
    /// selected recording camera position, at a fixed distance.
    /// </summary>
    public class MarkerSpawner : MonoBehaviour
    {
        [SerializeField] private ModeSelector modeSelector;
        [SerializeField] private Transform markerRig;
        [SerializeField] private GameObject markerPrefab;
        [Tooltip("XROrigin Camera Offset transform, used to convert XR eye-node local positions to world space.")]
        [SerializeField] private Transform cameraOffset;

        /// <summary>Quest 3 fixed IPD (63.5mm) divided by 2.</summary>
        private const float HalfIpd = 0.03175f;

        private readonly List<XRNodeState> nodeStates = new List<XRNodeState>();

        void OnEnable()
        {
            if (modeSelector != null) modeSelector.OnModeChanged += Rebuild;
        }

        void OnDisable()
        {
            if (modeSelector != null) modeSelector.OnModeChanged -= Rebuild;
        }

        void Start()
        {
            if (modeSelector != null) Rebuild(modeSelector.CurrentMode);
        }

        public void Rebuild(RecordingMode mode)
        {
            if (markerRig == null || markerPrefab == null) return;

            // Clear existing markers.
            for (int i = markerRig.childCount - 1; i >= 0; i--)
                Destroy(markerRig.GetChild(i).gameObject);

            Vector3 camPos = GetRecordingPosition(mode);
            Quaternion camRot = Camera.main != null ? Camera.main.transform.rotation : Quaternion.identity;

            foreach (var m in CalibrationMarkers.All)
            {
                // Angles are relative to the selected recording camera, not centre eye.
                Vector3 dir = camRot * Quaternion.Euler(-m.el, m.az, 0f) * Vector3.forward;
                Vector3 worldPos = camPos + dir * CalibrationMarkers.MarkerDistance;

                GameObject dot = Instantiate(markerPrefab, worldPos, Quaternion.identity, markerRig);
                dot.name = "Marker_" + m.id;

                var label = dot.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = m.id.ToString();
            }
        }

        private Vector3 GetRecordingPosition(RecordingMode mode)
        {
            Transform center = Camera.main != null ? Camera.main.transform : transform;

            if (mode == RecordingMode.Binocular)
                return center.position;

            XRNode node = mode == RecordingMode.LeftEye ? XRNode.LeftEye : XRNode.RightEye;
            float sign = mode == RecordingMode.LeftEye ? -1f : 1f;

            // Try the runtime eye node first.
            InputTracking.GetNodeStates(nodeStates);
            for (int i = 0; i < nodeStates.Count; i++)
            {
                XRNodeState s = nodeStates[i];
                if (s.nodeType == node && s.TryGetPosition(out Vector3 local))
                {
                    // X offset is reliable; Y/Z may collapse to centre eye. Reject a zero result.
                    if (local.sqrMagnitude > 1e-8f)
                    {
                        Transform basis = cameraOffset != null ? cameraOffset : center;
                        return basis.TransformPoint(local);
                    }
                }
            }

            // Mandatory hardcoded IPD fallback (Quest 3).
            return center.position + center.right * sign * HalfIpd;
        }
    }
}
