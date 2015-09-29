using UnityEngine;
using System.Collections;

public class Tornado : BaseAction {

    private float nextStateTime;

    private TornadoStates state;


    private enum TornadoStates
    {
        Casting,
        PostCast,
    }

    public override void Start()
    {
        base.Start();
        gc.OnJumpPadActivate += HandleJumpPad;
        useRootMotion = false;
    }

    //called from girl controller Update() while this ability is active
    public override void ActiveUpdate()
    {
        if (state == TornadoStates.Casting)
        {
            if (Time.time > nextStateTime)
            {
                state = TornadoStates.PostCast;
                gc.controller.LockY = false;
                nextStateTime = Time.time + 0.15f;

                //spawn projectile
                GameObject go = MonoBehaviour.Instantiate(Resources.Load("TornadoProj", typeof(GameObject))) as GameObject;
                TornadoProjectile t = go.GetComponent<TornadoProjectile>();
                t.Throw(gc.controller.FacingValue, transform);
            }
        }

        else if (state == TornadoStates.PostCast)
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
        nextStateTime = Time.time + 0.4f;
        state = TornadoStates.Casting;

        animator.speed = 1.5f;
        animator.CrossFade(HashLookup.tornadoHash, 0.1f, 0, 0);

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
    }

    public void HandleJumpPad()
    {
        ExitAction();
    }
}
