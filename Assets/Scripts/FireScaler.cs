using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScaler : MonoBehaviour
{
    [SerializeField] private float scale;
    [SerializeField] private RocketFlightController controller;
    [SerializeField] private Transform fire;

    
    void LateUpdate()
    {
        fire.localScale = new Vector3(
            fire.localScale.x,
            controller.CurrentSpeed01 * scale,
            fire.localScale.z
        );
    }
}
