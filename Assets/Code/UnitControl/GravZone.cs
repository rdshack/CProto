using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ParticlePlayground;

public class GravZone : MonoBehaviour {


    public float gravAccel = 2f;
    public bool ignoreGravity = false;
    public float minVeloY = -Mathf.Infinity;
    public float maxVeloY = Mathf.Infinity;

    private ControllerForce force;

    void Awake()
    {

    }

	// Use this for initialization
	void Start () {
        force = new ControllerForce();
        force.cancelGravity = ignoreGravity;
        force.accelY = gravAccel;
        force.minVeloYActive = minVeloY;
        force.maxVeloYActive = maxVeloY;

        PlaygroundParticlesC particles = GetComponentInChildren<PlaygroundParticlesC>();
        if (particles != null)
        {
            Vector3 half = transform.localScale * 0.5f; ;
            particles.sourceScatterMax = half;
            particles.sourceScatterMin = -half;
            particles.applySourceScatter = true;

            particles.initialVelocityMin = new Vector3(0, 0, 0);
            particles.initialVelocityMax = new Vector3(0, gravAccel / 5, 0);
            particles.applyInitialVelocity = true;

            particles.particleCount = ((int)((1000 / 70) * (transform.localScale.x * transform.localScale.y * transform.localScale.z)));
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == HashLookup.playerLayer)
        {
            other.GetComponent<BaseController>().AddForce(force);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == HashLookup.playerLayer)
        {
            other.GetComponent<BaseController>().RemoveForce(force);
        }
    }
}
