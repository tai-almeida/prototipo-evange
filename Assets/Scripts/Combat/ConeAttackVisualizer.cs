using System.Collections;
using UnityEngine;

//placeholder enquanto nao tem os assets dos ataques com vassoura
public class ConeAttackVisualizer : MonoBehaviour
{
    [SerializeField] private ConeAttack attack;
    [SerializeField] private Color color = new Color(1f, 0.85f, 0.2f, 0.45f);
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private int sortingOrder = 10;
    [SerializeField] private int segments = 16;

    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Coroutine showing;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = sortingOrder;
        meshRenderer.enabled = false;
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
        BuildCone(attack.Range, attack.Angle);
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        if (showing != null) StopCoroutine(showing);
        showing = StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        meshRenderer.enabled = true;
        yield return new WaitForSeconds(duration);
        meshRenderer.enabled = false;
    }

    // leque apontando para +X; a rotacao do transform aponta para a direcao do ataque
    private void BuildCone(float range, float angle)
    {
        var vertices = new Vector3[segments + 2];
        var colors = new Color[segments + 2];
        var triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float a = (-angle * 0.5f + angle * i / segments) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * range;
        }
        for (int i = 0; i < colors.Length; i++) colors[i] = color;
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }
}
