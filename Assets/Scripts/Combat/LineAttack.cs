using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LineAttack : MonoBehaviour
{
    [SerializeField] private EvangelineActions actions;
    [SerializeField] private EvangelineMovement movement;
    [SerializeField] private Transform origin;

    [SerializeField] private float range = 8f;
    [SerializeField] private float width = 0.2f;
    [SerializeField] private int damage = 3;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private float castDelay = 0.6f;
    [SerializeField] private LayerMask targetLayers = ~0;

    public event Action<Vector2> OnAttackStarted;
    public event Action<Vector2> OnAttackPerformed;
    public event Action<Health> OnTargetHit;

    public float Range => range;
    public float Width => width;

    private Vector2 facing = Vector2.down;
    private float nextAttackTime;

    // garante que cada alvo leve dano so uma vez por ataque
    private readonly HashSet<Health> hitThisAttack = new HashSet<Health>();

    private Vector2 Origin => origin != null ? origin.position : transform.position;

    private void OnEnable()
    {
        if (actions != null)
        {
            actions.OnCastTriggered += HandleCast;
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
            actions.OnCastTriggered -= HandleCast;
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

    private void HandleCast()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;

        // a direcao fica travada no momento do clique
        StartCoroutine(Cast(facing));
    }

    private IEnumerator Cast(Vector2 direction)
    {
        OnAttackStarted?.Invoke(direction);
        yield return new WaitForSeconds(castDelay);

        OnAttackPerformed?.Invoke(direction);

        Vector2 center = Origin + direction * range * 0.5f;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        hitThisAttack.Clear();
        foreach (var hit in Physics2D.OverlapBoxAll(center, new Vector2(range, width), angle, targetLayers))
        {
            var health = hit.GetComponentInParent<Health>();
            if (health == null || health.transform.IsChildOf(transform)) continue;
            if (!hitThisAttack.Add(health)) continue;

            health.TakeDamage(damage);
            OnTargetHit?.Invoke(health);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 dir = Application.isPlaying ? facing : Vector2.down;
        Vector3 center = Origin + dir * range * 0.5f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(range, width, 0f));
    }
}
