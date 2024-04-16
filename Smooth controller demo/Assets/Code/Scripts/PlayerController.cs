using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    [SerializeField] private bool[] layerMask;
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] public float mouseSensitivity;
    private int layerMaskInt;
    private InputAction lookAction = new InputAction(
        type: InputActionType.PassThrough,
        binding: "<Mouse>/delta"
    );
    private InputActionMap moveAction;
    private OrderedDictionary movementInputValues;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float forwardCheckDistance;
    private Vector3 currentCameraRotation;
    private Rigidbody rb;
    private HeadBob headBob;
    
    public static PlayerController main;

    private float timeLeftInMovementPause = 0;

    public PlayerController() {
        lookAction.performed += lookAction_performed;
    }

    private void Awake() {
        if (!TryGetComponent(out rb)) { Debug.LogWarning("No Rigidbody attached to Player"); }
        if (!TryGetComponent(out headBob)) { Debug.LogWarning("No HeadBob script attached to Player"); }
        if (main != null) { Debug.LogWarning("Multiple PlayerControllers in the scene!"); }
        else { main = this; }
        movementInputValues = new(4) {
            { "Forward", 0.0f },
            { "Backward", 0.0f },
            { "Right", 0.0f },
            { "Left", 0.0f }
        };
        layerMaskInt = Utility.BitToInt(layerMask);
        UpdateAllBindings();
        UpdateSensitivity(mouseSensitivity);
        EnableAllInput();
    }

    public void UpdateAllBindings() {
        UpdateMoveBindings();
    }

    public void UpdateMoveBindings() {

        moveAction = new InputActionMap("Movement");

        moveAction.AddAction(
            name: "Forward",
            type: InputActionType.Value,
            binding: "<Keyboard>/w" 
        );
        moveAction.AddAction(
            name: "Backward",
            type: InputActionType.Value,
            binding: "<Keyboard>/s" 
        );
        moveAction.AddAction(
            name: "Right",
            type: InputActionType.Value,
            binding: "<Keyboard>/d" 
        );
        moveAction.AddAction(
            name: "Left",
            type: InputActionType.Value,
            binding: "<Keyboard>/a" 
        );
        moveAction.actionTriggered += moveInput_performed;
    }

    public void UpdateSensitivity(float sensitivity) {
        mouseSensitivity = sensitivity;
    }

    public void EnableAllInput() {
        EnableLooking();
        EnableMovement();
    }

    public void DisableAllInput() {
        DisableLooking();
        DisableMovement();
    }

    public void EnableMovement() {
        if (timeLeftInMovementPause > 0) {
            StartCoroutine(PausingMovement(timeLeftInMovementPause));
        }
        else {
            moveAction.Enable();
        }
    }

    public void DisableMovement() {
        moveAction.Disable();
    }

    public void EnableLooking() {
        lookAction.Enable();
    }

    public void DisableLooking() {
        lookAction.Disable();
    }

    public void PauseMovementForSeconds(float time) {
        if (timeLeftInMovementPause != 0) {
            Debug.LogWarning("Overlapping PlayerController movement pausing attempts!");
            return;
        }
        timeLeftInMovementPause = 0;
        StartCoroutine(PausingMovement(time));
    }

    public IEnumerator PausingMovement(float time) {
        float timer = 0;
        DisableMovement();
        while (timer < time) {
            timer += Time.deltaTime;
            timeLeftInMovementPause = time - timer;
            yield return null;
        }
        timeLeftInMovementPause = 0;
        EnableMovement();
    }

    private void lookAction_performed(InputAction.CallbackContext context) {
        Vector2 inputVector = context.ReadValue<Vector2>();
        Vector3 inverseRotVector = mouseSensitivity * Time.deltaTime * (Vector3)inputVector;
        currentCameraRotation += new Vector3(-inverseRotVector.y, 0, 0);
        transform.localEulerAngles += new Vector3(0, inverseRotVector.x, 0);
        //currentCameraRotation.x = Mathf.Clamp(currentCameraRotation.x, -10, 10);
        PlayerCamera.lookRotation = Quaternion.Euler(currentCameraRotation);
    }

    public void NudgeRot(Vector3 rotationVect) {
        currentCameraRotation.x -= rotationVect.x;
        PlayerCamera.main.transform.localEulerAngles = currentCameraRotation;
    }

    private void moveInput_performed(InputAction.CallbackContext context) {
        movementInputValues[context.action.name] = context.ReadValue<float>();
    }

    private Vector2[] restrictVectors = new Vector2[] {
        new(0, 1f),
        new(1f, 0),
        new(0, -1f),
        new(-1f, 0)
    };

    private void RestrictMovement(ref Vector3 movementVect, int index) {
        if (movementVect.x == restrictVectors[index].x) {
            movementVect.x = 0;
        }
        if (movementVect.z == restrictVectors[index].y) {
            movementVect.z = 0;
        }
    }

    Ray groundCheckRay = new();
    private Vector3 constructMovementVector() {
        Vector3 movementVector = 
            new((float)movementInputValues[2] - (float)movementInputValues[3], 
            0, (float)movementInputValues[0] - (float)movementInputValues[1]); // construct vector from input values
        if (movementVector == Vector3.zero) {
            headBob.ReturnToBreathing();
            return Vector3.zero;
        }
        for (int i = 0; i < 4; i++) {
            Ray ray = new Ray(transform.position + Vector3.up, Quaternion.Euler(0, 90 * i, 0) * transform.forward);
            if (Physics.Raycast(ray, forwardCheckDistance, layerMaskInt)) {
                RestrictMovement(ref movementVector, i);
                if (movementVector == Vector3.zero) {
                    headBob.ReturnToBreathing();
                    return Vector3.zero;
                }
            }
        }
        groundCheckRay.origin = transform.position;
        groundCheckRay.direction = Vector3.down;
        Vector3 groundNormal = Vector3.up;
        if (Physics.Raycast(groundCheckRay, out RaycastHit groundCheckHit, groundCheckDistance, layerMaskInt)) {
            groundNormal = groundCheckHit.normal;
            headBob.HeadBobUpdate(movementVector);
        }
        else {
            headBob.ReturnToBreathing();
        }
        float moveSpeed = baseMoveSpeed;
        if (movementVector.x != 0) {
            moveSpeed *= movementVector.z >= 0 ? 1.2f : 1.5f;
        }
        movementVector = movementVector.normalized;
        movementVector = transform.TransformDirection(movementVector);
        movementVector = Quaternion.FromToRotation(Vector3.up, groundNormal) * movementVector;
        Debug.DrawRay(groundCheckHit.point, movementVector * 5);
        movementVector = moveSpeed * Time.fixedDeltaTime * movementVector;
        return movementVector;
    }

    private void FixedUpdate() {
        rb.MovePosition(rb.position + constructMovementVector());
    }
}