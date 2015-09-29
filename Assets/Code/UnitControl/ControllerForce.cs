using UnityEngine;
using System.Collections;

public class ControllerForce {

    public float accelY;
    public float minVeloYActive;
    public float maxVeloYActive;
    public bool cancelGravity;

    public bool InActiveYRange(float velo)
    {
        return velo < maxVeloYActive && velo > minVeloYActive;
    }

}
