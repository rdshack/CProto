using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BaseController))]
public class EnemyController : MonoBehaviour {

    public float aggroRange = 10f;

    public BaseController target;
    private Animator animator;
    private BaseController controller;
    private Damageable damageable;

    private float nextAttackTime;
    private float turnUnlockTime;
    private float deathTime;

    private SwarmlingState state = SwarmlingState.Idle;

    private enum SwarmlingState
    {
        Idle,
        Aggro
    }

	// Use this for initialization
    void Awake()
    {
        controller = GetComponent<BaseController>();
        animator = GetComponentInChildren<Animator>();
        damageable = GetComponent<Damageable>();
	}

    void Start()
    {
        controller.FacingTarget = target.transform;
        damageable.deathEvent += HandleDeath;
    }
	
	// Update is called once per frame
	void Update () {

        float targetDist = Vector3.Distance(target.ColliderPos, controller.ColliderPos);

        if (damageable.IsDead && Time.time > deathTime)
        {
            Object.Destroy(gameObject);
        }

        if (state == SwarmlingState.Idle)
        {
            if (targetDist < aggroRange)
            {
                state = SwarmlingState.Aggro;
            }
        }

        else if (state == SwarmlingState.Aggro)
        {
            if (!damageable.IsDead && !controller.IsStunned)
            {
                controller.LockTurn = CanTurn() ? false : true;

                bool hasJumpSpace = !JumpBlocked();
                bool attackReady = Time.time >= nextAttackTime;
                bool targetRight = target.transform.position.x > transform.position.x;

                if (targetDist < 1.5f)
                {
                    controller.MoveDir = BaseController.MoveDirection.None;

                    if (attackReady)
                    {
                        Attack();
                    }
                }
                else if (targetDist >= 1.5f && targetDist < 3f)
                {
                    if (attackReady && hasJumpSpace)
                    {
                        Attack();
                    }
                    else if (FindSpaceToTarget())
                    {
                        controller.MoveDir = targetRight ? BaseController.MoveDirection.Forward : BaseController.MoveDirection.Backward;
                    }
                    else
                    {
                        controller.MoveDir = BaseController.MoveDirection.None;
                    }
                }
                else if (FindSpaceToTarget())
                {
                    controller.MoveDir = targetRight ? BaseController.MoveDirection.Forward : BaseController.MoveDirection.Backward;
                }
                else
                {
                    controller.MoveDir = BaseController.MoveDirection.None;
                }
            }
        }
	}

    private bool CanTurn()
    {
        return Time.time >= turnUnlockTime;
    }

    private bool JumpBlocked()
    {
        int dir = target.ColliderPos.x > controller.ColliderPos.x ? 1 : -1;
        float pointOffset = controller.ColliderHeight / 2;
        float xOffset = (controller.ColliderRadius * 2) * dir;

        Vector3 p1 = controller.ColliderPos + new Vector3(xOffset, pointOffset, 0);
        Vector3 p2 = controller.ColliderPos + new Vector3(xOffset, -pointOffset, 0);

        Debug.DrawLine(p1, p2, Color.blue);

        int mask = (1 << HashLookup.enemyLayer);
        bool test = Physics.CheckCapsule(p1, p2, 0.25f, mask);


        return test;
    }

    private bool FindSpaceToTarget()
    {
        int dir = target.ColliderPos.x > controller.ColliderPos.x ? 1 : -1;

        Vector3 playerPoint = target.ColliderPos - new Vector3(target.ColliderRadius * dir, 0, 0);
        Vector3 myPoint = controller.ColliderPos - new Vector3(controller.ColliderRadius * dir, 0 ,0);
        playerPoint.y = myPoint.y;
        playerPoint.z = myPoint.z;

        float totalDist = Vector3.Distance(playerPoint, controller.ColliderPos + new Vector3(controller.ColliderRadius * dir, 0, 0));
       
        RaycastHit[] hits = Physics.RaycastAll(playerPoint, Vector3.left * dir, totalDist, HashLookup.EnemiesMask);
        float occupiedSpace = 0;
        foreach (RaycastHit hit in hits)
        {
            occupiedSpace += hit.transform.GetComponent<BaseController>().ColliderRadius * 2;
        }

        float freeSpace = totalDist - occupiedSpace;

        return (controller.ColliderRadius * 2f + 0.25f < freeSpace);

    }

    private void Attack()
    {
        animator.Play(HashLookup.attackHash, 0, 0);
        nextAttackTime = Time.time + 2f;
        turnUnlockTime = Time.time + 1f;
    }

    public void HandleDeath()
    {
        animator.Play(HashLookup.deathHash, 0, 0);
        deathTime = Time.time + 1.4f;

        foreach (Behaviour b in GetComponentsInChildren<Behaviour>())
        {
            b.enabled = false;
        }

        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        this.enabled = true;
        GetComponentInChildren<Animator>().enabled = true;
    }

}
