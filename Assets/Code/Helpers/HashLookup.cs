using UnityEngine;
using System.Collections;

public static class HashLookup {

    //girl states
    public static int combo1Hash = Animator.StringToHash("Base.Combo.Combo1");
    public static int combo2Hash = Animator.StringToHash("Base.Combo.Combo2");
    public static int combo3Hash = Animator.StringToHash("Base.Combo.Combo3");
    public static int combo4Hash = Animator.StringToHash("Base.Combo.Combo4");

    public static int dashGroundHash = Animator.StringToHash("Base.Dashing.DashGround");
    public static int endDashGroundHash = Animator.StringToHash("Base.Dashing.EndDashGround");
    public static int dashAirHash = Animator.StringToHash("Base.Dashing.DashAir");
    public static int endDashAirHash = Animator.StringToHash("Base.Dashing.EndDashAir");

    public static int idleMoveHash = Animator.StringToHash("Base.IdleMove");
    public static int loopJumpHash = Animator.StringToHash("Base.Jumping.LoopJump");

    public static int startJumpHash = Animator.StringToHash("Base.Jumping.StartJump");
    public static int blockHash = Animator.StringToHash("Base.Block");

    public static int airAttackHash = Animator.StringToHash("Base.Jumping.AirAttack");
    public static int airSpecialHash = Animator.StringToHash("Base.Jumping.AirSpecial");

    public static int slamHash = Animator.StringToHash("Base.Abilities.Slam");
    public static int tornadoHash = Animator.StringToHash("Base.Abilities.Tornado");

    //animation params
    public static int speedBlendHash = Animator.StringToHash("SpeedBlend");
    public static int nextActionHash = Animator.StringToHash("NextAction");
    public static int isBlockingHash = Animator.StringToHash("IsBlocking");
    public static int isAirborneHash = Animator.StringToHash("IsAirborne");
    public static int liftLegsHash = Animator.StringToHash("LiftLegs");

    //enemies
    public static int attackHash = Animator.StringToHash("Base.Attack");
    public static int deathHash = Animator.StringToHash("Base.Death");
    public static int stunHash = Animator.StringToHash("Base.Stunned");
    public static int blockIdleMoveHash = Animator.StringToHash("Base.BlockIdleMove");
    public static int shieldingHash = Animator.StringToHash("Base.Shielding");

    //jumppad
    public static int actionHash = Animator.StringToHash("Base.Action");

    //layers
    public static int groundLayer = LayerMask.NameToLayer("Ground");
    public static int playerLayer = LayerMask.NameToLayer("Player");
    public static int enemyLayer = LayerMask.NameToLayer("Enemy");
    public static int wallLayer = LayerMask.NameToLayer("Wall");
    public static int pushFieldLayer = LayerMask.NameToLayer("PushField");

    //masks
    public static int EnemiesMask
    {
        get { return (1 << HashLookup.enemyLayer); }
    }
}
