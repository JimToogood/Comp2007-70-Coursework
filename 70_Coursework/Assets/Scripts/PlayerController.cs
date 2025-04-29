using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour {
    [SerializeField, Range(0f, 50f)] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 50f)] private float maxAcceleration = 30f;
    [SerializeField, Range(0f, 25f)] private float jumpPower = 9.5f;
    [SerializeField, Range(0f, 25f)] private float mouseSensitivity = 5f;
    [SerializeField, Range(0f, 50f)] private float range = 25f;

    [SerializeField] private PlayerHealthbar healthbar;
    [SerializeField] private Transform gunTransform;
    [SerializeField] private ParticleSystem shootParticle;
    [SerializeField] private GameObject hitParticleObj;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private ParticleSpawner breakableParticleSpawner;
    [SerializeField] private GameObject shopKeeper;
    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip shootSound;

    public int maxHealth;
    public int currentHealth;
    public bool inSafeZone = false;

    private float groundCheckDistance;
    private float currentMaxSpeed;
    private int bulletsInClip;
    private int gold;
    private float rotationX;
    private int maxBullets;
    private float shootCooldown = -10f;
    private float reloadCooldown = -10f;
    private bool inputsDisabled = false;
    private string currentMenu = "";
    private Vector3 velocity = Vector3.zero;

    private Rigidbody rb;
    private AudioSource audioSource;
    private CapsuleCollider playerCollider;
    private ParticleSpawner hitParticleSpawner;
    private ShopKeeperController shopKeeperController;
    private Camera camera;
    private Animator gunAnimator;
    private SaveDataManager saveDataManager;

    private void Awake() {
        // Initialise variables
        pauseMenu.SetActive(false);
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponentInChildren<CapsuleCollider>();
        hitParticleSpawner = GetComponentInChildren<ParticleSpawner>();
        shopKeeperController = shopKeeper.GetComponent<ShopKeeperController>();
        camera = Camera.main;
        gunAnimator = GetComponentInChildren<Animator>();
        saveDataManager = GameObject.FindGameObjectWithTag("SaveManager").GetComponent<SaveDataManager>();

        groundCheckDistance = (playerCollider.height / 2f) + 0.2f;      // 0.2f of leeway is added to make jump input feel more responsive
        currentMaxSpeed = maxSpeed;

        // Increase gravity to make movement feel less floaty
        Physics.gravity = new Vector3(0f, -19.4f, 0f);

        // Hide and lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        // These functions are in Update not FixedUpdate to ensure player inputs are never dropped
        if (!inputsDisabled) {
            HandleCamera();
            Inputs();
            if (!inSafeZone) {
                // Dont allow player to shoot in safe zone to avoid cheesing the enemies
                CombatInputs();
            } else {
                CheckOpenShop();
            }
        } else {
            CheckCloseMenu();
        }
    }

    private void FixedUpdate() {
        if (!inputsDisabled) {
            Movement();
        }
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
    
        // Create Vector3 for desired velocity, relative to rb facing direction
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

        // Pause input
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePause(false);
        }

        // Aim input
        if (Input.GetMouseButtonDown(1) && Time.time >= reloadCooldown + 1f) {
            ToggleAim(true);
        } else if (Input.GetMouseButtonUp(1)) {
            ToggleAim(false);
        }

        // Reload input
        if (Input.GetKeyDown(KeyCode.R) && bulletsInClip != 15 && Time.time >= reloadCooldown + 2f && maxBullets > 0) {
            ToggleAim(false);
            reloadCooldown = Time.time;
            shootCooldown = Time.time + 1.2f;     // Stop player from shooting too soon after reloading
            gunAnimator.Play("GunReload");

            maxBullets -= (15 - bulletsInClip);
            bulletsInClip = 15;
            if (maxBullets < 0) {
                // Ensure maxBullets is never negative
                bulletsInClip += maxBullets;
                maxBullets = 0;
            }

            audioSource.PlayOneShot(reloadSound, 0.5f);
            ammoText.text = bulletsInClip.ToString() + " / " + maxBullets.ToString();
        }
    }

    private void CombatInputs() {
        // Shoot input
        if (Input.GetMouseButton(0) && bulletsInClip > 0 && Time.time >= shootCooldown + 0.4f) {
            shootCooldown = Time.time;
            bulletsInClip--;
            ammoText.text = bulletsInClip.ToString() + " / " + maxBullets.ToString();

            // Play particle at barrel of gun, kickback animation, and sound effect
            shootParticle.Play();
            audioSource.PlayOneShot(shootSound, 0.3f);
            gunAnimator.Play("GunKickback");

            // Rotate camera slightly to simulate gun kick back
            rotationX -= 2f;

            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, range)) {
                if (hit.collider != playerCollider) {
                    // Spawn hit particle at hit location
                    hitParticleSpawner.SpawnParticle(hit.point);

                    if (hit.collider.CompareTag("Breakable")) {
                        breakableParticleSpawner.SpawnParticle(hit.point);
                        hit.collider.gameObject.SetActive(false);
                    }

                    if (hit.collider.CompareTag("Enemy")) {
                        // GetComponent is fine here as it is only being called when a bullet hits an enemy not every runtime
                        hit.collider.GetComponentInParent<EnemyController>().TakeDamage(10);
                    }
                }
            }
        }
    }

    private void CheckCloseMenu() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (Time.timeScale == 0) {
                // If in pause menu
                TogglePause(true);
            } else {
                // If in shop menu
                shopKeeperController.ToggleShop();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                inputsDisabled = false;
            }
        }
    }

    public void TogglePause(bool isOpen) {
        if (isOpen) {
            // Unpause
            pauseMenu.SetActive(false);
            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inputsDisabled = false;
        } else {
            // Pause
            pauseMenu.SetActive(true);
            Time.timeScale = 0;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inputsDisabled = true;
        }
    }

    private void CheckOpenShop() {
        if (Input.GetMouseButtonDown(0)) {
            // If within 3 units of the shop keeper
            if (Vector3.Distance(playerCollider.transform.position, shopKeeper.transform.position) < 3f) {
                // Reset velocities so player stops moving when opening the shop
                rb.velocity = Vector3.zero;
                velocity = Vector3.zero;
                shopKeeperController.ToggleShop();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                inputsDisabled = true;
            }
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

    private void ToggleAim(bool isAiming) {
        if (isAiming) {
            // Aim
            camera.fieldOfView = 55f;
            gunTransform.localPosition = new Vector3(-0.01f, gunTransform.localPosition.y, gunTransform.localPosition.z);
            currentMaxSpeed = maxSpeed / 3f;
        } else {
            // Unaim
            camera.fieldOfView = 75f;
            gunTransform.localPosition = new Vector3(0.35f, gunTransform.localPosition.y, gunTransform.localPosition.z);
            currentMaxSpeed = maxSpeed;
        }
    }

    public void TakeDamage(int damage) {
        // Stop the playing taking damage when in the safe zone
        if (!inSafeZone) {
            currentHealth -= damage;
            healthbar.SetHealth(currentHealth);
            healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
            audioSource.PlayOneShot(damageSound, 0.4f);

            if (currentHealth <= 0) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Remove gold as punishment for dying, and reset current health and position to avoid death loop
                gold /= 2;
                if (gold < 0) {
                    gold = 0;
                }
                currentHealth = maxHealth;
                rb.transform.position = Vector3.zero;
                saveDataManager.Save();
                
                SceneManager.LoadScene("DeathScene");
            }
        }
    }

    public void FullHeal() {
        currentHealth = maxHealth;
        healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        healthbar.SetHealth(currentHealth);
    }

    public bool ChangeGold(int amount) {
        if (gold + amount >= 0) {
            gold += amount;
            goldText.text = gold.ToString();

            return true;
        } else {
            // If player does not have enough gold to complete transaction
            return false;
        }
    }

    public void ChangeMaxBullets(int amount) {
        maxBullets += amount;
        ammoText.text = bulletsInClip.ToString() + " / " + maxBullets.ToString();
    }

    public void ChangeMaxHealth(int amount) {
        maxHealth += amount;
        currentHealth += amount;
        healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        healthbar.SetMaxHealth(maxHealth, currentHealth);
    }

    public string[] GetAttributes() {
        // Vector3.Parse does not exist, so each sub-coordinate is stored individually
        string[] attributes = new string[] {
            rb.transform.position.x.ToString(),
            rb.transform.position.y.ToString(),
            rb.transform.position.z.ToString(),
            gold.ToString(),
            bulletsInClip.ToString(),
            maxBullets.ToString(),
            currentHealth.ToString(),
            maxHealth.ToString()
        };

        return attributes;
    }

    public void SetAttributes(string[] attributes = null) {
        if (attributes != null) {
            // If attributes is not null, aka save file found
            try {
                rb.transform.position = new Vector3(float.Parse(attributes[0]), float.Parse(attributes[1]), float.Parse(attributes[2]));
                gold = int.Parse(attributes[3]);
                bulletsInClip = int.Parse(attributes[4]);
                maxBullets = int.Parse(attributes[5]);
                currentHealth = int.Parse(attributes[6]);
                maxHealth = int.Parse(attributes[7]);
            } catch (Exception error) {
                // If one or more of the variables was not the expected format
                Debug.Log("Error trying to assign attributes: " + error.Message);
                Debug.Log("Defaulting attributes...");
                SetDefaultAttributes();
            }
        } else {
            // If attributes is null, aka no save file found
            SetDefaultAttributes();
        }

        // Update UI
        healthbar.SetMaxHealth(maxHealth);
        healthbar.SetHealth(currentHealth);
        goldText.text = gold.ToString();
        healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        ammoText.text = bulletsInClip.ToString() + " / " + maxBullets.ToString();
    }

    private void SetDefaultAttributes() {
        // In case of error, reset attributes to initial values
        rb.transform.position = Vector3.zero;
        gold = 0;
        bulletsInClip = 15;
        maxBullets = 90;
        currentHealth = 100;
        maxHealth = 100;
    }

    private void OnApplicationFocus(bool hasFocus) {
        if (!hasFocus) {
            // Pause the game when player alt tabs or switches away from game
            TogglePause(false);
        }
    }
}
