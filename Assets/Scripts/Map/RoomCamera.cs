using System;
using UnityEngine;

// camera presa na sala atual: segue a Evangeline mas nao mostra fora dos limites da sala.
// so troca de sala quando o RoomManager avisa pelo evento
[RequireComponent(typeof(Camera))]
public class RoomCamera : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothTime = 0.1f;
    [SerializeField] private float transitionSmoothTime = 0.35f;

    public event Action<Room> OnTransitionStarted;
    public event Action<Room> OnTransitionFinished;

    private Camera cam;
    private Room currentRoom;
    private Vector3 velocity;
    private bool transitioning;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (roomManager != null)
        {
            roomManager.OnCurrentRoomChanged += HandleRoomChanged;
        }
    }

    private void OnDisable()
    {
        if (roomManager != null)
        {
            roomManager.OnCurrentRoomChanged -= HandleRoomChanged;
        }
    }

    private void HandleRoomChanged(Room previous, Room current)
    {
        currentRoom = current;

        // primeira sala do jogo: pula direto, sem animacao
        if (previous == null)
        {
            transform.position = TargetPosition();
            return;
        }

        transitioning = true;
        OnTransitionStarted?.Invoke(current);
    }

    private void LateUpdate()
    {
        if (target == null || currentRoom == null) return;

        Vector3 goal = TargetPosition();
        float smoothTime = transitioning ? transitionSmoothTime : followSmoothTime;
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);

        if (transitioning && (transform.position - goal).sqrMagnitude < 0.01f)
        {
            transitioning = false;
            OnTransitionFinished?.Invoke(currentRoom);
        }
    }

    // posicao da Evangeline limitada aos cantos da sala; se a sala couber na tela, fica centralizada
    private Vector3 TargetPosition()
    {
        Bounds room = currentRoom.Bounds;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 pos = target.position;
        pos.x = ClampAxis(pos.x, room.min.x + halfWidth, room.max.x - halfWidth, room.center.x);
        pos.y = ClampAxis(pos.y, room.min.y + halfHeight, room.max.y - halfHeight, room.center.y);
        pos.z = transform.position.z;
        return pos;
    }

    private static float ClampAxis(float value, float min, float max, float center)
    {
        return min > max ? center : Mathf.Clamp(value, min, max);
    }
}
