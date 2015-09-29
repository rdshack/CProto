using UnityEngine;
using System.Collections;

public class AirAttack : BaseAction {

    private float nextStateTime;
    private float attackDuration = 0.3f;
    private float cooldownDuration = 0.25f;

    private AirAttackState state;

    private enum AirAttackState
    {
        AirAttackMain,
        AirAttackEnd
    }

    public override void Start()
    {
        base.Start();
        useRootMotion = false;
        cooldownTime = 0.8f;
    }

    //called from girl controller Update() while this ability is active
    public override void ActiveUpdate()
    {
        if (!gc.IsAirborne)
        {
            ExitAction();
            return;
        }

        if (state == AirAttackState.AirAttackMain)
        {
            if (Time.time > nextStateTime)
            {
                nextStateTime = Time.time + cooldownDuration;
                state = AirAttackState.AirAttackEnd;
                animator.speed = 1f;
                gc.controller.maxGravSpeed = 12f;

                animator.CrossFade(HashLookup.loopJumpHash, 0.1f);
            }
        }
        else if (state == AirAttackState.AirAttackEnd)
        {

            if (Time.time > nextStateTime)
            {
                ExitAction();
            }
        }


    }

    public override void Execute()
    {
        base.Execute();
        isActive = true;
        nextStateTime = Time.time + attackDuration;
        state = AirAttackState.AirAttackMain;
        StartCoroutine(BeginTurnWindow(0.05f));

        if (gc.controller.CurExternalVeloY < 2f)
        {
            gc.controller.CurExternalVeloY = 2f;
        }

        animator.speed = 2.5f;
        animator.CrossFade(HashLookup.airSpecialHash, 0.1f, 0, 0);
    }

    private void ExitAction()
    {
        if (gc.IsAirborne)
        {
            animator.CrossFade(HashLookup.loopJumpHash, 0.1f, 0, 0);
        }
        else
        {
            animator.CrossFade(HashLookup.idleMoveHash, 0.1f, 0, 0);
        }

        isActive = false;

        animator.speed = 1f;
        gc.controller.maxGravSpeed = 12f;
    }

    private IEnumerator BeginTurnWindow(float duration)
    {
        canTurn = true;
        yield return new WaitForSeconds(duration);
        canTurn = false;
    }

    public override bool AllowPlayerMotion()
    {
        return gc.IsAirborne;
    }
}
