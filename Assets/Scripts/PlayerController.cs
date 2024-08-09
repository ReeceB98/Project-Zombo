using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls playerControls;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;


    private Vector2 movementInput;

    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Gameplay.Enable();

        playerControls.Gameplay.Movement.performed += SetMovement;
        playerControls.Gameplay.Movement.canceled += SetMovement;
    }

    private void OnDisable()
    {
        playerControls.Gameplay.Disable();

        playerControls.Gameplay.Movement.performed -= SetMovement;
        playerControls.Gameplay.Movement.canceled -= SetMovement;
    }

    // Update is called once per frame
    private void Update()
    {
        rb.velocity = new Vector2(movementInput.x * speed, movementInput.y * speed);
        //Debug.Log(rb.velocity);
        Debug.Log(movementInput);
    }

    public void SetMovement(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
        //playerControls.Gameplay.Movement;
    }
}
