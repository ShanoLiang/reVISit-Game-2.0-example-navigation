using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Track total distance travelled
    private Vector3 lastPosition;
    public float totalDistanceTravelled = 0f;
    [Header("Movement")]
    public float speed = 6.0f;
    public float sprintMultiplier = 1.4f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform cam;
    public float minY = -70f;
    public float maxY = 70f;

    [Header("Events")]
    public UnityEvent onReachTarget;

    private CharacterController controller;
    private float cameraPitch = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 initialPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cam == null)
        {
            var camChild = transform.Find("Camera");
            if (camChild) cam = camChild;
            else Debug.LogWarning("Assign a Camera transform for PlayerController.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        initialPosition = transform.position;

    lastPosition = transform.position;

#if !ENABLE_LEGACY_INPUT_MANAGER
        Debug.LogWarning("?? Input System is set to 'New' only. This script uses the old Input Manager. To fix this, go to Edit > Project Settings > Player > Active Input Handling and set it to 'Both'.");
#endif
    }

    void Update()
    {
        if (transform.position.y < -10) // Reset if falling off world
            transform.position = initialPosition;

        CameraLook();
        PlayerMovement();

    // Track movement distance
    float frameDistance = Vector3.Distance(transform.position, lastPosition);
    totalDistanceTravelled += frameDistance;
    lastPosition = transform.position;
    }

    void CameraLook()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 20f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 20f * Time.deltaTime;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minY, maxY);
        if (cam)
            cam.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
#endif
    }

    void PlayerMovement()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = Vector3.ClampMagnitude(move, 1);

        float finalSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        controller.Move(move * finalSpeed * Time.deltaTime);

        // Jump (optional)
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
#endif
    }

    // Trigger onReachTarget when colliding with "Target"
    // Public function to send total distance via WebGLBridge
    [System.Serializable]
    public struct DistanceEvent {
        public float distance;
    }

    public void PostTotalDistanceToWebGL()
    {
        var data = new DistanceEvent { distance = totalDistanceTravelled };
        string json = JsonUtility.ToJson(data);
        WebGLBridge.PostJSON("distance_complete", json);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            onReachTarget?.Invoke();
            Debug.Log("Reaching Target");
        }

    }

}
