using UnityEngine;
using System.Collections;

public class Slam : BaseAction {

    private float nextStateTime;

    private SlamStates state;

    private enum SlamStates
    {
        Casting,
        PreImpact,
        PostImpact
    }

    public override void Start()
    {
        base.Start();
        gc.OnJumpPadActivate += HandleJumpPad;
    }

    //called from girl controller Update() while this ability is active
    public override void ActiveUpdate()
    {
        if (state == SlamStates.Casting)
        {
            if (Time.time > nextStateTime)
            {
                state = SlamStates.PreImpact;
                nextStateTime = Time.time + 0.15f;

                gc.controller.LockY = false;
                gc.controller.CurExternalVeloY = -40f;
            }
        }

        else if (state == SlamStates.PreImpact)
        {
            if (!gc.IsAirborne)
            {
                state = SlamStates.PostImpact;
                nextStateTime = Time.time + 0.25f;
                animator.speed = 1f;
                animator.CrossFade(HashLookup.slamHash, 0.1f, 0, 0.55f);

                SpawnDamageEffect();
            }

            if (Time.time > nextStateTime)
            {
                isActive = false;
                animator.speed = 1f;
                animator.CrossFade(HashLookup.loopJumpHash, 0.1f, 0, 0);
                gc.controller.CurExternalVeloY = 0f;
            }
        }

        else if (state == SlamStates.PostImpact)
        {
            if (Time.time > nextStateTime)
            {
                isActive = false;
            }
        }

    }

    private void SpawnDamageEffect()
    {
        Vector3 strikePosition = transform.position + new Vector3(2 * gc.controller.FacingValue, 0, 0);
        Affector a = TargetFinder.SpawnAffector(strikePosition, 4f, HashLookup.EnemiesMask);
        a.OnInterval += AffectTargets;
        a.OnEmptyInterval += OnAffectorEmpty;
    }

    private void AffectTargets(Transform[] targets)
    {
        foreach (Transform t in targets)
        {
            t.GetComponent<Damageable>().TakeDamage(40, gameObject);
            t.GetComponent<BaseController>().CurExternalVeloY = 7f;
            t.GetComponent<BaseController>().Stun(2f);
        }

        Vector3 strikePosition = transform.position + new Vector3(2 * gc.controller.FacingValue, 0, 0);
        GameObject go = MonoBehaviour.Instantiate(Resources.Load("SlamEffect", typeof(GameObject))) as GameObject;
        go.transform.position = strikePosition;
    }

    private void OnAffectorEmpty()
    {
        Vector3 strikePosition = transform.position + new Vector3(2 * gc.controller.FacingValue, 0, 0);
        GameObject go = MonoBehaviour.Instantiate(Resources.Load("SlamEffect", typeof(GameObject))) as GameObject;
        go.transform.position = strikePosition;
    }

    public override void Execute()
    {
        base.Execute();

        isActive = true;
        nextStateTime = Time.time + 0.4f;
        state = SlamStates.Casting;

        gc.controller.LockY = true;
        gc.controller.CurInfluenceX = 0;
        gc.controller.CurExternalVeloX = 0;

        animator.speed = 1.5f;
        animator.CrossFade(HashLookup.slamHash, 0.1f, 0, 0);

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
        if (state == SlamStates.PreImpact)
        {
            SpawnDamageEffect();
            state = SlamStates.Casting;
        }

        isActive = false;

    }

}
