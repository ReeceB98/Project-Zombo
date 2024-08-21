using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Unity new input system (input action)
    private PlayerControls playerControls;

    // Player physics
    private Rigidbody2D rb;

    // Player movement speed & rotation speed with right analog stick
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;

    // Set the direction of movement, mouse position and right analog stick
    private Vector2 movementInput;
    private Vector2 mousePos;
    private Vector2 lookInput;

    private Camera cam;

    // Shooting properties
    public GameObject bulletPrefab;
    public Transform firepoint;
    public float fireforce;
    private float fireRate = 0.5f;
    private float nextFire;
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private int currentAmmo;
    [SerializeField] private float reloadTime = 2.813f;
    private bool isReloading;

    // Condition to hold an input down
    bool isHeld = false;

    // Player animations
    private Animator anim;

    private AudioSource audioSource;
    [SerializeField] private AudioClip sfx1, sfx2;
    

    private void Awake()
    {
        // Get the components of Unity input system
        playerControls = new PlayerControls();

        rb = GetComponent<Rigidbody2D>();
        cam = FindObjectOfType<Camera>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Start with idle animation
        anim.Play("PlayerIdle");
        currentAmmo = maxAmmo;
    }

    private void OnEnable()
    {
        // Enable gameplay inputs
        playerControls.Gameplay.Enable();

        // On enable, Perform and cancel player movement
        playerControls.Gameplay.Movement.performed += OnMovement;
        playerControls.Gameplay.Movement.canceled += OnMovement;

        // On enable, Perform mouse aim position
        playerControls.Gameplay.MousePosition.performed += OnMousePosition;

        // On enable, Perform right stick aim 
        playerControls.Gameplay.Look.performed += OnLook;
        playerControls.Gameplay.Look.canceled += OnLook;

        // On enable, Perform shooting
        playerControls.Gameplay.Fire.performed += OnFire;
        playerControls.Gameplay.Fire.canceled += OnFire;

        // On enable, Perform sprint
        //playerControls.Gameplay.Sprint.performed += OnSprint;
        //playerControls.Gameplay.Sprint.canceled += OnSprint;

        ///playerControls.Gameplay.Reload.performed += OnReload;
        //playerControls.Gameplay.Reload.canceled += OnReload;
    }

    private void OnDisable()
    {
        // Disable gameplay inputs
        playerControls.Gameplay.Disable();

        // On disable, perform and cancel player movement
        playerControls.Gameplay.Movement.performed -= OnMovement;
        playerControls.Gameplay.Movement.canceled -= OnMovement;

        // On disable, perform and cancel shooting
        playerControls.Gameplay.Fire.performed -= OnFire;
        playerControls.Gameplay.Fire.canceled -= OnFire;

        // On disable, perform and cancel sprint
        //playerControls.Gameplay.Sprint.performed -= OnSprint;
        //playerControls.Gameplay.Sprint.canceled -= OnSprint;

        //playerControls.Gameplay.Reload.performed -= OnReload;
        //playerControls.Gameplay.Reload.canceled -= OnReload;

    }

    private void Update()
    {
        if (isReloading)
        {
            return;
        }
        // Shoot weapon with a rate of fire
        if (isHeld && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            FireBullet();
            currentAmmo--;
        }

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
    }

    private void FixedUpdate()
    {
        SetMovement();
        SetMousePosition();
        SetLook();
    }

    private void OnMovement(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();

        // Walking Animations
        if (ctx.performed)
        {
            anim.SetFloat("Walking", movementInput.sqrMagnitude);
        }
        else if (ctx.canceled)
        {
            anim.SetFloat("Walking", movementInput.sqrMagnitude);
        }
    }

    private void SetMovement()
    {
        // Player movment calculation
        rb.velocity = new Vector2(movementInput.x * speed, movementInput.y * speed);
    }

    //private void OnSprint(InputAction.CallbackContext ctx)
    //{
    //    // Calculation to perform the sprint when player is moving
    //    if (ctx.performed)
    //    {
    //        speed *= 2;
    //        anim.SetBool("IsWalking", false);
    //        anim.SetBool("IsRunning", true);
    //    }
    //    else if (ctx.canceled)
    //    {
    //        speed = 5;
    //        anim.SetBool("IsWalking", true);
    //        anim.SetBool("IsRunning", false);
    //    }
    //}

    private void OnMousePosition(InputAction.CallbackContext ctx)
    {
        mousePos = cam.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
    }

    private void SetMousePosition()
    {
        // Switch to mouse position
        if (playerControls.Gameplay.MousePosition.WasPerformedThisFrame())
        {
            // Apply rotation
            Vector2 lookDirection = mousePos - rb.position;
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle);

            // Cursor is visible and not locked
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();

        // Right stick shooting animations
        if (ctx.performed)
        {
            anim.SetBool("IsShooting", true);
            anim.SetBool("IsIdle", false);
            isHeld = true;
        }
        else if (ctx.canceled)
        {
            anim.SetBool("IsShooting", false);
            anim.SetBool("IsIdle", true);
            isHeld = false;
        }

        // Right stick shooting animations when moving
        if (isHeld)
        {
            anim.SetBool("IsMovingShooting", true);
            anim.SetBool("IsWalking", false);
        }
        else
        {
            anim.SetBool("IsMovingShooting", false);
            anim.SetBool("IsWalking", true);
        }
    }

    private void SetLook()
    {
        // Switch to right stick aim input
        if (playerControls.Gameplay.Look.WasPerformedThisFrame())
        {
            // Only rotate if there is input from the right stick
            if (lookInput.sqrMagnitude > 0.1f)
            {
                RotateTowards(lookInput);
            }

            // Cursor is invisible and locked
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        // Left mouse button shooting animations
        if (ctx.performed)
        {
            isHeld = true;
            anim.SetBool("IsShooting", true);
            anim.SetBool("IsIdle", false);
        }
        else if (ctx.canceled)
        {
            isHeld = false;
            anim.SetBool("IsShooting", false);
            anim.SetBool("IsIdle", true);
        }

        // Left mouse button shooting animations when moving
        if (isHeld)
        {
            anim.SetBool("IsMovingShooting", true);
            anim.SetBool("IsWalking", false);
        }
        else
        {
            anim.SetBool("IsMovingShooting", false);
            anim.SetBool("IsWalking", true);
        }

    }

    private void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firepoint.position, firepoint.rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(firepoint.up * fireforce, ForceMode2D.Impulse);
        //SetMuzzleFlash();
        audioSource.clip = sfx1;
        audioSource.Play();

        Destroy(bullet, 0.5f);
    }

    //private void OnReload(InputAction.CallbackContext ctx)
    //{
    //    //if (ctx.performed)
    //    //{
    //        if (currentAmmo <= 0)
    //        {
    //            if (ctx.performed)
    //            {
    //                StartCoroutine(Reload());
    //                return;
    //            }
    //        }
    //    //}
    //}

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        playerControls.Gameplay.Fire.Disable();
        audioSource.clip = sfx2;
        audioSource.Play();
        yield return new WaitForSeconds(reloadTime);
        playerControls.Gameplay.Fire.Enable();
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
