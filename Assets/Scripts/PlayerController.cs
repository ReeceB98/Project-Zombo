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

    public GameObject bulletPrefab;
    public Transform firepoint;
    public float fireforce;

    private float fireRate = 1.0f;
    private float nextFire;

    private void Awake()
    {
        // Get the components of Unity input system
        playerControls = new PlayerControls();

        rb = GetComponent<Rigidbody2D>();
        cam = FindObjectOfType<Camera>();
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

        playerControls.Gameplay.Fire.performed += OnFire;
        playerControls.Gameplay.Fire.canceled += OnFire;
    }

    private void OnDisable()
    {
        // Disable gameplay inputs
        playerControls.Gameplay.Disable();

        // On disable, perform and cancel player movement
        playerControls.Gameplay.Movement.performed -= OnMovement;
        playerControls.Gameplay.Movement.canceled -= OnMovement;

        playerControls.Gameplay.Fire.performed -= OnFire;
        playerControls.Gameplay.Fire.canceled -= OnFire;

    }

    private void Update()
    {
        
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
    }

    private void SetMovement()
    {
        // Player movment calculation
        rb.velocity = new Vector2(movementInput.x * speed, movementInput.y * speed);
    }

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
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90;
            rb.MoveRotation(angle);

            // Cursor is visible and not locked
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    private void SetLook()
    {
        // Switch to right stick aim input
        if (playerControls.Gameplay.Look.WasPerformedThisFrame())
        {
            // Only rotate if there is input from the right stick

            RotateTowards(lookInput);
            FireBullet();

            // Cursor is invisible and locked
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

        // Apply rotation
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (playerControls.Gameplay.Fire.WasPerformedThisFrame())
        {
            if (ctx.performed && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                FireBullet();
            }
        }
    }

    private void FireBullet()
    {
        Debug.Log("Gun was fired");
        GameObject bullet = Instantiate(bulletPrefab, firepoint.position, firepoint.rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(firepoint.up * fireforce, ForceMode2D.Impulse);

        Destroy(bullet, 0.2f);
    }
}
