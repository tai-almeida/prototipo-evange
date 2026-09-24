using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EvangelineMovement : MonoBehaviour
{
    // esse script cuida apenas da leitura do inputo e dispara um evento para informar a direcao e velocidade
    [SerializeField] private float moveSpeed = 15f;

    public event Action<Vector2> OnMoveVectorChanged;

    private Rigidbody2D rigidbody;
    private Vector2 moveInput;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            moveY += 1f;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            moveY -= 1f;
        } 
        if (Keyboard.current.aKey.isPressed)
        {
            moveX -= 1f;
        } 
        if (Keyboard.current.dKey.isPressed)
        {
            moveX += 1f;
        } 

        Vector2 newInput = new Vector2(moveX, moveY).normalized;

        if (newInput != moveInput)
        {
            moveInput = newInput;
            OnMoveVectorChanged?.Invoke(moveInput);
        }
    }

    private void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position 
                                + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

}

