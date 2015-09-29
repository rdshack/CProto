using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wall : MonoBehaviour {


    private List<CharacterController> overlapList;

    private float pushStrength = 1.5f;

	// Use this for initialization
	void Start () {
        overlapList = new List<CharacterController>(5);
	}
	
	// Update is called once per frame
	void Update () {
        foreach (CharacterController c in overlapList)
        {
            if(c.transform.position.x > transform.position.x)
                c.Move(new Vector3(pushStrength * Time.deltaTime, 0, 0));
            else
                c.Move(new Vector3(-pushStrength * Time.deltaTime, 0, 0));
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == HashLookup.playerLayer || other.gameObject.layer == HashLookup.enemyLayer)
        {
            overlapList.Add(other.gameObject.GetComponent<CharacterController>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == HashLookup.playerLayer || other.gameObject.layer == HashLookup.enemyLayer)
        {
            overlapList.Remove(other.gameObject.GetComponent<CharacterController>());
        }
    }
}
