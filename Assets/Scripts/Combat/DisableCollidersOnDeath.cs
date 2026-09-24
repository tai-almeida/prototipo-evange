using UnityEngine;

// quando morre, desliga os colliders para o corpo nao bloquear passagem nem receber ataques
public class DisableCollidersOnDeath : MonoBehaviour
{
    [SerializeField] private Health health;

    private void OnEnable()
    {
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        foreach (var collider in GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }
    }
}
