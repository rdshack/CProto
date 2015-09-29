using UnityEngine;
using System.Collections;

public static class TargetFinder {

    public static Transform[] SphereFind(Vector3 position, float radius, int mask)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius, mask);
        Transform[] targets = new Transform[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            targets[i] = hits[i].transform;
        }

        return targets;
    }

    public static Affector SpawnAffector(Vector3 position, float radius, int targetMask)
    {
        GameObject go = MonoBehaviour.Instantiate(Resources.Load("Affector", typeof(GameObject))) as GameObject;
        go.transform.position = position;
        Affector a = go.GetComponent<Affector>();
        a.targetMask = targetMask;
        a.SetRadius(radius);

        return a;
    }

}
