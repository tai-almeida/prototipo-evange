using System;
using UnityEngine;


public class EvangelineActions : MonoBehaviour
{
    public event Action<Vector2> OnMoveVectorChanged;
    public event Action OnAttackTriggered;
    public event Action OnTakeDamageTriggered;
    public event Action OnDieTriggered;

    public void OnAttack() => OnAttackTriggered?.Invoke();
    public void TakeDamage() => OnTakeDamageTriggered?.Invoke();
    public void Die() => OnDieTriggered?.Invoke();
}