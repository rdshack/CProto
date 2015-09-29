using UnityEngine;
using System.Collections;

public class AirFloat : BaseAction {

    private float loopTime;
    private float playSpeed = 1.2f;

    public Transform effectPrefab;
    private Transform tempEffect = null;


    public override void Start()
    {
        base.Start();
        useRootMotion = false;
        canTurn = true;

    }

    public override void ActiveUpdate()
    {

        if (!Input.GetMouseButton(1) || !gc.IsAirborne)
        {
            ExitAction();
        }

        if (Time.time > loopTime)
        {
            loopTime = Time.time + (0.55f / playSpeed);

            animator.CrossFade(HashLookup.airSpecialHash, 0.1f, 0, 0.04f);
        }

        if (tempEffect != null)
        {
            Vector3 curAngles = tempEffect.localEulerAngles;
            tempEffect.localEulerAngles = new Vector3(curAngles.x, curAngles.y + 500f * Time.deltaTime, curAngles.z);
        }
        

    }

    public override void Execute()
    {
        base.Execute();

        isActive = true;
        gc.controller.maxGravSpeed = 3f;

        animator.speed = playSpeed;
        StartCoroutine(DelaySpawn(0.2f));

        loopTime = Time.time + (0.55f / playSpeed);
        animator.CrossFade(HashLookup.airSpecialHash, 0.1f, 0, 0.04f);
    }

    public override bool IsDisabled()
    {
        return !gc.IsAirborne;
    }

    public override bool IsQueueable()
    {
        return false;
    }

    private void ExitAction()
    {
        isActive = false;
        if (gc.IsAirborne)
        {
            animator.CrossFade(HashLookup.loopJumpHash, 0.1f, 0, 0);
        }
        else
        {
            animator.CrossFade(HashLookup.idleMoveHash, 0.1f, 0, 0);
        }

        animator.speed = 1f;
        gc.controller.maxGravSpeed = 12f;

        if (tempEffect != null)
        {
            Destroy(tempEffect.gameObject);
        }
    }

    private IEnumerator DelaySpawn(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (isActive)
        {
            tempEffect = Instantiate(effectPrefab);
            tempEffect.transform.position = transform.position + new Vector3(0, 3f, 0);
            tempEffect.SetParent(gameObject.transform);
        }

    }
}
