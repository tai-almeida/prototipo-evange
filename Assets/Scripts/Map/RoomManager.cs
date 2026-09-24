using System;
using System.Collections.Generic;
using UnityEngine;

// escuta os eventos de todas as salas e decide qual eh a sala atual.
// dispara OnCurrentRoomChanged(anterior, atual) para quem precisar reagir
public class RoomManager : MonoBehaviour
{
    public event Action<Room, Room> OnCurrentRoomChanged;

    public Room CurrentRoom { get; private set; }

    private Room[] rooms;

    // salas em que a Evangeline esta encostando agora (na porta ela fica em duas ao mesmo tempo)
    private readonly List<Room> occupied = new List<Room>();

    private void Awake()
    {
        rooms = FindObjectsByType<Room>();
    }

    private void OnEnable()
    {
        foreach (var room in rooms)
        {
            room.OnPlayerEntered += HandlePlayerEntered;
            room.OnPlayerExited += HandlePlayerExited;
        }
    }

    private void OnDisable()
    {
        foreach (var room in rooms)
        {
            room.OnPlayerEntered -= HandlePlayerEntered;
            room.OnPlayerExited -= HandlePlayerExited;
        }
    }

    private void HandlePlayerEntered(Room room)
    {
        occupied.Remove(room);
        occupied.Add(room);
        SetCurrentRoom(room);
    }

    private void HandlePlayerExited(Room room)
    {
        occupied.Remove(room);

        // se ela voltou pela porta sem sair da sala anterior, a anterior volta a ser a atual
        if (room == CurrentRoom && occupied.Count > 0)
        {
            SetCurrentRoom(occupied[occupied.Count - 1]);
        }
    }

    private void SetCurrentRoom(Room room)
    {
        if (room == CurrentRoom) return;

        Room previous = CurrentRoom;
        CurrentRoom = room;
        OnCurrentRoomChanged?.Invoke(previous, room);
    }
}
