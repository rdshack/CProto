using UnityEngine;
using System.Collections;

public class ShielderAnimationEvents : MonoBehaviour {

    private ShielderController sc;

	// Use this for initialization
	void Awake () {
        sc = transform.parent.GetComponent<ShielderController>();
	}
	
    public void AttackHitFrame()
    {
        sc.AttackFrame();
    }
}
