using System;
using System.Collections.Generic;
using UnityEngine;

// aplica dano em todo Health dentro do raio do cone, ao receber o evento de ataque
public class ConeAttack : MonoBehaviour
{
    [SerializeField] private EvangelineActions actions;
    [SerializeField] private EvangelineMovement movement;
    [SerializeField] private Transform origin;

    [SerializeField] private float range = 15f;
    [SerializeField, Range(1f, 360f)] private float angle = 90f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private LayerMask targetLayers = ~0;

    public event Action<Vector2> OnAttackPerformed;
    public event Action<Health> OnTargetHit;

    public float Range => range;
    public float Angle => angle;

    private Vector2 facing = Vector2.down;
    private float nextAttackTime;


    // garantir que cada alvo leve dano so uma vez por ataque usando hash set
    private readonly HashSet<Health> hitThisAttack = new HashSet<Health>();
    

    private Vector2 Origin => origin != null ? origin.position : transform.position;

    private void OnEnable()
    {
        if (actions != null)
        {
            actions.OnAttackTriggered += HandleAttack;
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
            actions.OnAttackTriggered -= HandleAttack;
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

    private void HandleAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;

        OnAttackPerformed?.Invoke(facing);

        hitThisAttack.Clear();
        foreach (var hit in Physics2D.OverlapCircleAll(Origin, range, targetLayers))
        {
            /*
            ataca todos os healths no rnange selecionado
            verifica se a saude ja eh nula ou se eh filha de outro elemento
            adiciona o ataque ao hashset para nao causar dano de novo no mesmo elemento em um mesmo ataque
            */
            var health = hit.GetComponentInParent<Health>();
            if (health == null || health.transform.IsChildOf(transform)) continue;
            if (!hitThisAttack.Add(health)) continue;
            Vector2 toTarget = hit.ClosestPoint(Origin) - Origin;
            if (Vector2.Angle(facing, toTarget) > angle * 0.5f) continue;

            health.TakeDamage(damage);
            OnTargetHit?.Invoke(health);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 dir = Application.isPlaying ? facing : Vector2.down;
        Vector3 center = Origin;
        Vector3 left = Quaternion.Euler(0, 0, angle * 0.5f) * dir * range;
        Vector3 right = Quaternion.Euler(0, 0, -angle * 0.5f) * dir * range;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, center + left);
        Gizmos.DrawLine(center, center + right);

        const int segments = 16;
        Vector3 previous = center + right;
        for (int i = 1; i <= segments; i++)
        {
            Vector3 point = center + Quaternion.Euler(0, 0, -angle * 0.5f + angle * i / segments) * dir * range;
            Gizmos.DrawLine(previous, point);
            previous = point;
        }
    }
}
