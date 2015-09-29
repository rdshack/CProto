using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(BaseController))]
public class GirlController : MonoBehaviour {

	private Animator animator;

    public  BaseController controller;

    public float timescale = 1f;
    public float enemyOverlapSlow = 0.7f;
    public bool useAutoJump = false;

    private float autoJumpDetectOffsetX = 0.7f;
    private float autoJumpDepthTest = 1f;

    private BaseAction currentAction;
    private BaseAction nextAction;
    private Combo comboAction;
    private Dash dashAction;
    private Block blockAction;
    private Slam slamAction;
    private Tornado tornadoAction;
    private BaseAction airFloatAction;
    private BaseAction airSpecialAction;

    //events for abilities
    public delegate void JumpPadHandler();
    public event JumpPadHandler OnJumpPadActivate;


    //animation params


    public Vector3 ColliderPos { get { return controller.ColliderPos; } }
    public float ColliderHeight { get { return controller.ColliderHeight; } }
    public float ColliderRadius { get { return controller.ColliderRadius; } }
    public bool IsAirborne { get { return controller.IsAirborne; } }

	void Start()
	{
        Time.timeScale = timescale;
        controller = gameObject.GetComponent<BaseController>();
        animator = GetComponentInChildren<Animator>();

        comboAction = GetComponent<Combo>();
        dashAction = GetComponent<Dash>();
        blockAction = GetComponent<Block>();
        airFloatAction = GetComponent<AirFloat>();
        slamAction = GetComponent<Slam>();
        tornadoAction = GetComponent<Tornado>();

        if (GetComponent<AirAttack>() != null)
        {
            airSpecialAction = GetComponent<AirAttack>();
        }
        else if (GetComponent<AirSpecial>())
        {
            airSpecialAction = GetComponent<AirSpecial>();
        }

        controller.OnLand += HandleLanding;

        PlayerInput.SwipeLeft += SwipeLeft;
        PlayerInput.SwipeRight += SwipeRight;
	}


    void Update()
    {
        //Handle Action input (combo, abilities, etc)
        HandleActionInput();

        //Process action queue
        HandleCurrentAction();

        //Handle Movement input
        HandleMoveInput();

        //Process character movement
        HandleCharacterMotion();

        //Apply enemy overlap slow
        HandleOverlapSlow();

        if (!IsAirborne && DetectAutoJumpSpace() && (currentAction == null || currentAction.AllowAutoJump()))
        {
            OnJumpPadActivate();
            animator.Play(HashLookup.startJumpHash, 0, 0.25f);

            if (currentAction == dashAction)
            {
                controller.CurExternalVeloX = 6f * controller.FacingValue;
                controller.CurInfluenceX = controller.groundMaxInfluenceX * controller.FacingValue;
                controller.CurExternalVeloY = 5f;
            }
            else
            {
                controller.CurExternalVeloX = 3f * controller.FacingValue;
                controller.CurExternalVeloY = 5f;
            }
           
        }

    }

    private void HandleOverlapSlow()
    {
        float pointOffset = controller.ColliderHeight / 2;
        Vector3 p1 = ColliderPos + new Vector3(0, pointOffset, 0);
        Vector3 p2 = ColliderPos + new Vector3(0, -pointOffset, 0);

        if (Physics.CheckCapsule(p1, p2, controller.ColliderRadius, HashLookup.EnemiesMask))
        {
            controller.MaxInfluenceMultiplier = enemyOverlapSlow;
        }
        else
        {
            controller.MaxInfluenceMultiplier = 1f;
        }
    }

    void FixedUpdate()
    {
        HandleAirborneLogic();
    }

    //normal movement
    private void HandleCharacterMotion()
    {
        if (currentAction != null && currentAction.UseRootMotion())
        {
            if (!DetectForwardAirSpace())
            {
                int mask = (1 << HashLookup.playerLayer) | (1 << HashLookup.enemyLayer);
                controller.Move(new Vector3(animator.deltaPosition.x, 0, 0), mask, true);
            }
        }
    }


    private void HandleAirborneLogic()
    {
        if (controller.IsAirborne)
        {
            animator.SetBool(HashLookup.isAirborneHash, true);
            animator.SetBool(HashLookup.liftLegsHash, true);
        }
        else
        {
            animator.SetBool(HashLookup.isAirborneHash, false);
        }
    }

