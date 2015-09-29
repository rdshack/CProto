using UnityEngine;
using System.Collections;

public class CamTrigger : MonoBehaviour {

    public Vector2 focusOffset;
    public float distanceFactor, zoomTime, panTime;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == HashLookup.playerLayer)
        {
            CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
            cf.focusOffset = focusOffset;
            cf.distanceFactor = distanceFactor;
            cf.zoomTime = zoomTime;
            cf.panTime = panTime;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == HashLookup.playerLayer)
        {
            CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
            cf.ResetAll();
        }
    }
}
