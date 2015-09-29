using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour {

    public float maxHealth;
    public bool showHealthBar = true;

    private float curHealth;
    private bool isDead;

    public delegate float DamageValidator(float amount, GameObject source);
    public DamageValidator damageValidator = null;

    public Material hitMaterial;
    private Material defaultMaterial;
    private SkinnedMeshRenderer meshRenderer;

    public delegate void DeathHandler();
    public event DeathHandler deathEvent;

    public bool IsDead { get { return isDead; } }

    void Awake()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        defaultMaterial = meshRenderer.material;
    }

	// Use this for initialization
	void Start () {
        curHealth = maxHealth;

        if (showHealthBar)
        {
            CanvasManager.RequestHealthbar(this);
        }

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void TakeDamage(float damage, GameObject source)
    {
        float takenDamage = damage;

        if (IsDead)
        {
            return;
        }

        if (damageValidator != null)
        {
            takenDamage = damageValidator(damage, source);
        }

        curHealth -= takenDamage;
        if (curHealth <= 0)
        {
            isDead = true;

            if (deathEvent != null)
            {
                deathEvent();
            }
        }

        if (takenDamage > 0)
        {
            StartCoroutine(MaterialFlash(0.065f));
        }
    }

    public float GetPercentHP()
    {
        return ((float)curHealth / (float)maxHealth);
    }

    IEnumerator MaterialFlash(float wait)
    {
        meshRenderer.material = hitMaterial;
        yield return new WaitForSeconds(wait);
        meshRenderer.material = defaultMaterial;
    }

}
