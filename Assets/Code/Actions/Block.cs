using UnityEngine;
using System.Collections;

public class Block : BaseAction {




	// Use this for initialization
	public override void Start () {
        base.Start();
        canTurn = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void ActiveUpdate()
    {
        if (!Input.GetMouseButton(1))
        {
            isActive = false;
            animator.CrossFade(HashLookup.idleMoveHash, 0.1f);
        }
    }

    public override void Execute()
    {
        isActive = true;
        animator.CrossFade(HashLookup.blockHash, 0.06f, 0, 0);
    }

    public override bool IsQueueable()
    {
        return false;
    }

    public override bool IsDisabled()
    {
        return gc.IsAirborne;
    }

     
}
