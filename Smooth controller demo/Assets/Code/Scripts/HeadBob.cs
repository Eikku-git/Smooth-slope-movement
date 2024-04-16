using System;
using System.Collections;
using UnityEngine;

public class HeadBob : MonoBehaviour {

    [SerializeField] AnimationCurve Breathing;
    [SerializeField] private float breathingMagnitude;
    [SerializeField] AnimationCurve Bob;
    [SerializeField] float bobMagnitude;
    [SerializeField] FuncEvent bobEvent;
    private bool bobbed = false;
    [SerializeField] AnimationCurve Sway;
    [SerializeField] float swayMagnitude;
    [SerializeField] AnimationCurve cameraRotationLerpCurve;
    [SerializeField] float cameraRotationMaxAngle;
    private Quaternion leftRotation;
    private Quaternion rightRotation;
    private Quaternion[] rotations;

    private float defaultHeadHeight;

    private float cameraRotationLerp = 0.5f;
    private float cameraRotationReleaseTime;

    private float bobLerpValue = 0;
    private float swayLerpValue = 0; 

    private float breathCurvePoint = 0;
    private float bobCurvePoint = 0;
    private float swayCurvePoint = 0;

    private void Start() {
        defaultHeadHeight = PlayerCamera.main.transform.localPosition.y;
        leftRotation = Quaternion.AngleAxis(cameraRotationMaxAngle, Vector3.forward);
        rightRotation = Quaternion.AngleAxis(cameraRotationMaxAngle, Vector3.back);
        rotations = new[] { leftRotation, rightRotation };
    }

    private void Update() {
        EvaluateCurves();
    }

    private void EvaluateCurves() {
        float bob = defaultHeadHeight + Bob.Evaluate(bobCurvePoint) * bobMagnitude;
        float sway = Sway.Evaluate(swayCurvePoint) * swayMagnitude;
        float breathing = defaultHeadHeight + Breathing.Evaluate(breathCurvePoint) * breathingMagnitude;
        PlayerCamera.localPosition = new Vector3(Mathf.Lerp(0, sway, swayLerpValue), Mathf.Lerp(breathing, bob, bobLerpValue), 0);
        PlayerCamera.localRotation = Quaternion.Slerp(leftRotation, rightRotation, cameraRotationLerp);
    }

    public void ReturnToBreathing() {
        BreathUpdate();
        LerpTransition(ref bobLerpValue, -1, 4f);
        LerpTransition(ref swayLerpValue, -1, 2f);
        swayCurvePoint = swayLerpValue != 0 ? swayCurvePoint : 0;
        bobCurvePoint = bobLerpValue != 0 ? bobCurvePoint : 0;
        cameraRotationReleaseTime += Time.deltaTime * 2;
        cameraRotationLerp = Mathf.Lerp(cameraRotationLerp, 0.5f, cameraRotationReleaseTime);
    }

    public void BreathUpdate() {
        if (breathCurvePoint < 1) {
            breathCurvePoint += Time.deltaTime * 0.75f;
        }
        else {
            breathCurvePoint = 0;
        }
    }

    public void HeadBobUpdate(Vector3 movementVector) {
        LerpTransition(ref breathCurvePoint, -1, 1.0f);
        LerpTransition(ref bobLerpValue, 1, 5.0f);
        if (bobCurvePoint < 2) {
            bobCurvePoint += Time.deltaTime * 4;
            if (!bobbed && Bob.Evaluate(bobCurvePoint) > bobMagnitude * 0.99f) {
                bobEvent.Invoke();
                bobbed = true;
            }
        }
        else {
            bobCurvePoint = 0;
            bobbed = false;
        }
        if (movementVector.x != 0 && movementVector.z != 0) {
            HeadSwayUpdate(movementVector);
            int dir = (int)movementVector.x;
            cameraRotationLerp = Math.Clamp(
                cameraRotationLerp + Time.deltaTime * dir * Quaternion.Angle(PlayerCamera.main.transform.localRotation, rotations[(dir + 1) / 2]) * 0.25f, 
                0, 
                1);
            cameraRotationReleaseTime = 0;
            LerpTransition(ref swayLerpValue, 1, 1.0f);
        }
        else {
            LerpTransition(ref swayLerpValue, -1, 2.0f);
            cameraRotationReleaseTime += Time.deltaTime * 2;
            cameraRotationLerp = Mathf.Lerp(cameraRotationLerp, 0.5f, cameraRotationReleaseTime);
            if (swayLerpValue == 0) {
                swayCurvePoint = 0;
            }
        }
    }

    public void HeadSwayUpdate(Vector3 movementVector) {
        swayCurvePoint += Time.deltaTime * movementVector.x;
    }

    private void LerpTransition(ref float lerpValue, int dir, float timeMult) {
        if (lerpValue >= 0 && lerpValue <= 1) {
            lerpValue += dir * Time.deltaTime * timeMult;
        }
        else {
            lerpValue = Mathf.Clamp(lerpValue, 0, 1);
        }
    }
}
