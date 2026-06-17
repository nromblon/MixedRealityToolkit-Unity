using UnityEngine;

namespace CalibrationApp
{
    /// <summary>
    /// Drives a ring/torus material colour from green (steady) to red (moving) based on
    /// the main camera's angular speed. Green at/below 1.5 deg/s, red at/above 6.0 deg/s.
    /// </summary>
    public class StabilityIndicator : MonoBehaviour
    {
        [SerializeField] private Renderer ringRenderer;
        [SerializeField] private float greenThreshold = 1.5f; // deg/s
        [SerializeField] private float redThreshold = 6.0f;    // deg/s

        private Quaternion lastRot;
        private Material mat;

        void Start()
        {
            if (ringRenderer != null) mat = ringRenderer.material;
            if (Camera.main != null) lastRot = Camera.main.transform.rotation;
        }

        void Update()
        {
            Camera cam = Camera.main;
            if (cam == null || mat == null) return;

            Quaternion current = cam.transform.rotation;
            float dt = Mathf.Max(Time.deltaTime, 1e-5f);
            float angularSpeed = Quaternion.Angle(lastRot, current) / dt;
            lastRot = current;

            float t = Mathf.InverseLerp(greenThreshold, redThreshold, angularSpeed);
            Color c = Color.Lerp(Color.green, Color.red, t);
            mat.color = c;
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c);
        }
    }
}
