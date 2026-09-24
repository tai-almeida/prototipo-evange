using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EvangelineMovement : MonoBehaviour
{
    // esse script cuida apenas da leitura do inputo e dispara um evento para informar a direcao e velocidade
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private EvangelineDash dash;

    public event Action<Vector2> OnMoveVectorChanged;

    private Rigidbody2D rigidbody;
    private Vector2 moveInput;
    private bool isDashing;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (dash != null)
        {
            dash.OnDashStarted += HandleDashStarted;
            dash.OnDashEnded += HandleDashEnded;
        }
    }

    private void OnDisable()
    {
        if (dash != null)
        {
            dash.OnDashStarted -= HandleDashStarted;
            dash.OnDashEnded -= HandleDashEnded;
        }
    }

    // durante o dash quem move o corpo e o EvangelineDash
    private void HandleDashStarted(Vector2 direction) => isDashing = true;
    private void HandleDashEnded() => isDashing = false;

    private void Update()
    {
        if (Keyboard.current == null) return;

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            moveY += 1f;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            moveY -= 1f;
        } 
        if (Keyboard.current.aKey.isPressed)
        {
            moveX -= 1f;
        } 
        if (Keyboard.current.dKey.isPressed)
        {
            moveX += 1f;
        } 

        Vector2 newInput = new Vector2(moveX, moveY).normalized;

        if (newInput != moveInput)
        {
            moveInput = newInput;
            OnMoveVectorChanged?.Invoke(moveInput);
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        rigidbody.MovePosition(rigidbody.position 
                                + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

}

