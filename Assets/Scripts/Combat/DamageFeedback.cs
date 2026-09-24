using System.Collections;
using UnityEngine;

// placeholder visual de dano: pisca o sprite e mostra o numero do dano subindo
public class DamageFeedback : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Color deadColor = Color.gray;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float popupDuration = 0.6f;

    private Color originalColor;

    private void Awake()
    {
        originalColor = spriteRenderer.color;
    }

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
        StartCoroutine(Flash());
        StartCoroutine(Popup(amount));
    }

    private void HandleDied()
    {
        spriteRenderer.color = deadColor;
    }

    private IEnumerator Flash()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = health.IsDead ? deadColor : originalColor;
    }

    private IEnumerator Popup(int amount)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var popup = new GameObject("DamagePopup");
        popup.transform.position = spriteRenderer.bounds.center + Vector3.up * spriteRenderer.bounds.extents.y;

        var text = popup.AddComponent<TextMesh>();
        text.text = "-" + amount;
        text.font = font;
        text.fontSize = 64;
        text.characterSize = 0.05f;
        text.anchor = TextAnchor.MiddleCenter;
        var textRenderer = popup.GetComponent<MeshRenderer>();
        textRenderer.material = font.material;
        textRenderer.sortingOrder = 20;

        for (float t = 0; t < popupDuration; t += Time.deltaTime)
        {
            popup.transform.position += Vector3.up * Time.deltaTime;
            text.color = new Color(1f, 1f, 1f, 1f - t / popupDuration);
            yield return null;
        }
        Destroy(popup);
    }
}
