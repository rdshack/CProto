using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Affector : MonoBehaviour {

    public int targetMask;
    public float startDelay;
    public int numIntervals = 1;
    public float intervalDuration = 0;
    public int touchesPerTarget = 1;

    private float lifetime = Mathf.Infinity;
    private bool useLifetime = false;

    public event IntervalHandler OnInterval;
    public event EmptyIntervalHandler OnEmptyInterval;
    public delegate void IntervalHandler(Transform[] targets);
    public delegate void EmptyIntervalHandler();
    public event TouchHandler OnTouch;
    public delegate void TouchHandler(Transform target);

    private float spawnTime;
    private float lastIntervalTime;
    private int intervalCount = 0;
    private AffectorStates curState;
    private SphereCollider collider;
    private Dictionary<Transform, AffectorEntry> curTargets;


    private enum AffectorStates
    {
        Delay,
        Interval
    }

    private class AffectorEntry
    {
        public bool overlap = true;
        public int touchCount = 1;

        public AffectorEntry(bool overlap, int touchCount)
        {
            this.overlap = overlap;
            this.touchCount = touchCount;
        }
    }

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
        spawnTime = Time.time;
        curState = AffectorStates.Delay;
    }

	// Use this for initialization
	void Start () {
        curTargets = new Dictionary<Transform, AffectorEntry>();
	}
	
	// Update is called once per frame
	void Update () {

        if(IsFinished()) {
            Destroy(gameObject);
            return;
        }

        if (curState == AffectorStates.Delay)
        {
            if (Time.time >= spawnTime + startDelay)
            {
                SendIntervalEvent();

                if (numIntervals > 1)
                {
                    lastIntervalTime = Time.time;
                    intervalCount = 1;
                }

                curState = AffectorStates.Interval;
            } 
        }
        else if (curState == AffectorStates.Interval)
        {
            if (Time.time > lastIntervalTime + intervalDuration)
            {
                SendIntervalEvent();

                if (numIntervals > intervalCount)
                {
                    lastIntervalTime = Time.time;
                    intervalCount++;
                }
            }
        } 
	}

    private void SendIntervalEvent()
    {
        List<Transform> targets = new List<Transform>();
        foreach (KeyValuePair<Transform, AffectorEntry> pair in curTargets)
        {
            if (pair.Value.overlap)
            {
                targets.Add(pair.Key);
            }
        }

        if (targets.Count > 0)
        {
            if (OnInterval != null)
            {
                OnInterval(targets.ToArray());
            }
        }
        else
        {
            if (OnEmptyInterval != null)
            {
                OnEmptyInterval();
            }
        }   
    }

    public void SetRadius(float radius)
    {
        collider.radius = radius;
    }

    public void ResetTouches()
    {
        foreach (AffectorEntry entry in curTargets.Values)
        {
            entry.touchCount = 0;
        }
    }

    public void SetLifetime(float lifetime)
    {
        this.lifetime = lifetime;
        useLifetime = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & targetMask) != 0)
        {
            if (curTargets.ContainsKey(other.transform))
            {
                AffectorEntry entry = curTargets[other.transform];
                entry.touchCount++;
                entry.overlap = true;

                if (entry.touchCount <= touchesPerTarget)
                {
                    if (OnTouch != null)
                    {
                        OnTouch(other.transform);
                    }        
                }
            }
            else
            {
                curTargets.Add(other.transform, new AffectorEntry(true, 1));

                if (touchesPerTarget > 0)
                {
                    if (OnTouch != null)
                    {
                        OnTouch(other.transform);
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer & targetMask) != 0)
        {
            if (curTargets.ContainsKey(other.transform))
            {
                curTargets[other.transform].overlap = false;
            }
        }
    }

    private bool IsFinished()
    {
        if (useLifetime)
        {
            return (Time.time > spawnTime + lifetime);
        }
        else
        {
            if (intervalCount >= numIntervals)
            {
                return true;
            }
        }

        return false;

    }
}
