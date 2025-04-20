using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour {
    [SerializeField, Range(0f, 50f)] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 50f)] public float maxAcceleration = 30f;
    [SerializeField, Range(0f, 25f)] public float jumpPower = 9.5f;
    [SerializeField, Range(0f, 25f)] public float mouseSensitivity = 5f;
    [SerializeField, Range(0, 250)] public int maxHealth = 100;
    [SerializeField, Range(0f, 50f)] public float range = 25f;

    [SerializeField] public PlayerHealthbar healthbar;
    [SerializeField] public int currentHealth;
    [SerializeField] public Transform gunTransform;
    [SerializeField] public ParticleSystem shootParticle;
    [SerializeField] public GameObject hitParticleObj;
    [SerializeField] public TextMeshProUGUI ammoText;

    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip shootSound;
    [SerializeField] public AudioClip deathSound;

    private Vector3 velocity = Vector3.zero;
    private float rotationX;
    private float reloadCooldown = -10f;
    private float shootCooldown = -10f;
    private int bullets = 15;

    private Rigidbody rb;
    private AudioSource audioSource;
    private CapsuleCollider playerCollider;
    private Camera camera;
    private float groundCheckDistance;
    private float currentMaxSpeed;

    private void Awake() {
        // Initialise variables
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponentInChildren<CapsuleCollider>();
        camera = Camera.main;
        groundCheckDistance = (playerCollider.height / 2f) + 0.2f;      // 0.2f of leeway is added to make jump input feel more responsive
        currentMaxSpeed = maxSpeed;

        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);

        // Increase gravity to make movement feel less floaty
        Physics.gravity = new Vector3(0f, -19.4f, 0f);

        // Hide and lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        // Camera and Inputs are in Update not FixedUpdate to ensure inputs are not dropped
        HandleCamera();
        Inputs();
    }

    private void FixedUpdate() {
        Movement();
    }

    private void HandleCamera() {
        // Create Vector2 from mouse input
        Vector2 mouseInput = new Vector2 {
            x = Input.GetAxis("Mouse X") * mouseSensitivity,
            y = Input.GetAxis("Mouse Y") * mouseSensitivity
        };

        // Clamp the rotation so the player cant look behind themselves
        rotationX = Mathf.Clamp(rotationX - mouseInput.y, -90f, 90f);

        // Rotate the camera vertically
        camera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Rotate the player horizontally
        rb.transform.Rotate(Vector3.up * mouseInput.x);
    }

    private void Movement() {
        // Create Vector2 from joystick or keyboard input
        Vector2 playerInput = new Vector2 {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };

        // Normalise player input to keep speed consistent with multiple inputs
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
    
        // Create Vector3 for desired velocity
        // rb.transform is used so that the player moves relative to the direction they are facing
        Vector3 desiredVelocity = (rb.transform.forward * playerInput.y) + (rb.transform.right * playerInput.x);

        // If in the air, reduce max speed
        if (!CheckGrounded()) {
            desiredVelocity *= (currentMaxSpeed / 1.5f);
        } else {
            desiredVelocity *= currentMaxSpeed;
        }

        // Store change in speed over time to clamp velocity to match max speed
        float speedChange = maxAcceleration * Time.deltaTime;

        // Calculate the current velocity
        velocity = Vector3.MoveTowards(velocity, desiredVelocity, speedChange);

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
        if (Input.GetMouseButton(0) && bullets > 0 && Time.time >= shootCooldown + 0.4f) {
            shootCooldown = Time.time;
            bullets--;
            ammoText.text = bullets.ToString();

            // Play particle at barrel of gun, and sound effect
            shootParticle.Play();
            audioSource.PlayOneShot(shootSound, 0.3f);

            // Rotate camera slightly to simulate gun kick back
            rotationX -= 2f;

            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, range)) {
                if (hit.collider != playerCollider) {
                    // Create hit particle at hit location, delete after 0.5 seconds
                    Destroy(Instantiate(hitParticleObj, hit.point, Quaternion.identity), 0.5f);
    
                    if (hit.collider.CompareTag("Breakable")) {
                        Destroy(hit.collider.gameObject);
                    }

                    if (hit.collider.CompareTag("Enemy")) {
                        // GetComponent is fine here as it is only being called when a bullet hits an enemy not every runtime
                        hit.collider.GetComponentInParent<EnemyController>().TakeDamage(10);
                    }
                }
            }
        }

        // Aim input
        if (Input.GetMouseButtonDown(1)) {
            camera.fieldOfView = 55f;
            gunTransform.localPosition = new Vector3(-0.01f, gunTransform.localPosition.y, gunTransform.localPosition.z);
            currentMaxSpeed = maxSpeed / 3f;
        } else if (Input.GetMouseButtonUp(1)) {
            camera.fieldOfView = 75f;
            gunTransform.localPosition = new Vector3(0.35f, gunTransform.localPosition.y, gunTransform.localPosition.z);
            currentMaxSpeed = maxSpeed;
        }

        // Reload input
        if (Input.GetKeyDown(KeyCode.R) && bullets != 15 && Time.time >= reloadCooldown + 2f) {
            reloadCooldown = Time.time;
            shootCooldown = Time.time + 1.2f;     // Stop player from shooting too soon after reloading

            bullets = 15;
            audioSource.PlayOneShot(reloadSound, 0.5f);
            ammoText.text = bullets.ToString();
        }
    }

    private bool CheckGrounded() {
        // Create raycast downwards to check if player is close enough to ground
        RaycastHit hit;
        if (Physics.Raycast(playerCollider.transform.position, -playerCollider.transform.up, out hit, groundCheckDistance)) {
            return true;
        } else {
            return false;
        }
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        healthbar.SetHealth(currentHealth);

        if (currentHealth <= 0) {
            audioSource.PlayOneShot(deathSound, 1f);
            // Death code here later
            Debug.Log("Dead");
        }
    }
}
