using UnityEngine;
using System.Collections;

public class AirSpecial : BaseAction {

    private float loopTime;
    private float playSpeed = 1.2f;

    private float attackDuration = 0.3f;
    private float cooldownDuration = 0.2f;

    private float curHoldTime = 0f;
    private float reqHoldTime = 0.15f;

    private bool isHolding = false;
    public Transform effectPrefab;
    private Transform tempEffect = null;

    //4% to 32%
    //2.333

    private AirSpecialState state;
    private float nextStateTime;

    private enum AirSpecialState
    {
        AirSpecialAttack,
        AirSpecialAttackEnd,
        AirSpecialHold
    }

    public override void Start()
    {
        base.Start();
        useRootMotion = false;
        canTurn = true;
        cooldownTime = 0.8f;

    }

    void Update()
    {
        if (!isActive && tempEffect != null)
        {
            if (tempEffect != null)
            {
                Destroy(tempEffect.gameObject);
            }
        }

        if (PlayerInput.IsAttackPressed())
        {
            curHoldTime += Time.deltaTime;
        }
        else
        {
            curHoldTime = 0;
        }
    }

    public override void ActiveUpdate()
    {
        if (state == AirSpecialState.AirSpecialAttack)
        {

            if (Time.time > nextStateTime)
            {
                if (curHoldTime > reqHoldTime && PlayerInput.IsAttackPressed())
                {
                    state = AirSpecialState.AirSpecialHold;
                    loopTime = Time.time + (0.55f / playSpeed);
                    animator.speed = playSpeed;
                    animator.CrossFade(HashLookup.airSpecialHash, 0.15f, 0, 0.04f);
                    StartCoroutine(DelaySpawn(0.2f));

                    gc.controller.maxGravSpeed = 2f;
                }
                else
                {
                    nextStateTime = Time.time + cooldownDuration;
                    state = AirSpecialState.AirSpecialAttackEnd;
                    animator.speed = 1f;
                    gc.controller.maxGravSpeed = 12f;

                    animator.CrossFade(HashLookup.loopJumpHash, 0.1f);
                }

            }
        }
        else if (state == AirSpecialState.AirSpecialAttackEnd)
        {
            if (curHoldTime > reqHoldTime && PlayerInput.IsAttackPressed())
            {
                state = AirSpecialState.AirSpecialHold;
                loopTime = Time.time + (0.55f / playSpeed);
                animator.speed = playSpeed;
                animator.CrossFade(HashLookup.airSpecialHash, 0.15f, 0, 0.04f);
                StartCoroutine(DelaySpawn(0.2f));

                gc.controller.maxGravSpeed = 2f;
            }
            else if(Time.time > nextStateTime)
            {
                ExitAction();
            }
        }
        else if (state == AirSpecialState.AirSpecialHold)
        {
            if (!PlayerInput.IsAttackPressed())
            {
                isHolding = false;
            }

            if (Time.time > loopTime)
            {
                loopTime = Time.time + (0.55f / playSpeed);

                animator.CrossFade(HashLookup.airSpecialHash, 0.1f, 0, 0.04f);
            }

            if (!isHolding || !gc.IsAirborne)
            {
                ExitAction();
            }

            if (tempEffect != null)
            {
                Vector3 curAngles = tempEffect.localEulerAngles;
                tempEffect.localEulerAngles = new Vector3(curAngles.x, curAngles.y + 500f * Time.deltaTime, curAngles.z);
            }
        }

    }

    public override void Execute()
    {
        base.Execute();

        isActive = true;
        state = AirSpecialState.AirSpecialAttack;
        nextStateTime = Time.time + attackDuration;

        if (gc.controller.CurExternalVeloY < 3f)
        {
            gc.controller.CurExternalVeloY = 3f;
        }
        
        isHolding = true;

        animator.speed = 2.5f;
        animator.CrossFade(HashLookup.airSpecialHash, 0.1f, 0, 0);
    }

    public override bool IsDisabled()
    {
        return !gc.IsAirborne;
    }

    private void ExitAction()
    {
        isActive = false;
        animator.CrossFade(HashLookup.loopJumpHash, 0.1f);

        animator.speed = 1f;
        gc.controller.maxGravSpeed = 12f;
    }

    private IEnumerator DelaySpawn(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (isActive && state == AirSpecialState.AirSpecialHold)
        {
            gc.ClearNextAction();
            tempEffect = Instantiate(effectPrefab);
            tempEffect.transform.position = transform.position + new Vector3(0, 3f, 0);
            tempEffect.SetParent(gameObject.transform);
        }

    }
}
