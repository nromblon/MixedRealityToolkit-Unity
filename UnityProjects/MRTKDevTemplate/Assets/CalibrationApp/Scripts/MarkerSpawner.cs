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
            StartCoroutine(InitialSpawnWhenReady());
        }

        System.Collections.IEnumerator InitialSpawnWhenReady()
        {
            if (Camera.main == null) yield break;
            Transform cam = Camera.main.transform;

            float timeout = 2f;
            while (cam.position.sqrMagnitude < 1e-4f && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            yield return null; // one extra frame for safety

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
                    Debug.Log($"[MarkerSpawner] center.pos={center.position}  node local={local}");
                    // X offset is reliable; Y/Z may collapse to centre eye. Reject a zero result.
                    if (local.sqrMagnitude > 1e-8f)
                    {
                        return center.position + center.right * local.x;
                    }
                }
            }

            // Mandatory hardcoded IPD fallback (Quest 3).
            return center.position + center.right * sign * HalfIpd;
        }
    }
}
