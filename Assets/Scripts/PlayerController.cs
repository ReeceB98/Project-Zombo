using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls playerControls;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float controllerDeadzone;

    private Transform playersTransform;


    private Vector2 movementInput;
    private Vector2 mousePos;
    private Vector2 lookInput;

    [SerializeField] private Camera cam;


    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerControls = new PlayerControls();

    }

    private void Start()
    {
        playersTransform = this.transform;
    }

    private void OnEnable()
    {
        playerControls.Gameplay.Enable();

        playerControls.Gameplay.Movement.performed += OnMovement;
        playerControls.Gameplay.Movement.canceled += OnMovement;

        playerControls.Gameplay.MousePosition.performed += OnMousePosition;

        playerControls.Gameplay.Look.performed += OnLook;
    }

    private void OnDisable()
    {
        playerControls.Gameplay.Disable();

        playerControls.Gameplay.Movement.performed -= OnMovement;
        playerControls.Gameplay.Movement.canceled -= OnMovement;

    }

    private void Update()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(movementInput.x * speed, movementInput.y * speed);

        if (playerControls.Gameplay.MousePosition.WasPerformedThisFrame())
        {

            Vector2 lookDirection = mousePos - rb.position;
            //Debug.Log(lookDirection);
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90;
            //Debug.Log(angle);
            rb.MoveRotation(angle);
            //Debug.Log(rb.velocity);
            //Debug.Log(movementInput);
        }

        if (playerControls.Gameplay.Look.WasPerformedThisFrame())
        {
            // Only rotate if there is input from the right stick
            if (lookInput.sqrMagnitude > 0.1f)
            {
                RotateTowards(lookInput);
            }
        }
    }

    private void OnMovement(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
        //playerControls.Gameplay.Movement;
    }

    private void OnMousePosition(InputAction.CallbackContext ctx)
    {
        //Vector2 direction = Camera.main.ScreenToWorldPoint(
        //    Input.mousePosition) - playersTransform.position;
        //Debug.Log(direction);
        mousePos = cam.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    private void RotateTowards(Vector2 direction)
    {
        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        //Debug.Log(angle);

        // Apply rotation
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        playersTransform.rotation = Quaternion.RotateTowards(playersTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
