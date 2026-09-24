using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class EvangelineActions : MonoBehaviour
{
    public event Action<Vector2> OnMoveVectorChanged;
    public event Action OnAttackTriggered;
    public event Action OnCastTriggered;
    public event Action OnTakeDamageTriggered;
    public event Action OnDieTriggered;

    public event Action OnDashTriggered;

    private void Update()
    {
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnAttack();
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                OnCast();
            }
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnDash();
        }
    }

    public void OnAttack() => OnAttackTriggered?.Invoke();
    public void OnCast() => OnCastTriggered?.Invoke();
    public void OnDash() => OnDashTriggered?.Invoke();
    public void TakeDamage() => OnTakeDamageTriggered?.Invoke();
    public void Die() => OnDieTriggered?.Invoke();
}