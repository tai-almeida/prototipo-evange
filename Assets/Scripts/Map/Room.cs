using System;
using UnityEngine;
using UnityEngine.Tilemaps;

// uma sala do mapa: so detecta a Evangeline entrando/saindo e dispara eventos.
// quem precisa reagir (camera, portas, spawn de inimigos) escuta esses eventos
[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Vector2Int expectedSize = new Vector2Int(58, 34);

    public event Action<Room> OnPlayerEntered;
    public event Action<Room> OnPlayerExited;

    public Bounds Bounds => area.bounds;
    public bool HasPlayer => playerCollidersInside > 0;

    private BoxCollider2D area;

    // a Evangeline pode ter mais de um collider, entao conta quantos estao dentro
    private int playerCollidersInside;

    private void Awake()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
        FitToTilemap();
    }

    // ajusta o trigger para cobrir a sala inteira (58x34 nas salas atuais)
    private void FitToTilemap()
    {
        if (tilemap == null) tilemap = GetComponentInChildren<Tilemap>();
        if (tilemap == null) return;

        tilemap.CompressBounds();
        var cells = tilemap.cellBounds;

        // tile pintado fora da sala aumenta o trigger e faz ela "engolir" as vizinhas
        if (cells.size.x != expectedSize.x || cells.size.y != expectedSize.y)
        {
            Debug.LogWarning($"Sala {name} tem {cells.size.x}x{cells.size.y} tiles (esperado {expectedSize.x}x{expectedSize.y}). " +
                             $"Confira se tem tile pintado fora dela: {cells}", this);
        }
        Vector3 min = tilemap.CellToWorld(cells.min);
        Vector3 max = tilemap.CellToWorld(cells.max);

        area.offset = transform.InverseTransformPoint((min + max) * 0.5f);
        area.size = max - min;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        playerCollidersInside++;
        if (playerCollidersInside == 1)
        {
            OnPlayerEntered?.Invoke(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other) || playerCollidersInside == 0) return;

        playerCollidersInside--;
        if (playerCollidersInside == 0)
        {
            OnPlayerExited?.Invoke(this);
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponentInParent<EvangelineMovement>() != null;
    }

    private void Reset()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
        FitToTilemap();
    }
}
