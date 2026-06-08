using UnityEngine;

namespace CalibrationApp
{
    /// <summary>Billboards this transform so its +Z faces away from the main camera,
    /// keeping attached world-space TMP text readable from the user's viewpoint.</summary>
    public class FaceCamera : MonoBehaviour
    {
        void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 dir = transform.position - cam.transform.position;
            if (dir.sqrMagnitude < 1e-8f) return;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
