using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls playerControls;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;

    private Transform playersTransform;


    private Vector2 movementInput;
    private Vector2 mousePos;

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
    }

    private void OnDisable()
    {
        playerControls.Gameplay.Disable();

        playerControls.Gameplay.Movement.performed -= OnMovement;
        playerControls.Gameplay.Movement.canceled -= OnMovement;
    }

    private void Update()
    {
        //OnMousePosition();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(movementInput.x * speed, movementInput.y * speed);

        Vector2 lookDirection = mousePos - rb.position;
        //Debug.Log(lookDirection);
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90;
        //Debug.Log(angle);
        rb.MoveRotation(angle);
        //Debug.Log(rb.velocity);
        //Debug.Log(movementInput);
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
}
