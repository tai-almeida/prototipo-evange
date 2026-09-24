using System;
using UnityEngine;

// dash: ao receber o evento de dash, desloca o corpo na direcao travada por um tempo curto
public class EvangelineDash : MonoBehaviour
{
    [SerializeField] private EvangelineActions actions;
    [SerializeField] private EvangelineMovement movement;

    [SerializeField] private float distance = 6f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float cooldown = 0.5f; // conta a partir do fim do dash

    public event Action<Vector2> OnDashStarted;
    public event Action OnDashEnded;

    public bool IsDashing { get; private set; }

    private Rigidbody2D rigidbody;
    private Vector2 facing = Vector2.down;
    private Vector2 dashDirection;
    private float dashEndTime;
    private float nextDashTime;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (actions != null)
        {
            actions.OnDashTriggered += HandleDash;
        }

        if (movement != null)
        {
            movement.OnMoveVectorChanged += HandleMovementChanged;
        }
    }

    private void OnDisable()
    {
        if (actions != null)
        {
            actions.OnDashTriggered -= HandleDash;
        }

        if (movement != null)
        {
            movement.OnMoveVectorChanged -= HandleMovementChanged;
        }
    }

    private void HandleMovementChanged(Vector2 moveInput)
    {
        // parado mantem a ultima direcao
        if (moveInput.sqrMagnitude > 0)
        {
            facing = moveInput;
        }
    }

    private void HandleDash()
    {
        if (IsDashing || Time.time < nextDashTime) return;

        // a direcao fica travada no inicio do dash
        dashDirection = facing;
        dashEndTime = Time.time + duration;
        IsDashing = true;

        OnDashStarted?.Invoke(dashDirection);
    }

    private void FixedUpdate()
    {
        if (!IsDashing) return;

        if (Time.time >= dashEndTime)
        {
            IsDashing = false;
            nextDashTime = Time.time + cooldown;
            OnDashEnded?.Invoke();
            return;
        }

        float speed = distance / duration;
        rigidbody.MovePosition(rigidbody.position + dashDirection * speed * Time.fixedDeltaTime);
    }
}
