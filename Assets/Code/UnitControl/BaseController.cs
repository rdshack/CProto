using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class BaseController : MonoBehaviour {

    private CharacterController controller;
    private Animator animator;

    private bool isAirborne = false;
    private bool lockY = false;
    private bool lockTurn = false;

    private bool facingForward = true;

    public float gravStrength = 15f;
    public float moveAccelX = 4f;
    public float groundInfluenceDecayX = 4f;
    public float airInfluenceDecayX = 4f;
    public float groundExternalVeloDecayX = 4f;
    public float airExternalVeloDecayX = 4f;
    public float turnSpeed = 50f;
    public float faceTargetTurnSpeed = 12f;
    public float groundMaxSpeedX = 4f;
    public float groundMaxInfluenceX = 4f;
    public float airMaxSpeedX = 4f;
    public float airMaxInfluenceX = 4f;
    public float maxGravSpeed = 12f;
    
    private MoveDirection moveDir = MoveDirection.None;

    private Transform facingTarget = null;
    private int moveBlockMask = 0;

    public delegate void EventHandler(BaseController self);
    public event EventHandler OnLand;

    public enum MoveDirection
    {
        None,
        Forward,
        Backward
    }

    private struct TotalExternalForce
    {
        public float veloX;
        public float veloY;
        public float accelX;
        public float accelY;
    }

    public MoveDirection MoveDir { get { return moveDir; } set { moveDir = value; } }
    public int MoveBlockMask { set { moveBlockMask = value; } }
    public int FacingValue { get { return facingForward ? 1 : -1; } }

    public Vector3 ColliderPos { get { return transform.position + controller.center; } }
    public float ColliderHeight { get { return controller.height; } }
    public float ColliderRadius { get { return controller.radius; } }
    public Vector3 ColliderCenter { get { return controller.center; } }

    public float CurInfluenceX { get; set; }
    public float CurExternalVeloX { get; set; }
    public float TotalVeloX { get { return CurInfluenceX + CurExternalVeloX; } }
    public float CurExternalVeloY { get; set; }
    public bool IsAirborne { get { return isAirborne; } }

    public bool LockY { get { return lockY; } set { lockY = value; } }

    public bool LockTurn { get { return lockTurn; } set { lockTurn = value; } }
    public bool FacingForward { get { return facingForward; } set { facingForward = value; } }
    public Transform FacingTarget { get { return facingTarget; } set { facingTarget = value; } }

    public float MaxInfluenceMultiplier { get; set; }
    public bool IsStunned { get { return isStunned; } }

    private List<ControllerForce> externalForces;
    private bool ignoreGravity = false;
    private bool isStunned = false;

	void Awake () {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        externalForces = new List<ControllerForce>();
	}

    void Start()
    {
        MaxInfluenceMultiplier = 1;
    }
	
	void Update () {
        HandleMovement();
        HandleTurning();
        HandleWalls();
	}

    private void HandleTurning()
    {
        if (lockTurn)
        {
            return;
        }

        if (facingTarget != null)
        {
            Vector3 tempTarget = new Vector3(facingTarget.position.x, transform.position.y, facingTarget.position.z);
            Vector3 relativePos = tempTarget - transform.position;
            float targetAngle = Quaternion.LookRotation(relativePos).eulerAngles.y - 90f;
                
            float yRot = Mathf.LerpAngle(transform.localEulerAngles.y, targetAngle, faceTargetTurnSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0f, yRot, 0f);

            FaceTarget(facingTarget);
        }
        else if (facingForward)
        {
            float yRot = Mathf.LerpAngle(transform.localEulerAngles.y, 0f, turnSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0f, yRot, 0f);
        }
        else
        {
            float yRot = Mathf.LerpAngle(transform.localEulerAngles.y, 180f, turnSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0f, yRot, 0f);
        }
    }

    private void HandleMovement()
    {
        if (CurExternalVeloY > 0)
            isAirborne = true;

        if (isStunned)
        {
            CurInfluenceX = 0;
        }
        else
        {
            if (moveDir == MoveDirection.Forward)
            {
                CurInfluenceX += moveAccelX * Time.deltaTime;
            }
            else if (moveDir == MoveDirection.Backward)
            {
                CurInfluenceX -= moveAccelX * Time.deltaTime;
            }
            else
            {
                float influenceDecay = isAirborne ? airInfluenceDecayX : groundInfluenceDecayX;
                if (Mathf.Abs(CurInfluenceX) < influenceDecay * Time.deltaTime)
                {
                    CurInfluenceX = 0;
                }
                else
                {
                    if (CurInfluenceX < 0)
                    {
                        CurInfluenceX += influenceDecay * Time.deltaTime;
                    }
                    else
                    {
                        CurInfluenceX -= influenceDecay * Time.deltaTime;
                    }
                }
            }
        }

        float externalVeloDecay = isAirborne ? airExternalVeloDecayX : groundExternalVeloDecayX;
        if (Mathf.Abs(CurExternalVeloX) < externalVeloDecay * Time.deltaTime)
        {
            CurExternalVeloX = 0;
        }
        else
        {
            if (CurExternalVeloX < 0)
            {
                CurExternalVeloX += externalVeloDecay * Time.deltaTime;
            }
            else
            {
                CurExternalVeloX -= externalVeloDecay * Time.deltaTime;
            }
        }
       
        if (isAirborne)
        {
            if (lockY)
            {
                CurExternalVeloY = 0;
            }
            else
            {
                CurExternalVeloY += GetTotalExternalForce().accelY * Time.deltaTime;
            }
        }
        else
        {
            //if we're grounded, and we have Y locked - stick to ground
            if(lockY) 
            {
                CurExternalVeloY = -4f;
            }
            //if we're grounded, and summed external forces are positive - begin floating up (this will start airborne state)
            else if (GetTotalExternalForce().accelY > 0) 
            {
                CurExternalVeloY = GetTotalExternalForce().accelY * Time.deltaTime;
            }
        }

        float maxSpeedX = isAirborne ? airMaxSpeedX : groundMaxSpeedX;
        float maxInfluenceX = isAirborne ? airMaxInfluenceX : groundMaxInfluenceX;

        CurInfluenceX = Mathf.Clamp(CurInfluenceX, -maxInfluenceX * MaxInfluenceMultiplier, maxInfluenceX * MaxInfluenceMultiplier);
        float combinedVeloX = Mathf.Clamp(CurInfluenceX + CurExternalVeloX, -maxSpeedX, maxSpeedX);

        Vector3 tempMove = Vector3.zero;
        tempMove.x = combinedVeloX * Time.deltaTime;
        tempMove.y = CurExternalVeloY * Time.deltaTime;

        Move(tempMove, moveBlockMask);

        float speedPerc = Mathf.Abs(CurInfluenceX) / maxInfluenceX;
        animator.SetFloat(HashLookup.speedBlendHash, speedPerc);

    }

    void FixedUpdate()
    {
        bool wasAirborne = isAirborne;
        isAirborne = !IsGrounded();

        if (!isAirborne && wasAirborne)
        {
            CurExternalVeloY = -2f;

            if (OnLand != null)
            {
                OnLand(this);
            }

        }
    }

    //helpers
    private bool IsGrounded()
    {
        int mask = 1 << HashLookup.groundLayer;
        float pointOffset = controller.height / 2 - controller.radius;
        Vector3 p1 = ColliderPos + new Vector3(0, pointOffset - 0.2f, 0);
        Vector3 p2 = ColliderPos + new Vector3(0, -pointOffset - 0.2f, 0);

        bool foundGround = Physics.CheckCapsule(p1, p2, controller.radius, mask);

        return foundGround && (CurExternalVeloY <= 0) && ComputeGroundAngle() < controller.slopeLimit;
    }

    public void Move(Vector3 move, int blockMask = 0, bool checkAllZ = false)
    {
        Vector3 tempMove = move;

        if (blockMask != 0)
        {
            if (CheckSpace(move.x, blockMask, checkAllZ))
            {
                tempMove.x = 0;
            }
        }

        if (CheckGroundSpace(move.x))
        {
            tempMove.x = 0;
        }

        controller.Move(tempMove);
    }

    public void AddForce(ControllerForce force)
    {
        externalForces.Add(force);

        //if at least 1 active force cancels gravity, we ignore gravity
        if (force.cancelGravity)
        {
            ignoreGravity = true;
        }
    }

    public void RemoveForce(ControllerForce force)
    {
        externalForces.Remove(force);

        bool temp = false;
        foreach (ControllerForce f in externalForces)
        {
            if (f.cancelGravity)
            {
                temp = true;
            }
        }
        ignoreGravity = temp;
    }

    private TotalExternalForce GetTotalExternalForce()
    {
        TotalExternalForce total = new TotalExternalForce();
        foreach (ControllerForce f in externalForces)
        {
            if (f.InActiveYRange(CurExternalVeloY))
            {
                total.accelY += f.accelY;
            }
        }

        total.accelY -= (!ignoreGravity && CurExternalVeloY > -maxGravSpeed) ? gravStrength : 0;

        return total;
    }

    private bool CheckSpace(float xMove, int hitMask, bool checkAllZ = false)
    {
        int dir = xMove > 0 ? 1 : -1;
        float pointOffset = controller.height / 2;
        float xOffset = (controller.radius + 0.1f) * dir;

        Vector3 p1 = ColliderPos + new Vector3(xOffset, pointOffset, 0);
        Vector3 p2 = ColliderPos + new Vector3(xOffset, -pointOffset, 0);

        bool hit = Physics.CheckCapsule(p1, p2, 0.05f, hitMask);

        if (checkAllZ)
        {
            hit |= Physics.CheckCapsule(p1 + new Vector3(0, 0, -0.5f), p2 + new Vector3(0, 0, -1), 0.05f, hitMask);
            hit |= Physics.CheckCapsule(p1 + new Vector3(0, 0, -1), p2 + new Vector3(0, 0, -1), 0.05f, hitMask);
            hit |= Physics.CheckCapsule(p1 + new Vector3(0, 0, 0.5f), p2 + new Vector3(0, 0, -1), 0.05f, hitMask);
            hit |= Physics.CheckCapsule(p1 + new Vector3(0, 0, 1), p2 + new Vector3(0, 0, -1), 0.05f, hitMask);
        }

        return hit;
    }

    private bool CheckGroundSpace(float xMove)
    {
        int dir = xMove > 0 ? 1 : -1;
        float pointOffset = controller.height / 2;
        float xOffset = (controller.radius + 0.5f) * dir;

        Vector3 p1 = ColliderPos + new Vector3(xOffset, pointOffset, 0);
        Vector3 p2 = ColliderPos + new Vector3(xOffset, 0, 0);

        return Physics.CheckCapsule(p1, p2, 0.05f, 1 << HashLookup.groundLayer);
    }

    private void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        if ((target.position.x < transform.position.x && FacingForward) || (target.position.x > transform.position.x && !FacingForward))
        {
            FacingForward = !FacingForward;
        }
    }

    private void HandleWalls()
    {
        if (isAirborne && ComputeGroundAngle() > controller.slopeLimit)
        {
            Vector3 foot1 = ColliderPos + new Vector3(-ColliderRadius, -ColliderHeight / 2 + ColliderRadius, 0);
            Vector3 foot2 = ColliderPos + new Vector3(ColliderRadius, -ColliderHeight / 2 + ColliderRadius, 0);

            RaycastHit hitInfo;
            float f1Dist, f2Dist;
            f1Dist = f2Dist = Mathf.Infinity;

            if (Physics.Raycast(foot1, Vector3.down, out hitInfo, 4f, 1 << HashLookup.groundLayer))
            {
                f1Dist = hitInfo.distance;
            }
            if (Physics.Raycast(foot2, Vector3.down, out hitInfo, 4f, 1 << HashLookup.groundLayer))
            {
                f2Dist = hitInfo.distance;
            }

            if (f1Dist < 0.5f || f2Dist < 0.5f)
            {
                int pushDir = f1Dist >= f2Dist ? -1 : 1;
                float pushX = pushDir * 2f * Time.deltaTime;
                Move(new Vector3(pushX, 0, 0));
            }
        }
    }

    private float ComputeGroundAngle()
    {
        Vector3 foot1 = ColliderPos + new Vector3(-ColliderRadius, -ColliderHeight / 2 + ColliderRadius, 0);
        Vector3 foot2 = ColliderPos + new Vector3(ColliderRadius, -ColliderHeight / 2 + ColliderRadius, 0);

        RaycastHit hitInfo;
        float f1Dist, f2Dist;
        f1Dist = f2Dist = Mathf.Infinity;

        if (Physics.Raycast(foot1, Vector3.down, out hitInfo, 4f, 1 << HashLookup.groundLayer))
        {
            f1Dist = hitInfo.distance;
            Debug.DrawLine(foot1, hitInfo.point, Color.blue, 2);
        }
        if (Physics.Raycast(foot2, Vector3.down, out hitInfo, 4f, 1 << HashLookup.groundLayer))
        {
            f2Dist = hitInfo.distance;
            Debug.DrawLine(foot2, hitInfo.point, Color.blue, 2);
        }



        if (f1Dist < 1f || f2Dist < 1f)
        {
            float deltaX = 0.1f;
            Vector3 footOffset;
            float offsetDistance = Mathf.Infinity;
            if (f1Dist < f2Dist)
            {
                footOffset = foot1 + new Vector3(deltaX, 0, 0);

                if (Physics.Raycast(footOffset, Vector3.down, out hitInfo, 4f, 1 << HashLookup.groundLayer))
                {
                    offsetDistance = hitInfo.distance - f1Dist;
                }
            }
            else
            {
                footOffset = foot2 - new Vector3(deltaX, 0, 0);

                if (Physics.Raycast(footOffset, Vector3.down, out hitInfo, 4f, 1 << HashLookup.groundLayer))
                {
                    offsetDistance = hitInfo.distance - f2Dist;
                }

            }

            float angle = Mathf.Atan2(offsetDistance, deltaX) * Mathf.Rad2Deg;
            return angle;
        }

        return -1;
    }

    public void Stun(float duration)
    {
        Damageable d = GetComponent<Damageable>();

        if (d == null || (d != null && !d.IsDead))
        {
            isStunned = true;
            StartCoroutine(StunRemove(duration));
            animator.CrossFade(HashLookup.stunHash, 0.1f, 0, 0);
        }
    }

    private IEnumerator StunRemove(float time)
    {
        yield return new WaitForSeconds(time);

        Damageable d = GetComponent<Damageable>();

        if (d == null || (d != null && !d.IsDead))
        {
            isStunned = false;
            animator.CrossFade(HashLookup.idleMoveHash, 0.1f, 0, 0);
        }
    }

}
