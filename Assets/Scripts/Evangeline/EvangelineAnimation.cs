using UnityEngine;

public class EvangelineAnimation : MonoBehaviour
{
    [SerializeField] private EvangelineMovement movementScript;
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        if (movementScript != null)
        {
            movementScript.OnMoveVectorChanged += HandleMovementChanged;
        }
    }

    private void OnDisable()
    {
        if (movementScript != null)
        {
            movementScript.OnMoveVectorChanged -= HandleMovementChanged;
        }
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