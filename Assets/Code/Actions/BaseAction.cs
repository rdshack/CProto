using UnityEngine;
using System.Collections;

public abstract class BaseAction : MonoBehaviour {

    protected bool useRootMotion = true;
    protected bool canTurn = false;
    protected bool isActive = false;
    protected float cooldownTime = 0f;

    private float cooldownReadyTime = 0f;

    protected GirlController gc;
    protected Animator animator;

    public virtual void Execute()
    {
        cooldownReadyTime = Time.time + cooldownTime;
    }

    public virtual void Start()
    {
        gc = GetComponent<GirlController>();
        animator = GetComponentInChildren<Animator>();
    }

    public virtual void ActiveUpdate()
    {
    }

    public virtual void QueueNextStep()
    {
    }

    public virtual void CancelNextStep()
    {

    }

    public virtual bool IsMultiStep()
    {
        return false;
    }

    public virtual bool IsQueueable()
    {
        return true;
    }

    public bool CanTurn()
    {
        return canTurn;
    }

    public bool UseRootMotion()
    {
        return useRootMotion;
    }

    public virtual bool AllowPlayerMotion()
    {
        return !useRootMotion;
    }

    public bool IsActive()
    {
        return isActive;
    }

    public virtual bool AllowAutoJump()
    {
        return false;
    }

    public virtual bool IsDisabled()
    {
        return false;
    }

    public bool CooldownReady()
    {
        return Time.time > cooldownReadyTime;
    }

}
