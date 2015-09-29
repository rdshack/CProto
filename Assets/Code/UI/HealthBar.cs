using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour {

    public Damageable followTarget;

    private RectTransform rect;
    private float startLength;
    private float startHeight;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

	void Start () {
        startLength = rect.sizeDelta.x;
        startHeight = rect.sizeDelta.y;
	}
	
	// Update is called once per frame
	void Update () {
        if (followTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        float newLength = followTarget.GetPercentHP() * startLength;
        Vector2 screenPoints = RectTransformUtility.WorldToScreenPoint(Camera.main, followTarget.transform.position);

        screenPoints.y += 50;
        screenPoints.x -= (startLength - newLength) / 2;
        GetComponent<RectTransform>().position = screenPoints;

        
        rect.sizeDelta = new Vector2(newLength, startHeight);
	}
}
