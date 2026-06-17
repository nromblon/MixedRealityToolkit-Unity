using UnityEngine;

public class FaceUserOnSpawn : MonoBehaviour
{
    [SerializeField] float distance = 1.0f;     // metres in front
    [SerializeField] float verticalOffset = -0.1f; // slightly below eye line, comfortable
    [SerializeField] bool faceUser = true;

    void OnEnable()
    {
        // Defer one frame so the HMD pose is valid (camera not yet updated at scene load)
        StartCoroutine(PlaceNextFrame());
    }

    System.Collections.IEnumerator PlaceNextFrame()
    {
        yield return new WaitForSeconds(0.5f); // wait one frame for tracking to update Camera.main

        Transform cam = Camera.main.transform;

        // Flatten the forward direction so the panel doesn't tilt up/down with head pitch
        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPos = cam.position + forward * distance;
        targetPos.y = cam.position.y + verticalOffset;  // anchor to head height
        transform.position = targetPos;

        if (faceUser)
        {
            // Face the panel toward the user (rotate 180 so text faces them)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f); // keep upright
        }
    }
}
