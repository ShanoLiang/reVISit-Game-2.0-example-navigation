using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            transform.LookAt(mainCam.transform);
            transform.Rotate(0, 180, 0); // Flip left and right to face the camera in a correct way
        }
    }
}
