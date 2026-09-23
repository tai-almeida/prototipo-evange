using System.Collections;
using UnityEngine;

// placeholder enquanto nao tem o asset do feitico: desenha a faixa do ataque por alguns instantes
[RequireComponent(typeof(LineRenderer))]
public class LineAttackVisualizer : MonoBehaviour
{
    [SerializeField] private LineAttack attack;
    [SerializeField] private Color color = new Color(0.6f, 0.3f, 1f, 0.6f);
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private int sortingOrder = 10;

    private LineRenderer line;
    private Coroutine showing;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.sortingOrder = sortingOrder;
        line.enabled = false;
    }

    private void OnEnable()
    {
        if (attack != null) attack.OnAttackPerformed += HandleAttackPerformed;
    }

    private void OnDisable()
    {
        if (attack != null) attack.OnAttackPerformed -= HandleAttackPerformed;
    }

    private void HandleAttackPerformed(Vector2 direction)
    {
        Vector3 start = transform.position;
        line.SetPosition(0, start);
        line.SetPosition(1, start + (Vector3)(direction * attack.Range));
        line.startWidth = line.endWidth = attack.Width;
        line.startColor = line.endColor = color;

        if (showing != null) StopCoroutine(showing);
        showing = StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        line.enabled = true;
        yield return new WaitForSeconds(duration);
        line.enabled = false;
    }
}
