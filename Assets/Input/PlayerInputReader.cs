using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    private BeyondYourHandsInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 AimScreenPosition { get; private set; }
    public bool HasAimInput { get; private set; }

    public event Action OnJumpPressed;
    public event Action OnThrowPressed;
    public event Action OnRecallPressed;

    private void Awake()
    {
        inputActions = new BeyondYourHandsInputActions();
    }

    private void OnEnable()
    {
        inputActions.Gameplay.Enable();

        inputActions.Gameplay.Move.performed += HandleMovePerformed;
        inputActions.Gameplay.Move.canceled += HandleMoveCanceled;

        inputActions.Gameplay.Aim.performed += HandleAimPerformed;
        inputActions.Gameplay.Aim.canceled += HandleAimCanceled;

        inputActions.Gameplay.Jump.performed += HandleJumpPerformed;
        inputActions.Gameplay.Throw.performed += HandleThrowPerformed;
        inputActions.Gameplay.Recall.performed += HandleRecallPerformed;
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Move.performed -= HandleMovePerformed;
        inputActions.Gameplay.Move.canceled -= HandleMoveCanceled;

        inputActions.Gameplay.Aim.performed -= HandleAimPerformed;
        inputActions.Gameplay.Aim.canceled -= HandleAimCanceled;

        inputActions.Gameplay.Jump.performed -= HandleJumpPerformed;
        inputActions.Gameplay.Throw.performed -= HandleThrowPerformed;
        inputActions.Gameplay.Recall.performed -= HandleRecallPerformed;

        inputActions.Gameplay.Disable();
    }

    private void HandleMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void HandleMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }

    private void HandleAimPerformed(InputAction.CallbackContext context)
    {
        AimScreenPosition = context.ReadValue<Vector2>();
        HasAimInput = true;
    }

    private void HandleAimCanceled(InputAction.CallbackContext context)
    {
        AimScreenPosition = Vector2.zero;
        HasAimInput = false;
    }

    private void HandleJumpPerformed(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
    }

    private void HandleThrowPerformed(InputAction.CallbackContext context)
    {
        OnThrowPressed?.Invoke();
    }

    private void HandleRecallPerformed(InputAction.CallbackContext context)
    {
        OnRecallPressed?.Invoke();
    }
}
