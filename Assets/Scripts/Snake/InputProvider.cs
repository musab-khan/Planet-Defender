using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class Vector2Event : UnityEvent<Vector2> { }

public class InputProvider : MonoBehaviour
{
    public Vector2Event OnDirectionInput;

    // Swipe detection
    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f;
    public float maxSwipeTime = 0.5f;

    private Vector2 touchStartPos;
    private float touchStartTime;


    void Update()
    {
        // Keyboard (Arrow keys or WASD)
        if (Keyboard.current != null)
        {
            Vector2 kb = Vector2.zero;
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) kb.x = -1;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) kb.x = 1;
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) kb.y = 1;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) kb.y = -1;

                OnDirectionInput?.Invoke(kb.normalized);
        }

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    touchStartPos = Mouse.current.position.ReadValue();
                    touchStartTime = Time.time;
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    Vector2 endPos = Mouse.current.position.ReadValue();
                    float dt = Time.time - touchStartTime;
                    Vector2 delta = endPos - touchStartPos;
                    if (delta.magnitude >= minSwipeDistance && dt <= maxSwipeTime)
                    {
                        OnDirectionInput?.Invoke(delta.normalized);
                    }
                }
            }
    }
}
