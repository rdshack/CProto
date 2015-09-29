using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Combo: BaseAction {

    [System.Serializable]
    public class ComboStep {

        //state name in animator
        public string animStateName;
        //how long to play the animation
        public float animLength = 1f;
        //time spent transitioning back to idle
        public float toIdleLength = 0.2f;
        //speed of animation
        public float playSpeed = 1f;
        //radius of aoe (0 = not aoe)
        public float aoeRadius = 0;
        //percent of damage aoe targets take
        public float aoePercent = 0;
        //special effect to play on use
        public Transform impactEffectPrefab;

        [HideInInspector]
        public int animHash;
    }

    public float attackSpeed = 1f;
    public float damage = 50;
    public ComboStep[] steps;

    private int curStep = 0;
    private bool repeat = false;
    private bool attackHold = false;

    private float nextStepTime;

    private Transform turnTarget = null;

    public override void Start()
    {
        base.Start();

        //set up each animation using the assigned state name
        foreach (ComboStep cs in steps)
        {
            cs.animHash = Animator.StringToHash(cs.animStateName);
        }
    } 

    //called from girl controller Update() while this ability is active
    public override void ActiveUpdate()
    {
        //if we're airborne, we cancel this attack
        if (gc.IsAirborne)
        {
            isActive = false;
            return;
        }

        //if we ever release attack input, un-set attack hold
        if (!PlayerInput.IsAttackPressed())
        {
            attackHold = false;
        }

        //have we reached the end of the step?
        if (Time.time >= nextStepTime)
        {
            //if we've held attack down, or we've queue'd another step => begin next step
            if (repeat || attackHold)
            {
                curStep++;
                if (curStep == steps.Length)
                {
                    curStep = 0;
                }

                BeginStep();
            }
            //otherwise => end combo, reset animator speed, crossfade back to idle
            else
            {
                isActive = false;
                animator.speed = 1f;
                animator.CrossFade(HashLookup.idleMoveHash, steps[curStep].toIdleLength);

                gc.controller.FacingTarget = null;
            }
        }
    }

    //called when starting new combo => reset combo state, then begin step
    public override void Execute()
    {
        curStep = 0;
        attackHold = true;
        repeat = false;
        BeginStep();
    }

    //called instead of execute when a multi-step action is executed again
    public override void QueueNextStep()
    {
        attackHold = true;

        //we don't allow queueing if this is the final combo step
        if (curStep != steps.Length - 1)
        {
            if (isActive)
            {
                repeat = true;
            }
            else
            {
                Execute();
            }
        }
    }

    //sometimes we want to cancel a queued step
    public override void CancelNextStep()
    {
        repeat = false;
        attackHold = false;
    }

    //Begin new combo step
    private void BeginStep()
    {
        isActive = true;
        repeat = false;
     
        //find target to turn towards
        AcquireTurnTarget();

        //next, we set the animator to play at the clip speed multiplied by the character's attack speed
        animator.speed = steps[curStep].playSpeed * attackSpeed;
        animator.Play(steps[curStep].animHash, 0, 0f);

        //finally, we set the expiration time of the combo step based on the animation length and animation speed
        nextStepTime = Time.time + steps[curStep].animLength / steps[curStep].playSpeed / attackSpeed;
    }

    //is this a multi-step action?
    public override bool IsMultiStep()
    {
        return true;
    }

    //is this action unavailable?
    public override bool IsDisabled() {
        return gc.IsAirborne;
    }

    //what is the current combo step?
    public ComboStep GetCurrentComboStep()
    {
        return steps[curStep];
    }

    private void AcquireTurnTarget()
    {
        //Find potential targets in an area around the player
        Transform[] possibleTargets = TargetFinder.SphereFind(transform.position, 3f, HashLookup.EnemiesMask);
        //find target to turn towards, preferring our previous target
        turnTarget = gc.FindTarget(possibleTargets, true, true, turnTarget);
        gc.controller.FacingTarget = turnTarget;

        //If a player is holding input, face input direction (note: if a turn target was found, it takes priority (see BaseController.cs)
        PlayerInput.JoystickPositions dir = PlayerInput.GetInputDirection();
        if (dir == PlayerInput.JoystickPositions.Right)
        {
            gc.controller.FacingForward = true;
        }
        else if(dir == PlayerInput.JoystickPositions.Left)
        {
            gc.controller.FacingForward = false;
        }

    }

}
