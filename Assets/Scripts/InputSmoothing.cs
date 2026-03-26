using UnityEngine;

public static class InputSmoothing
{
    public static float SmoothExp(float current, float target, float riseSpeed, float fallSpeed, float deltaTime)
    {
        float speed = target > current ? riseSpeed : fallSpeed;
        float t = 1f - Mathf.Exp(-speed * deltaTime);
        return Mathf.Lerp(current, target, t);
    }
}