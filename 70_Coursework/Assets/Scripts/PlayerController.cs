using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour {
    [SerializeField, Range(0f, 25f)] public float mouseSensitivity = 5f;
    [SerializeField, Range(0f, 50f)] public float maxAcceleration = 30f;
    [SerializeField, Range(0f, 25f)] public float jumpPower = 9.5f;
    [SerializeField, Range(0f, 50f)] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 50f)] public float range = 20f;

    [SerializeField] public ParticleSystem shootParticle;
    [SerializeField] public Transform cameraTransform;
    [SerializeField] public GameObject hitParticleObj;
    [SerializeField] public TextMeshProUGUI ammoText;
    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip shootSound;

    private float rotationX;
    private int bullets = 15;
    private float groundCheckDistance;
    private Vector3 velocity = Vector3.zero;

    private Rigidbody rb;
    private AudioSource audioSource;
    private CapsuleCollider playerCollider;

    void Start() {
        Debug.Log("'PlayerController.cs' initialised.");

        // Initialise variables
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponentInChildren<AudioSource>();
        playerCollider = GetComponentInChildren<CapsuleCollider>();
        groundCheckDistance = (playerCollider.height / 2f) + 0.2f;

        // Increase gravity to make jumps feel less floaty
        Physics.gravity = new Vector3(0f, -19.4f, 0f);

        // Hide and lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        Camera();
        Inputs();
    }

    void FixedUpdate() {
        Movement();
    }

    private void Camera() {
        // Create Vector2 for input from mouse
        Vector2 mouseInput = new Vector2 {
            x = Input.GetAxis("Mouse X") * mouseSensitivity,
            y = Input.GetAxis("Mouse Y") * mouseSensitivity
        };

        // Clamp the rotation so the player cant look behind themselves
        rotationX = Mathf.Clamp(rotationX - mouseInput.y, -90f, 90f);

        // Rotate the camera vertically
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Rotate the player horizontally
        rb.transform.Rotate(Vector3.up * mouseInput.x);
    }

    private void Movement() {
        // Create Vector2 for input from joystick or keyboard
        Vector2 playerInput = new Vector2 {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };

        // Normalises player input to keep speed consistent even with multiple inputs
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
    
        // Create Vector3 for desired velocity
        // rb transform is used so that the player moves relative to the direction they are facing
        Vector3 desiredVelocity = (rb.transform.forward * playerInput.y) + (rb.transform.right * playerInput.x);

        // If in the air, reduce max speed
        if (!CheckGrounded()) {
            desiredVelocity *= (maxSpeed / 1.5f);
        } else {
            desiredVelocity *= maxSpeed;
        }

        // Store change in speed over time to clamp velocity to match max speed
        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        // Calculate the current velocity
        velocity = Vector3.MoveTowards(velocity, desiredVelocity, maxSpeedChange);

        // Retains current y velocity (gravity)
        velocity.y = rb.velocity.y;
        rb.velocity = velocity;
    }

    private void Inputs() {
        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && CheckGrounded()) {
            rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }

        // Shoot input
        if (Input.GetMouseButtonDown(0) && bullets > 0) {
            bullets--;
            ammoText.text = bullets.ToString();

            // Play particle at barrel of gun, and sound effect
            shootParticle.Play();
            audioSource.PlayOneShot(shootSound, 0.5f);

            // Rotate camera slightly to simulate gun kick back
            rotationX -= 2f;

            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, range)) {
                if (hit.collider != playerCollider) {
                    // Create hit particle at hit location, delete after 0.5 seconds
                    Destroy(Instantiate(hitParticleObj, hit.point, Quaternion.identity), 0.5f);
                    if (hit.collider.tag == "Enemy") {
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }

        // Reload input
        if (Input.GetKeyDown(KeyCode.R) && bullets != 15) {
            bullets = 15;
            audioSource.PlayOneShot(reloadSound, 0.7f);
            ammoText.text = bullets.ToString();
        }
    }

    private bool CheckGrounded() {
        RaycastHit hit;
        if (Physics.Raycast(playerCollider.transform.position, -playerCollider.transform.up, out hit, groundCheckDistance)) {
            return true;
        } else {
            return false;
        }
    }

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.red;
        // Visualise shooting range in editor
    //    Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * range);
    //}
}
