using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class EvangelineActions : MonoBehaviour
{
    public event Action<Vector2> OnMoveVectorChanged;
    public event Action OnAttackTriggered;
    public event Action OnTakeDamageTriggered;
    public event Action OnDieTriggered;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnAttack();
        }
    }

    public void OnAttack() => OnAttackTriggered?.Invoke();
    public void TakeDamage() => OnTakeDamageTriggered?.Invoke();
    public void Die() => OnDieTriggered?.Invoke();
}