using UnityEngine;
using System.Collections;
using ParticlePlayground;


public class Dash : BaseAction {

    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;

    private DashDirection dashDir = DashDirection.Neutral;
    private bool usedInAir = false;
    private Vector3 moveVector;

    private DashState state;
    private float changeStateTime;
    private PushField girlPushField;

    //particle systems
    public PlaygroundParticlesC dashParticles;


    private enum DashState
    {
        DashMain,
        DashEnd
    }

    public enum DashDirection
    {
        Neutral,
        Forward,
        Backward
    }

    public override void Start()
    {
        base.Start();
        moveVector = new Vector3(0, 0, 0);
        cooldownTime = 0.55f;

        gc.OnJumpPadActivate += HandleJumpPad;

        dashParticles.emit = false;
        girlPushField = GetComponentInChildren<PushField>();
    }

    void Update()
    {
        if (!gc.IsAirborne)
        {
            usedInAir = false;
        }
    }

    public override void ActiveUpdate()
    {
        if (state == DashState.DashMain)
        {
            if (Time.time > changeStateTime)
            {
                state = DashState.DashEnd;
                gc.controller.LockY = false;
                dashParticles.emit = false;
               
                if (gc.IsAirborne)
                {
                    usedInAir = true;
                    animator.CrossFade(HashLookup.loopJumpHash, 0.1f);
                    changeStateTime = Time.time + 0.1f;
                    isActive = false;
                    girlPushField.Active = true;
                }
                else
                {
                    animator.CrossFade(HashLookup.endDashGroundHash, 0.1f);
                    changeStateTime = Time.time + 0.3f;
                    isActive = false;
                    girlPushField.Active = true;
                }
            }
            else
            {
                CharacterController controller = gc.GetComponent<CharacterController>();
                moveVector.x = dashSpeed * Time.deltaTime * gc.controller.FacingValue;

                gc.controller.Move(moveVector, 1 << HashLookup.wallLayer);
            }
        } 
        else if (state == DashState.DashEnd) 
        {
            if (Time.time > changeStateTime)
            {
                isActive = false;
                girlPushField.Active = true;
            }
        }
    }

    public override void Execute()
    {
        base.Execute();

        isActive = true;
        state = DashState.DashMain;
        changeStateTime = Time.time + dashDuration;

        if (dashDir == DashDirection.Neutral)
        {
            StartCoroutine(EnableTurning(0.05f));
        }
        else if (dashDir == DashDirection.Forward)
        {
            gc.controller.FacingForward = true;
        }
        else
        {
            gc.controller.FacingForward = false;
        }
       
        dashParticles.emit = true;
        girlPushField.Active = false;

        gc.controller.LockY = true;

        if (gc.IsAirborne)
        {
            animator.CrossFade(HashLookup.dashAirHash, 0.1f);
        }
        else
        {
            animator.CrossFade(HashLookup.dashGroundHash, 0.1f);
        }
    }

    public override bool IsDisabled()
    {
        return usedInAir;
    }

    public override bool AllowAutoJump()
    {
        return true;
    }

    public void HandleJumpPad()
    {
        usedInAir = false;
        isActive = false;
        gc.controller.LockY = false;
        dashParticles.emit = false;
        girlPushField.Active = true;
    }

    public void SetDashDir(DashDirection dir)
    {
        dashDir = dir;
    }

    private IEnumerator EnableTurning(float duration)
    {
        canTurn = true;
        yield return new WaitForSeconds(duration);
        canTurn = false;
    }
}
