using UnityEngine;
using System.Collections;

public class ShielderController : MonoBehaviour {

    public float aggroRange = 10f;

    public BaseController target;
    private Animator animator;
    private BaseController controller;
    private Damageable damageable;

    private float nextAttackTime;
    private float turnUnlockTime;
    private float attackUnlockTime;
    private float minBlockTime;
    private float deathTime;

    private bool isBlocking = false;

    private ShielderState state = ShielderState.Idle;
    private ShielderMode mode = ShielderMode.Normal;

    private enum ShielderState
    {
        Idle,
        Aggro
    }

    private enum ShielderMode
    {
        Normal,
        Blocking
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
        damageable.damageValidator = CheckBlock;
    }

    // Update is called once per frame
    void Update()
    {

        float targetDist = TargetDistance();

        if (damageable.IsDead && Time.time > deathTime)
        {
            Object.Destroy(gameObject);
        }

        if (state == ShielderState.Idle)
        {
            if (targetDist < aggroRange)
            {
                state = ShielderState.Aggro;
            }
        }

        else if (state == ShielderState.Aggro)
        {
            if (mode == ShielderMode.Normal)
            {
                if (!damageable.IsDead && !controller.IsStunned)
                {
                    controller.LockTurn = false;
                    bool targetRight = target.transform.position.x > transform.position.x;

                    if (targetDist > 1f && FindSpaceToTarget())
                    {
                        controller.MoveDir = targetRight ? BaseController.MoveDirection.Forward : BaseController.MoveDirection.Backward;
                    }
                    else
                    {
                        controller.MoveDir = BaseController.MoveDirection.None;

                        if (FacingTarget())
                        {
                            animator.Play(HashLookup.shieldingHash);
                            mode = ShielderMode.Blocking;
                            attackUnlockTime = Time.time + 2f;
                            minBlockTime = Time.time + 1f;
                            StartCoroutine(DelayInvulnerable());
                            Debug.Log("ENTER BLOCK MODE");
                        }
                    }
                }
            }
            else if (mode == ShielderMode.Blocking)
            {
                controller.LockTurn = true;

                if (!CanTurn())
                    return;

                if (!damageable.IsDead && !controller.IsStunned)
                {
                    bool attackReady = Time.time >= nextAttackTime;
                    bool targetRight = target.transform.position.x > transform.position.x;

                    if (controller.FacingForward != targetRight && Time.time > minBlockTime)
                    {
                        mode = ShielderMode.Normal;
                        animator.CrossFade(HashLookup.idleMoveHash, 0.2f, 0, 0);
                        isBlocking = false;
                    }
                    else
                    {
                        if (targetDist > 1f)
                        {
                            if (FindSpaceToTarget())
                            {
                                controller.MoveDir = targetRight ? BaseController.MoveDirection.Forward : BaseController.MoveDirection.Backward;
                            }
                            else
                            {
                                controller.MoveDir = BaseController.MoveDirection.None;
                            }
                        }
                        else
                        {
                            controller.MoveDir = BaseController.MoveDirection.None;

                            if (FacingTarget())
                            {
                                TryAttack();
                            }

                        }
                    }
                }
            }
        }
    }

    private bool CanTurn()
    {
        return Time.time >= turnUnlockTime;
    }

    private bool FacingTarget()
    {
        bool targetRight = target.transform.position.x > transform.position.x;

        if (targetRight)
        {
            return Mathf.Abs(Mathf.DeltaAngle(0, transform.localEulerAngles.y)) < 5f;
        }
        else
        {
            return Mathf.Abs(Mathf.DeltaAngle(180, transform.localEulerAngles.y)) < 5f;
        }
    }

    private bool FindSpaceToTarget()
    {
        int dir = target.ColliderPos.x > controller.ColliderPos.x ? 1 : -1;

        Vector3 playerPoint = target.ColliderPos - new Vector3(target.ColliderRadius * dir, 0, 0);
        Vector3 myPoint = controller.ColliderPos + new Vector3(controller.ColliderRadius * dir, 0, 0);

        myPoint.y = playerPoint.y;
        myPoint.z = playerPoint.z;

        float totalDist = Vector3.Distance(playerPoint, myPoint) - 0.3f;

        RaycastHit[] hits = Physics.RaycastAll(playerPoint, Vector3.left * dir, totalDist, HashLookup.EnemiesMask);
        float occupiedSpace = 0;

        if (hits.Length == 0)
        {
            return true;
        }

        foreach (RaycastHit hit in hits)
        {
            occupiedSpace += hit.transform.GetComponent<BaseController>().ColliderRadius * 2;
        }

        float freeSpace = totalDist - occupiedSpace;
        Debug.Log(freeSpace);
        Debug.Log(controller.ColliderRadius * 2f + 0.25f < freeSpace);

        return (controller.ColliderRadius * 2f + 0.25f < freeSpace);

    }

    private float TargetDistance()
    {
        int dir = target.ColliderPos.x > controller.ColliderPos.x ? 1 : -1;

        Vector3 playerPoint = target.ColliderPos - new Vector3(target.ColliderRadius * dir, 0, 0);
        Vector3 myPoint = controller.ColliderPos + new Vector3(controller.ColliderRadius * dir, 0, 0);

        myPoint.y = playerPoint.y;
        myPoint.z = playerPoint.z;

        float totalDist = Vector3.Distance(playerPoint, myPoint);
        return totalDist;
    }

    private void TryAttack()
    {
        if (Time.time > nextAttackTime && Time.time > attackUnlockTime)
        {
            StartCoroutine(ToggleVulnerable());
            animator.Play(HashLookup.attackHash, 0, 0);
            nextAttackTime = Time.time + 2.5f;
            turnUnlockTime = Time.time + 1.5f;
        }
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

    private IEnumerator ToggleVulnerable()
    {
        yield return new WaitForSeconds(0.2f);
        isBlocking = false;
        yield return new WaitForSeconds(1);
        isBlocking = true;
    }

    private IEnumerator DelayInvulnerable()
    {
        yield return new WaitForSeconds(0.3f);
        isBlocking = true;
    }

    public float CheckBlock(float damage, GameObject source)
    {
        bool targetRight = source.transform.position.x > transform.position.x;

        if (isBlocking && targetRight == controller.FacingForward)
        {
            return 0;
        }

        return damage;
    }

    public void AttackFrame()
    {
        Transform[] targets = TargetFinder.SphereFind(transform.position + new Vector3((controller.ColliderRadius + 0.5f) * controller.FacingValue, 0, 0), 1f, 1 << HashLookup.playerLayer);
        if (targets.Length > 0)
        {
            targets[0].GetComponent<BaseController>().CurExternalVeloX = 8f * controller.FacingValue;
            targets[0].GetComponent<BaseController>().Stun(0.25f);
            targets[0].GetComponent<Damageable>().TakeDamage(20, gameObject);
        }
    }

}
