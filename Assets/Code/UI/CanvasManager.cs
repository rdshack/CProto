using UnityEngine;
using System.Collections;

public class CanvasManager : MonoBehaviour {

    private static CanvasManager _instance;

    public Transform healthBarPrefab;

    private static CanvasManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<CanvasManager>();
            }
            return _instance;
        }
    }

    public static void RequestHealthbar(Damageable d)
    {
        HealthBar newHealthBar = Instantiate(instance.healthBarPrefab).GetComponent<HealthBar>();
        newHealthBar.followTarget = d;
        newHealthBar.transform.SetParent(instance.transform, false);

    }
}
