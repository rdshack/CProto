using UnityEngine;
using System.Collections;

public class HitEffect : MonoBehaviour {

    public Material material;
    public Animator animator;
    private Material defaultMaterial;
    public SkinnedMeshRenderer mesh;

    public Transform girl;

    private static int hitHash = Animator.StringToHash("Hit");

	// Use this for initialization
	void Start () {
        defaultMaterial = mesh.material;
	}

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "HitBox")
        {
            StartCoroutine(MaterialFlash(0.05f));
            animator.SetTrigger(hitHash);
            Destroy(coll.gameObject);
        }
    }

    IEnumerator MaterialFlash(float wait)
    {
        mesh.material = material;
        yield return new WaitForSeconds(wait);
        mesh.material = defaultMaterial;
    }
}
