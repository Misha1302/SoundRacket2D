using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class RocketFlightController : MonoBehaviour
{
    [SerializeField] private RocketInputModeController inputModeController;
    [SerializeField, Min(0f)] private float maxUpwardForce = 15f;
    [SerializeField, Min(0.1f)] private float maxVerticalSpeed = 8f;
    [SerializeField, Min(0f)] private float releaseDamping = 2.5f;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    public float HeightFromStart => transform.position.y - startPosition.y;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        var power01 = inputModeController.CurrentPower01;
        rb.velocity = new Vector2(rb.velocity.x, power01 * maxVerticalSpeed);
    }

    public void ResetToStart()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.SetPositionAndRotation(startPosition, startRotation);
    }

    public void SetCurrentAsStart()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }
}
