using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Transform followTarget;
    public Vector2 focusOffset;

    public float distanceFactor = 1f;
    public float panTime = 2f;
    public float zoomTime = 2f;

    public bool useMotionPrediction = true;
    private Vector2 followTargetPreviousPosition;

    public float motionPredictionRangeX = 3f;
    private float curPredictionOffsetX = 0;
    private int previousDirX = 0;
    private float predictionDelayExpireTimeX = 0;

    public float motionPredictionRangeY = 3f;
    private float curPredictionOffsetY = 0;
    private int previousDirY = 0;
    private float predictionDelayExpireTimeY = 0;

    public float predictionMoveSpeedX = 3f;
    public float predictionMoveSpeedY = 2f;

    private float reset_distanceFactor = 0;
    private float reset_panTime = 0;
    private float reset_zoomTime = 0;
    private Vector2 reset_focusOffset;

    private float startCamLookDist = 0f;
    private float focusRange = 0.8f;

	// Use this for initialization
    void Start()
    {
        float intersectDistance;
        Plane xyPlane = new Plane(Vector3.back, Vector3.zero);
        xyPlane.Raycast(new Ray(transform.position, transform.forward), out intersectDistance);
        startCamLookDist = intersectDistance;

        //set up reset values
        reset_panTime = panTime;
        reset_distanceFactor = distanceFactor;
        reset_zoomTime = zoomTime;
        reset_focusOffset = focusOffset;

        followTargetPreviousPosition = followTarget.position;
    }
	
	// Update is called once per frame
    void LateUpdate()
    {
        PredictTargetMotion();
        UpdateCamPos();
    }

    private void PredictTargetMotion()
    {
        if (useMotionPrediction)
        {
            int newDirX = GetPlayerDirX();
            if (previousDirX != newDirX)
            {
                if ((curPredictionOffsetX > 0 && newDirX == -1) || (curPredictionOffsetX < 0 && newDirX == 1))
                {
                    predictionDelayExpireTimeX = Time.time + 0.15f;
                }
                else
                {
                    predictionDelayExpireTimeX = Time.time + 0.35f;
                }
                
                previousDirX = newDirX;
            }
            else if (Time.time > predictionDelayExpireTimeX)
            {
                if (previousDirX == 0)
                {
                    if (curPredictionOffsetX > 0)
                    {
                        curPredictionOffsetX = Mathf.Max(curPredictionOffsetX - predictionMoveSpeedX * Time.deltaTime, 0);
                    }
                    else
                    {
                        curPredictionOffsetX = Mathf.Min(curPredictionOffsetX + predictionMoveSpeedX * Time.deltaTime, 0);
                    }
                }
                else if (previousDirX == 1)
                {
                    curPredictionOffsetX = Mathf.Min(curPredictionOffsetX + predictionMoveSpeedX * Time.deltaTime, motionPredictionRangeX);
                }
                else
                {
                    curPredictionOffsetX = Mathf.Max(curPredictionOffsetX - predictionMoveSpeedX * Time.deltaTime, -motionPredictionRangeX);
                }
            }


            int newDirY = GetPlayerDirY();
            if (previousDirY != newDirY)
            {
                if ((curPredictionOffsetY > 0 && newDirY == -1) || (curPredictionOffsetY < 0 && newDirY == 1))
                {
                    predictionDelayExpireTimeY = Time.time + 0.15f;
                }
                else
                {
                    predictionDelayExpireTimeY = Time.time + 0.35f;
                }

                previousDirY = newDirY;
            }
            else if (Time.time > predictionDelayExpireTimeY)
            {
                if (previousDirY == 0)
                {
                    if (curPredictionOffsetY > 0)
                    {
                        curPredictionOffsetY = Mathf.Max(curPredictionOffsetY - predictionMoveSpeedY * Time.deltaTime, 0);
                    }
                    else
                    {
                        curPredictionOffsetY = Mathf.Min(curPredictionOffsetY + predictionMoveSpeedY * Time.deltaTime, 0);
                    }
                }
                else if (previousDirY == 1)
                {
                    curPredictionOffsetY = Mathf.Min(curPredictionOffsetY + predictionMoveSpeedY * Time.deltaTime, motionPredictionRangeY);
                }
                else
                {
                    curPredictionOffsetY = Mathf.Max(curPredictionOffsetY - predictionMoveSpeedY * Time.deltaTime, -motionPredictionRangeY);
                }
            }
        }
        else
        {
            curPredictionOffsetX = 0;
            curPredictionOffsetY = 0;
        }

        followTargetPreviousPosition = followTarget.position;
    }

    private int GetPlayerDirX()
    {
        if (followTargetPreviousPosition.x == followTarget.position.x)
        {
            return 0;
        }
        else if (followTargetPreviousPosition.x < followTarget.position.x)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    private int GetPlayerDirY()
    {
        if (followTargetPreviousPosition.y == followTarget.position.y)
        {
            return 0;
        }
        else if (followTargetPreviousPosition.y < followTarget.position.y)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }


    void UpdateCamPos()
    {
        float camLookDist;
        Plane xyPlane = new Plane(Vector3.back, Vector3.zero);
        xyPlane.Raycast(new Ray(transform.position, transform.forward), out camLookDist);

        Vector2 camLookIntersect = transform.position + transform.forward * camLookDist;
        Vector2 camLookTarget = new Vector2(followTarget.position.x, followTarget.position.y) + focusOffset;

        float offsetX = camLookTarget.x - camLookIntersect.x + curPredictionOffsetX;
        float offsetY = camLookTarget.y - camLookIntersect.y + curPredictionOffsetY;

        float xDist = Mathf.Abs(offsetX);
        float yDist = Mathf.Abs(offsetY);

        float deltaX = (xDist > focusRange) ? offsetX : 0;
        float deltaY = (yDist > focusRange) ? offsetY : 0;

        float curDistFactor = camLookDist / startCamLookDist;
        float factorDiff = distanceFactor - curDistFactor;
        Vector3 zoomDelta = -transform.forward * startCamLookDist * factorDiff;

        if (Mathf.Abs(deltaX) > 0.01f || Mathf.Abs(deltaY) > 0.01f)
        {
            iTween.MoveUpdate(gameObject, transform.position + new Vector3(deltaX, deltaY, 0), panTime);
        }

        if (Mathf.Abs(zoomDelta.z) > 0.01f)
        {
            iTween.MoveUpdate(gameObject, transform.position + zoomDelta, zoomTime);
        }
        
    }

    public void ResetPanTime(float lerpTime = 2)
    {
        iTween.StopByName("panTime");
        iTween.ValueTo(gameObject, iTween.Hash("name", "panTime", "from", panTime, "to", reset_panTime, "time", lerpTime, "easetype", iTween.EaseType.linear, "onupdate", "UpdatePanTime"));
    }

    private void UpdatePanTime(float newValue)
    {
        panTime = newValue;
    }
    public void ResetZoomTime(float lerpTime = 2)
    {
        iTween.StopByName("zoomTime");
        iTween.ValueTo(gameObject, iTween.Hash("name", "zoomTime", "from", zoomTime, "to", reset_zoomTime, "time", lerpTime, "easetype", iTween.EaseType.linear, "onupdate", "UpdateZoomTime"));
    }

    private void UpdateZoomTime(float newValue)
    {
        zoomTime = newValue;
    }

    public void ResetFocusOffset()
    {
        focusOffset = reset_focusOffset;
    }

    public void ResetDistanceFactor()
    {
        distanceFactor = reset_distanceFactor;
    }

    public void ResetAll()
    {
        ResetZoomTime();
        ResetPanTime();
        ResetFocusOffset();
        ResetDistanceFactor();
    }



}