    private void HandleMoveInput()
    {
        controller.MoveDir = BaseController.MoveDirection.None;

        if (PlayerInput.GetInputDirection() == PlayerInput.JoystickPositions.Right)
        {
            controller.MoveDir = BaseController.MoveDirection.Forward;

            if (CanTurn())
            {
                controller.FacingForward = true;
            }
        }

        if (PlayerInput.GetInputDirection() == PlayerInput.JoystickPositions.Left)
        {
            controller.MoveDir = BaseController.MoveDirection.Backward;

            if (CanTurn())
            {
                controller.FacingForward = false;
            }
        }     

        if (currentAction != null && currentAction.IsActive() && !currentAction.AllowPlayerMotion())
        {
            controller.MoveDir = BaseController.MoveDirection.None;

            if (!IsAirborne)
            {
                controller.CurInfluenceX = 0;
            }
        }
            
    }


    private void QueueAction(BaseAction action)
    {

        if (action == null)
            return;

        if (currentAction == null && action.CooldownReady() && !action.IsDisabled())
        {
            currentAction = action;
            nextAction = null;
            currentAction.Execute();
        }
        else if (action.IsQueueable())
        {
            if (currentAction == action && currentAction.IsMultiStep())
            {
                currentAction.QueueNextStep();
                nextAction = null;
            }
            else
            {
                nextAction = action;

                if (currentAction != null && currentAction.IsMultiStep())
                {
                    currentAction.CancelNextStep();
                }
            }   
        }
    }

    private void HandleCurrentAction()
    {
        if (currentAction == null && nextAction == null)
            return;

        if (currentAction != null && currentAction.IsActive())
        {
            currentAction.ActiveUpdate();
        }
        else if(currentAction == null || !currentAction.IsActive())
        {
            currentAction = null;

            if (nextAction != null && nextAction.CooldownReady())
            {
                if (!nextAction.IsDisabled())
                {
                    currentAction = nextAction;
                    currentAction.Execute();
                }

                nextAction = null;  
            }
        }

    }


    private void HandleActionInput()
    {

        if (!controller.IsAirborne && (PlayerInput.IsAttackDown() || (currentAction == null && PlayerInput.IsAttackPressed())))
        {
            QueueAction(comboAction);
        }

        if (controller.IsAirborne && (PlayerInput.IsAttackDown() || (currentAction == null && PlayerInput.IsAttackPressed())))
        {
            QueueAction(airSpecialAction);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            dashAction.SetDashDir(Dash.DashDirection.Neutral);
            QueueAction(dashAction);
        }

        if (PlayerInput.IsAbil2Down())
        {
            QueueAction(tornadoAction);
        }

        if (PlayerInput.IsAbil1Down())
        {
            QueueAction(slamAction);
        }
    }
    

    public void ClearNextAction()
    {
        nextAction = null;
    }


    //are we facing a ledge?
    private bool DetectForwardAirSpace()
    {
        int layerMask = 1 << HashLookup.groundLayer;
        bool groundDetected = true;

        groundDetected = Physics.CheckCapsule(transform.position + new Vector3(autoJumpDetectOffsetX * controller.FacingValue, 0, 0), transform.position + new Vector3(autoJumpDetectOffsetX * controller.FacingValue, -autoJumpDepthTest, 0), 0.1f, layerMask);

        return !groundDetected;
    }

    private bool DetectAutoJumpSpace()
    {
        int layerMask = 1 << HashLookup.groundLayer;
        bool groundDetected = true;

        float detect = autoJumpDetectOffsetX - 0.2f;
        groundDetected = Physics.CheckCapsule(transform.position + new Vector3(detect * controller.FacingValue, 0, 0), transform.position + new Vector3(autoJumpDetectOffsetX * controller.FacingValue, -autoJumpDepthTest, 0), 0.1f, layerMask);

        return !groundDetected;
    }

    private bool CanTurn()
    {
        if (currentAction != null)
        {
            return currentAction.CanTurn();
        }
        else
        {
            return true;
        }
    }

