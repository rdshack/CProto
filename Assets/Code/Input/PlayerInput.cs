using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

    private bool usePhoneControls = false;

    private ButtonData atkButton;
    private ButtonData ability1Button;
    private ButtonData ability2Button;

    public Transform debugPanel;
    public Transform atkButtonPanel;
    public Transform ability1ButtonPanel;
    public Transform ability2ButtonPanel;

    private float forwardInputTimestamp = 0f;
    private float backwardInputTimestamp = 0f;

    private const float reqSwipeVelocity = 1500f;  //pixels per sec
    private const float reqSwipeDist = 80f;
    private const float reqSwipeAngle = 45f;
    private readonly Vector2 xAxis = new Vector2(1, 0);

    private const float minMoveDist = 60f;
    private const float trackDistance = 90f;

    private Vector2 swipeStartPosition;
    private float swipeStartTime;
    private int curSwipeFingerId = -1;
    private Touch curSwipeTouch;

    private float moveStartPositionX;
    private int curMoveFingerId = -1;
    private Touch curMoveTouch;

    private JoystickPositions joystickPosition;

    public delegate void SwipeHandler();

    public static event SwipeHandler SwipeLeft;
    public static event SwipeHandler SwipeRight;

    public static JoystickPositions JoystickPosition { get { return instance.joystickPosition; } }

    private static PlayerInput _instance;

    public enum JoystickPositions
    {
        Neutral,
        Left,
        Right
    }

    private struct ButtonData
    {
        public int downFrame;
        public bool isPressed;
    }

    public static PlayerInput instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PlayerInput>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        atkButton = new ButtonData();
        atkButton.isPressed = false;

        ability1Button = new ButtonData();
        ability1Button.isPressed = false;

        ability2Button = new ButtonData();
        ability2Button.isPressed = false;

#if UNITY_EDITOR
        Debug.Log("Unity Editor");
#else
        Debug.Log("Any other platform");
        usePhoneControls = true;
        debugPanel.gameObject.SetActive(true);
        atkButtonPanel.gameObject.SetActive(true);
        ability1ButtonPanel.gameObject.SetActive(true);
        ability2ButtonPanel.gameObject.SetActive(true);
