using UnityEngine;

// placeholder enquanto nao tem arte de dash: deixa o sprite transparente durante o dash
public class DashVisualizer : MonoBehaviour
{
    [SerializeField] private EvangelineDash dash;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField, Range(0f, 1f)] private float alpha = 0.35f;

    private float originalAlpha;

    private void OnEnable()
    {
        if (dash != null)
        {
            dash.OnDashStarted += HandleDashStarted;
            dash.OnDashEnded += HandleDashEnded;
        }
    }

    private void OnDisable()
    {
        if (dash != null)
        {
            dash.OnDashStarted -= HandleDashStarted;
            dash.OnDashEnded -= HandleDashEnded;
        }
    }

    private void HandleDashStarted(Vector2 direction)
    {
        originalAlpha = spriteRenderer.color.a;
        SetAlpha(alpha);
    }

    private void HandleDashEnded()
    {
        SetAlpha(originalAlpha);
    }

    private void SetAlpha(float value)
    {
        var color = spriteRenderer.color;
        color.a = value;
        spriteRenderer.color = color;
    }
}
