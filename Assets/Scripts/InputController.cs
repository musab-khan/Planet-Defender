// ===== INPUT SYSTEM (SRP: Single Responsibility for Input) =====
using UnityEngine;
using UnityEngine.Events;

public class InputController : MonoBehaviour
{
    [System.Serializable]
    public class DirectionEvent : UnityEvent<Vector2> { }

    [Header("Input Events")]
    public DirectionEvent OnDirectionChanged;

    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f;

    private Vector2 startTouchPosition;
    private bool isSwiping = false;

    void Update()
    {
        HandleKeyboardInput();
        HandleTouchInput();
    }

    private void HandleKeyboardInput()
    {
        Vector2 direction = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2.right;

        if (direction != Vector2.zero)
            OnDirectionChanged?.Invoke(direction);
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isSwiping)
                    {
                        Vector2 endPosition = touch.position;
                        Vector2 swipeVector = endPosition - startTouchPosition;

                        if (swipeVector.magnitude >= minSwipeDistance)
                        {
                            Vector2 direction = GetSwipeDirection(swipeVector);
                            OnDirectionChanged?.Invoke(direction);
                        }
                    }
                    isSwiping = false;
                    break;
            }
        }

        // Mouse support for editor testing
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            Vector2 endPosition = Input.mousePosition;
            Vector2 swipeVector = endPosition - startTouchPosition;

            if (swipeVector.magnitude >= minSwipeDistance)
            {
                Vector2 direction = GetSwipeDirection(swipeVector);
                OnDirectionChanged?.Invoke(direction);
            }
            isSwiping = false;
        }
    }

    private Vector2 GetSwipeDirection(Vector2 swipeVector)
    {
        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            return swipeVector.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return swipeVector.y > 0 ? Vector2.up : Vector2.down;
        }
    }
}