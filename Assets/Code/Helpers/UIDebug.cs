using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDebug : MonoBehaviour {

    private Text debugText;

    private static UIDebug _instance;

    public static UIDebug instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<UIDebug>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        debugText = GetComponent<Text>();
    }

    public static void Log(object o)
    {
        instance.debugText.text = o.ToString();
    }
}
