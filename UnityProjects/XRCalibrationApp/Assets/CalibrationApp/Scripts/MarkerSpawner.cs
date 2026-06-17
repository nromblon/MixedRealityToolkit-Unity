using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using System.Linq;

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
        private float HalfIpd = 0.03175f;
        //private bool hasRuntimeIpdLogged = false;

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
            Debug.Log($"[MarkerSpawner] Center eye position: {center.position}");
            if (mode == RecordingMode.Binocular)
                return center.position;

            XRNode node = mode == RecordingMode.LeftEye ? XRNode.LeftEye : XRNode.RightEye;
            float sign = mode == RecordingMode.LeftEye ? -1f : 1f;

            //if (!hasRuntimeIpdLogged)
            //{
            //    hasRuntimeIpdLogged = true;
            //    Vector3 leftEyePos = Vector3.zero;
            //    Vector3 rightEyePos = Vector3.zero;

            //    var leftEyeDevice = new List<InputDevice>();
            //    var rightEyeDevice = new List<InputDevice>();

            //    InputDevices.GetDevicesAtXRNode(XRNode.LeftEye, leftEyeDevice);
            //    InputDevices.GetDevicesAtXRNode(XRNode.RightEye, rightEyeDevice);

            //    if (leftEyeDevice.Count > 0)
            //    {
            //        leftEyeDevice[0].TryGetFeatureValue(CommonUsages.devicePosition, out leftEyePos);
            //    }
            //    if (rightEyeDevice.Count > 0)
            //    {
            //        rightEyeDevice[0].TryGetFeatureValue(CommonUsages.devicePosition, out rightEyePos);
            //    }

            //    if (leftEyePos.sqrMagnitude > 1e-8f && rightEyePos.sqrMagnitude > 1e-8f)
            //    {
            //        float runtimeIpd = leftEyePos.x - rightEyePos.x;
            //        Debug.Log($"[MarkerSpawner] leftEyePos={leftEyePos} || rightEyePos={rightEyePos}");
            //        Debug.Log($"[MarkerSpawner] Runtime IPD={runtimeIpd * 1000f}mm");
            //        HalfIpd = Mathf.Abs(runtimeIpd) / 2f;
            //    }
            //}

            return center.position + center.right * sign * HalfIpd;
        }
    }
}