#endif


    }

    void Update()
    {
        bool swipeActive = false;
        bool moveActive = false;
        foreach (Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began)
            {
                if ((t.position.x / Screen.width) > 0.4f)
                {
                    curSwipeFingerId = t.fingerId;
                    curSwipeTouch = t;
                    swipeStartPosition = t.position;
                    swipeStartTime = Time.time;
                    swipeActive = true;
                }
                else
                {
                    curMoveFingerId = t.fingerId;
                    curMoveTouch = t;
                    moveStartPositionX = t.position.x;
                    moveActive = true;
                }
            }

            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                if (t.fingerId == curSwipeFingerId)
                {
                    curSwipeFingerId = -1;
                }
                if (t.fingerId == curMoveFingerId)
                {
                    curMoveFingerId = -1;
                }
            }

            if (t.fingerId == curSwipeFingerId)
            {
                curSwipeTouch = t;
                swipeActive = true;
            }
            else if (t.fingerId == curMoveFingerId)
            {
                curMoveTouch = t;
                moveActive = true;
            }
        }


        if (swipeActive)
        {
            ProcessSwipe();
        }

        if (moveActive)
        {
            ProcessMove();
        }
        else
        {
            joystickPosition = JoystickPositions.Neutral;
        }

        if (Input.GetKeyDown("d"))
            forwardInputTimestamp = Time.time;

        if (Input.GetKeyDown("a"))
            backwardInputTimestamp = Time.time;

    }

    private void ProcessMove()
    {
        UIDebug.Log("Checking movement");

        float curTouchX = curMoveTouch.position.x;
        float distance = Mathf.Abs(moveStartPositionX - curTouchX);

        if (distance > minMoveDist)
        {
            if (curTouchX > moveStartPositionX)
            {
                UIDebug.Log("MOVE RIGHT");
                joystickPosition = JoystickPositions.Right;

                if (distance > trackDistance)
                {
                    moveStartPositionX = curTouchX - trackDistance;
                }
            }
            else
            {
                UIDebug.Log("MOVE LEFT");
                joystickPosition = JoystickPositions.Left;

                if (distance > trackDistance)
                {
                    moveStartPositionX = curTouchX + trackDistance;
                }
            }
        }
        else
        {
            joystickPosition = JoystickPositions.Neutral;
        }
    }

    private void ProcessSwipe()
    {
        UIDebug.Log("Checking swipes");

        float deltaTime = Time.time - swipeStartTime;

        Vector2 endPosition = new Vector2(curSwipeTouch.position.x,
                                           curSwipeTouch.position.y);
        Vector2 swipeVector = endPosition - swipeStartPosition;

        float velocity = swipeVector.magnitude / deltaTime;

        if (velocity > reqSwipeVelocity && swipeVector.magnitude > reqSwipeDist)
        {
            swipeVector.Normalize();

            float angleOfSwipe = Vector2.Dot(swipeVector, xAxis);
            angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

            if (angleOfSwipe < reqSwipeAngle)
            {
                curSwipeFingerId = -1;
                SwipeRight();
            }
            else if ((180.0f - angleOfSwipe) < reqSwipeAngle)
            {
                curSwipeFingerId = -1;
                SwipeLeft();
            }
        }
    }

    public void PressAttackButton()
    {
        instance.atkButton.downFrame = Time.frameCount;
        instance.atkButton.isPressed = true;
    }

    public void ReleaseAttackButton()
    {
        instance.atkButton.isPressed = false;
    }

    public static bool IsAttackDown()
    {
        if (instance.usePhoneControls)
        {
            return instance.atkButton.isPressed && instance.atkButton.downFrame == Time.frameCount;
        }
        else
        {
            return Input.GetMouseButtonDown(0);
        }
        
    }

    public static bool IsAttackPressed()
    {
        if (instance.usePhoneControls)
        {
            return instance.atkButton.isPressed;
        }
        else
        {
            return Input.GetMouseButton(0);
        }
    }

    public void PressAbil1Button()
    {
        instance.ability1Button.downFrame = Time.frameCount;
        instance.ability1Button.isPressed = true;
    }

    public void ReleaseAbil1Button()
    {
        instance.ability1Button.isPressed = false;
    }

    public static bool IsAbil1Down()
    {
        if (instance.usePhoneControls)
        {
            return instance.ability1Button.isPressed && instance.ability1Button.downFrame == Time.frameCount;
        }
        else
        {
            return Input.GetKeyDown(KeyCode.Q);
        }

    }

    public static bool IsAbil1Pressed()
    {
        if (instance.usePhoneControls)
        {
            return instance.ability1Button.isPressed;
        }
        else
        {
            return Input.GetKey(KeyCode.Q);
        }
    }

    public void PressAbil2Button()
    {
        instance.ability2Button.downFrame = Time.frameCount;
        instance.ability2Button.isPressed = true;
    }

    public void ReleaseAbil2Button()
    {
        instance.ability2Button.isPressed = false;
    }

    public static bool IsAbil2Down()
    {
        if (instance.usePhoneControls)
        {
            return instance.ability2Button.isPressed && instance.ability2Button.downFrame == Time.frameCount;
        }
        else
        {
            return Input.GetKeyDown(KeyCode.T);
        }

    }

    public static bool IsAbil2Pressed()
    {
        if (instance.usePhoneControls)
        {
            return instance.ability2Button.isPressed;
        }
        else
        {
            return Input.GetKey(KeyCode.Q);
        }
    }


    public static JoystickPositions GetInputDirection()
    {
        if ((Input.GetKey(KeyCode.D) && instance.HasForwardPriority()) || PlayerInput.JoystickPosition == PlayerInput.JoystickPositions.Right)
        {
            return JoystickPositions.Right;
        }
        else if ((Input.GetKey(KeyCode.A) && instance.HasBackwardPriority()) || PlayerInput.JoystickPosition == PlayerInput.JoystickPositions.Left)
        {
            return JoystickPositions.Left;
        }

        return JoystickPositions.Neutral;
    }

    //have I attempted forward motion most recently?
    private bool HasForwardPriority()
    {
        return forwardInputTimestamp > backwardInputTimestamp || !Input.GetKey(KeyCode.A);
    }

    //have I attempted back motion most recently?
    private bool HasBackwardPriority()
    {
        return backwardInputTimestamp > forwardInputTimestamp || !Input.GetKey(KeyCode.D);
    }

}
