using System;
using UnityEngine;

// vida generica: qualquer objeto que pode tomar dano. Outros scripts reagem pelos eventos
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    public event Action<int> OnDamaged;
    public event Action OnDied;

    public int Current { get; private set; }
    public bool IsDead => Current <= 0;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        Current = Mathf.Max(Current - amount, 0);
        OnDamaged?.Invoke(amount);

        if (IsDead)
        {
            OnDied?.Invoke();
        }
    }
}
