using UnityEngine;

public class EvangelineAnimation : MonoBehaviour
{
    [SerializeField] private EvangelineMovement movementScript;
    [SerializeField] private LineAttack wandAttack;
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        if (movementScript != null)
        {
            movementScript.OnMoveVectorChanged += HandleMovementChanged;
        }

        if (wandAttack != null)
        {
            wandAttack.OnAttackStarted += HandleWandAttackStarted;
        }
    }

    private void OnDisable()
    {
        if (movementScript != null)
        {
            movementScript.OnMoveVectorChanged -= HandleMovementChanged;
        }

        if (wandAttack != null)
        {
            wandAttack.OnAttackStarted -= HandleWandAttackStarted;
        }
    }

    private void HandleWandAttackStarted(Vector2 direction)
    {
        if (animator == null) return;

        animator.SetTrigger("Cast");
    }

    private void HandleMovementChanged(Vector2 moveInput)
    {
        if (animator == null) return;

        animator.SetFloat("Speed", moveInput.sqrMagnitude);

        if (moveInput.sqrMagnitude > 0)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }
    }
}