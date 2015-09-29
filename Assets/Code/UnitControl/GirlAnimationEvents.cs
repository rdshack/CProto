using UnityEngine;
using System.Collections;

public class GirlAnimationEvents : MonoBehaviour {

    private BaseController controller;
    private GirlController gc;
    private Combo combo;
    public Transform attackOrigin;

    private Transform damageTarget = null;

	// Use this for initialization
    void Awake()
    {
        controller = transform.parent.GetComponent<BaseController>();
        combo = transform.parent.GetComponent<Combo>();
        gc = transform.parent.GetComponent<GirlController>();
    }

    public void Combo1Hit()
    {
        //First, find possible targets
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, 2f, HashLookup.EnemiesMask);
        Transform[] possibleTargets = new Transform[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            possibleTargets[i] = hits[i].transform;
        }

        //Find closest target
        damageTarget = gc.FindTarget(possibleTargets, false, false, damageTarget);

        if (damageTarget != null)
        {
            //If attack is aoe, deal aoe damage to nearby enemies
            if (combo.GetCurrentComboStep().aoeRadius > 0)
            {
                hits = Physics.OverlapSphere(damageTarget.position, combo.GetCurrentComboStep().aoeRadius, HashLookup.EnemiesMask);
                foreach (Collider col in hits)
                {
                    if (col.transform != damageTarget)
                    {
                        col.GetComponent<Damageable>().TakeDamage(combo.damage * combo.GetCurrentComboStep().aoePercent, gameObject);
                    }
                }
            }

            //if this attack has a visual impact effect, play it
            if (combo.GetCurrentComboStep().impactEffectPrefab != null)
            {
                Instantiate(combo.GetCurrentComboStep().impactEffectPrefab, attackOrigin.position - new Vector3(0, 0.5f, 0), transform.rotation);
            }

            //deal damage to the primary target
            damageTarget.GetComponent<Damageable>().TakeDamage(combo.damage, gameObject);
        }

    }

    public void Combo2Hit()
    {
        Combo1Hit();
    }

    public void Combo3Hit()
    {
        Combo1Hit();
    }

    public void Combo4Hit()
    {
        Combo1Hit();
    }
}
