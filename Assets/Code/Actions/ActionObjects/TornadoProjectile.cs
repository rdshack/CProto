using UnityEngine;
using System.Collections;

public class TornadoProjectile : MonoBehaviour {

    private TornadoProjStates state;

    private int initialDirection;
    private float curVeloX;
    private float curVeloY;
    private float accelX;
    private float accelY = 0.15f;
    private Transform thrower;

    private Vector2 spawnOffset = new Vector2(1, 1.5f);
    private Affector affector;

    public float startSpeed = 5f;
    public float decel = 3f;
    public float lifetime = 5f;
    private float deathTime;

    private float lastPosX = Mathf.Infinity;

    private CharacterController controller;

    private Transform tempEffect = null;
    public Transform effectPrefab;

    private enum TornadoProjStates
    {
        Outgoing,
        Return
    }

	void Awake () {
        controller = GetComponent<CharacterController>();
	}

    void Start()
    {
        tempEffect = Instantiate(effectPrefab);
        tempEffect.transform.position = transform.position;
        tempEffect.SetParent(gameObject.transform);

        deathTime = Time.time + lifetime;
    }
	
	void Update () {

        if (state == TornadoProjStates.Outgoing)
        {
            curVeloX += accelX * Time.deltaTime;
            controller.Move(new Vector3(curVeloX * Time.deltaTime, 0, 0));

            if (Mathf.Abs(lastPosX - transform.position.x) * Time.deltaTime < 0.001f)
            {
                curVeloX *= -0.7f;
                state = TornadoProjStates.Return;
                gameObject.layer = LayerMask.NameToLayer("NoCollide");
                affector.ResetTouches();
            }

            if ((curVeloX < 0 && initialDirection > 0) || (curVeloX > 0 && initialDirection < 0))
            {
                state = TornadoProjStates.Return;
                gameObject.layer = LayerMask.NameToLayer("NoCollide");
                affector.ResetTouches();
            }

            lastPosX = transform.position.x;
        }

        if (state == TornadoProjStates.Return)
        {
            curVeloX += accelX * Time.deltaTime;

            if (thrower.position.y + spawnOffset.y > transform.position.y)
            {
                curVeloY += accelY;
            }
            else if (thrower.position.y + spawnOffset.y < transform.position.y)
            {
                curVeloY -= accelY;
            }

            controller.Move(new Vector3(curVeloX * Time.deltaTime, curVeloY * Time.deltaTime, 0));

            if (lastPosX == transform.position.x)
            {
                Destroy(gameObject);
            }

            lastPosX = transform.position.x;

            if (Vector3.Distance(thrower.position + new Vector3(0, spawnOffset.y, 0), transform.position) < 1.3f)
            {
                Destroy(gameObject);
            }
        }

        if (tempEffect != null)
        {
            Vector3 curAngles = tempEffect.localEulerAngles;
            tempEffect.localEulerAngles = new Vector3(curAngles.x, curAngles.y + 500f * Time.deltaTime, curAngles.z);
        }

        if (Time.time > deathTime)
        {
            Destroy(gameObject);
        }
	}

    public void Throw(int dir, Transform source)
    {
        if (dir > 0)
        {
            initialDirection = 1;
        }
        else
        {
            initialDirection = -1;
        }

        curVeloX = startSpeed * initialDirection;
        curVeloY = 0;
        accelX = decel * dir * -1;

        transform.position = source.position + new Vector3(dir * spawnOffset.x, spawnOffset.y, 0);

        Affector a = TargetFinder.SpawnAffector(transform.position, 2f, HashLookup.EnemiesMask);
        a.OnTouch += TornadoTouchHandler;
        a.SetLifetime(lifetime);
        a.transform.SetParent(transform);
        affector = a;

        state = TornadoProjStates.Outgoing;
        thrower = source;
    }

    public void TornadoTouchHandler(Transform target)
    {
        target.GetComponent<Damageable>().TakeDamage(40, gameObject);
    }

}
