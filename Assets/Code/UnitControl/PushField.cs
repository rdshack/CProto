using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PushField : MonoBehaviour {

    private CharacterController charController;
    private BaseController baseController;
    public Transform player;

    public float pushStrength = 1.5f;
    private List<PushField> overlapList;

    //we only interact with pushfields of the same line ID
    public LineIDName lineID = 0;

    public bool Active { get; set; }

    public enum LineIDName
    {
        Center,
        Near,
        Far
    }

    public Vector3 ColliderPos
    {
        get { return charController.transform.position + charController.center; }
    }

    void Awake()
    {
        charController = transform.parent.GetComponent<CharacterController>();
        baseController = transform.parent.GetComponent<BaseController>();
    }


	void Start () {

        //match push field's collider to the object it is attached to
        CapsuleCollider pushBackCapsule = GetComponent<CapsuleCollider>();
        pushBackCapsule.center = charController.center;
        pushBackCapsule.height = charController.height;
        pushBackCapsule.radius = charController.radius;

        overlapList = new List<PushField>(10);
        Active = true;

        if (transform.parent.position.z > 0)
        {
            lineID = LineIDName.Far;
        } 
        else if (transform.parent.position.z < 0) 
        {
            lineID = LineIDName.Near;
        }
        else
        {
            lineID = LineIDName.Center;
        }

	}
	
	void FixedUpdate () {
        HandleOverlap();
	}

    private void HandleOverlap()
    {
        if (!Active)
            return;

        float minDist = Mathf.Infinity;
        PushField closest = null;

        //Find closest overlapping unit
        foreach (PushField ep in overlapList)
        {
            if (ep == null || ep.lineID != this.lineID)
            {
                continue;
            }
                
            //only check active push fields with a similar velocity
            if (ep.Active && SimilarVelocity(ep))
            {
                float dist = Mathf.Abs(ep.charController.transform.position.x + ep.charController.center.x - ColliderPos.x);

                if (dist < minDist)
                {
                    minDist = dist;
                    closest = ep;
                }
            }
        }

        //push away from the closest valid target
        if (closest != null)
        {
            int dir;
            if (closest.transform.position.x + closest.charController.center.x <= ColliderPos.x)
                dir = 1;
            else
                dir = -1;

            float pushX = pushStrength * Time.deltaTime * dir;

            //push away, but never push into the player (only the player's push field will push him)
            baseController.Move(new Vector3(pushX, 0, 0), 1 << HashLookup.playerLayer);
        }


  
    }

    //are we moving a similar velocity?
    private bool SimilarVelocity(PushField other)
    {
        float myVelo = baseController.TotalVeloX;
        float otherVelo = other.transform.parent.GetComponent<BaseController>().TotalVeloX;
        if (Mathf.Abs(myVelo - otherVelo) <= 0.5f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //are we on a ledge? ----REWRITE
    /*private bool DetectLedge(bool lookRight)
    {
        int layerMask = 1 << HashLookup.groundLayer;
        Vector3 raycastStart = ColliderPos + new Vector3(charController.radius, 0, 0) * (lookRight ? 1 : -1);

        return !Physics.Raycast(raycastStart, Vector3.down, charController.height / 2 + 0.5f, layerMask);
    }*/

    //add overlapping push fields to list
    void OnTriggerEnter(Collider other)
    {

        PushField otherPush = other.GetComponentInChildren<PushField>();

        if (otherPush == this || otherPush.lineID != this.lineID || overlapList.Contains(otherPush))
            return;

        if (other.gameObject.layer == HashLookup.enemyLayer || other.gameObject.layer == HashLookup.playerLayer)
        {
            overlapList.Add(otherPush);
            otherPush.TryAdd(this);
        }
    }

    //remove pushfields from list
    void OnTriggerExit(Collider other)
    {
        PushField otherPush = other.GetComponentInChildren<PushField>();


        if (other.gameObject.layer == HashLookup.enemyLayer || other.gameObject.layer == HashLookup.playerLayer)
        {
            overlapList.Remove(otherPush);
            otherPush.TryRemove(this);
        }
    }

    //manually add push fields
    public void TryAdd(PushField otherPush) 
    {
        if (otherPush == this || otherPush.lineID != this.lineID || overlapList.Contains(otherPush))
            return;

        overlapList.Add(otherPush);
    }

    //manually remove push fields
    public void TryRemove(PushField otherPush)
    {
        overlapList.Remove(otherPush);
    }

}
