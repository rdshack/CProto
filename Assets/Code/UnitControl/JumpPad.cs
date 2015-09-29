using UnityEngine;
using System.Collections;

public class JumpPad : MonoBehaviour {

    public GirlController player;
    private BoxCollider myCollider;

    public float jumpPadStrength = 7f;

	// Use this for initialization
	void Start () {
        myCollider = GetComponent<BoxCollider>();
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 foot1 = player.ColliderPos + new Vector3(-player.ColliderRadius, -player.ColliderHeight / 2, 0);
        Vector3 foot2 = player.ColliderPos + new Vector3(player.ColliderRadius, -player.ColliderHeight / 2, 0);

        if (myCollider.bounds.Contains(foot1) || myCollider.bounds.Contains(foot2))
        {
            if (player.controller.CurExternalVeloY <= 0)
            {
                player.ActivateJumpPad(jumpPadStrength);
                GetComponent<Animator>().Play(HashLookup.actionHash, 0, 0);
            }
        }

	}

}