    //this method selects a target from a set of possible targets using special rules
    //possible targets:  targets we'll search through
    //obeyUserInput: if true, we only search in the direction of player input (if there is directional input)
    //searchBehind: if true, we will search behind the player after searching in front
    //preferred target:  we'll prefer to select this target (small distance handicap)
    public Transform FindTarget(Transform[] possibleTargets, bool obeyUserInput, bool searchBehind, Transform preferredTarget = null)
    {
        float preferTargetHandicap = 0.2f;
        Transform curTarget = null;

        PlayerInput.JoystickPositions inputDir = PlayerInput.GetInputDirection();

        //if the player is NOT HOLDING an input direction, we'll search his front first - then behind him if no targets are found in front
        if (!obeyUserInput || inputDir == PlayerInput.JoystickPositions.Neutral)
        {
            List<Transform> behind = new List<Transform>();
            
            foreach (Transform t in possibleTargets)
            {
                Damageable enemy = t.GetComponent<Damageable>();

                //is the target in front?
                bool inFront = (controller.FacingForward && t.transform.position.x > transform.position.x)
                                    || (!controller.FacingForward && t.transform.position.x < transform.position.x);

                if (inFront)
                {
                    if (!enemy.IsDead)
                    {
                        if (curTarget == null)
                        {
                            curTarget = t;
                        }
                        else
                        {
                            //out of all the possible targets in front, is this one the closest?
                            if (Vector3.Distance(t.position, transform.position) < Vector3.Distance(curTarget.position, transform.position))
                            {
                                curTarget = t;
                            }
                        }
                    }
                }
                //store all targets behind us
                else
                {
                    behind.Add(t);
                }
            }

            //if at least 1 target was found in front of us, we check to see if our "preferred target" should be used
            if (curTarget != null)
            {
                //is our preferred target still alive?
                bool prefIsAlive = (preferredTarget != null) && (!preferredTarget.GetComponent<Damageable>().IsDead);

                //is our preferred target in front of us?
                bool prefIsInFront = (preferredTarget != null) &&
                    ((controller.FacingForward && preferredTarget.position.x > transform.position.x) || (!controller.FacingForward && preferredTarget.position.x < transform.position.x));

                //if he is alive, and in front, and is closer (or almost closer using "handicap")- we make him the target.
                if (prefIsInFront && prefIsAlive && Vector3.Distance(preferredTarget.position, transform.position) - preferTargetHandicap < Vector3.Distance(curTarget.transform.position, transform.position))
                {
                    curTarget = preferredTarget;
                }
            }

            //if no target was found in front, we look behind the player
            else if(searchBehind)
            {
                foreach (Transform t in behind)
                {
                    Damageable enemy = t.GetComponent<Damageable>();

                    if (enemy != null && !enemy.IsDead)
                    {
                        if (curTarget == null)
                        {
                            curTarget = t;
                        }
                        else
                        {
                            //is this the closest target behind us?
                            if (Vector3.Distance(t.position, transform.position) < Vector3.Distance(curTarget.transform.position, transform.position))
                            {
                                curTarget = t;
                            }
                        }
                    }
                }
            }
        }

       //if the player IS HOLDING an input direction, we only search in that direction
        else if(obeyUserInput)
        {
            foreach (Transform t in possibleTargets)
            {
                Damageable enemy = t.GetComponent<Damageable>();

                //is the enemy position in the direction of player input?
                bool correctSide = (inputDir == PlayerInput.JoystickPositions.Right && t.transform.position.x > transform.position.x)
                                    || (inputDir == PlayerInput.JoystickPositions.Left && t.transform.position.x < transform.position.x);

                if (correctSide)
                {
                    if (!enemy.IsDead)
                    {
                        if (curTarget == null)
                        {
                            curTarget = t;
                        }
                        else
                        {
                            if (Vector3.Distance(t.position, transform.position) < Vector3.Distance(curTarget.position, transform.position))
                            {
                                curTarget = t;
                            }
                        }
                    }
                }
            }

            //was a target found in the input direction of the player?
            if (curTarget != null)
            {
                //is the preferred target still alive?
                bool preferredTargetAlive = (preferredTarget != null) && (!preferredTarget.GetComponent<Damageable>().IsDead);

                //is the preferred target in the direction of the player input?
                bool correctSide = (preferredTarget != null) &&
                    ((inputDir == PlayerInput.JoystickPositions.Right && preferredTarget.position.x > transform.position.x) 
                    || (inputDir == PlayerInput.JoystickPositions.Left && preferredTarget.position.x < transform.position.x));

                //if preferred target is alive and correct side, and is closer (using handicap) - we set it as target.
                if (correctSide && preferredTargetAlive && Vector3.Distance(preferredTarget.position, transform.position) - preferTargetHandicap < Vector3.Distance(curTarget.transform.position, transform.position))
                {
                    curTarget = preferredTarget;
                }
            }
        }

        return curTarget;
    }

    //events

    public void HandleLanding(BaseController con)
    {
       
    }

    public void ActivateJumpPad(float stength)
    {
        controller.CurExternalVeloY = stength;
        OnJumpPadActivate();
        animator.Play(HashLookup.startJumpHash, 0, 0.25f);
    }

    public void SwipeLeft()
    {
        dashAction.SetDashDir(Dash.DashDirection.Backward);
        QueueAction(dashAction);
    }

    public void SwipeRight()
    {
        dashAction.SetDashDir(Dash.DashDirection.Forward);
        controller.FacingForward = true;
        QueueAction(dashAction);
    }
}