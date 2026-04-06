using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRControllerRecognitionTest : MonoBehaviour
{
    private InputDevice left;
    private InputDevice right;

    void Update()
    {
        left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        Debug.Log($"Left valid: {left.isValid} | Right valid: {right.isValid}");

        DumpDevice("LEFT", left);
        DumpDevice("RIGHT", right);

        // var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        // var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (left.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos))
            Debug.Log("Left Pos: " + leftPos);

        if (right.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos))
            Debug.Log("Right Pos: " + rightPos);
    }

    void DumpDevice(string label, InputDevice device)
    {
        if (!device.isValid)
            return;

        if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
            Debug.Log($"{label} Position: {pos}");

        if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
            Debug.Log($"{label} Rotation: {rot.eulerAngles}");

        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            Debug.Log($"{label} primary2DAxis: {axis}");

        if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            Debug.Log($"{label} trigger value: {triggerValue}");

        if (device.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
            Debug.Log($"{label} grip value: {gripValue}");

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed))
            Debug.Log($"{label} triggerButton: {triggerPressed}");

        if (device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed))
            Debug.Log($"{label} gripButton: {gripPressed}");

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton))
            Debug.Log($"{label} primaryButton: {primaryButton}");

        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton))
            Debug.Log($"{label} secondaryButton: {secondaryButton}");

        if (device.TryGetFeatureValue(CommonUsages.menuButton, out bool menuButton))
            Debug.Log($"{label} menuButton: {menuButton}");
    }
}