using UnityEngine;

// traduz os eventos de vida em triggers do Animator ("Hit" e "Die")
public class HealthAnimation : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
    }

    private void HandleDamaged(int amount)
    {
        // o golpe que mata toca so a animacao de morte
        if (!health.IsDead)
        {
            animator.SetTrigger("Hit");
        }
    }

    private void HandleDied()
    {
        animator.SetTrigger("Die");
    }
}
