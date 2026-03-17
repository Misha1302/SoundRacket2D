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
        var power01 = inputModeController == null ? 0f : inputModeController.CurrentPower01;
        if (power01 > 0f)
        {
            rb.AddForce(Vector2.up * (power01 * maxUpwardForce), ForceMode2D.Force);
        }
        else
        {
            var velocity = rb.velocity;
            if (velocity.y > 0f)
            {
                velocity.y = Mathf.MoveTowards(velocity.y, 0f, releaseDamping * Time.fixedDeltaTime);
                rb.velocity = velocity;
            }
        }

        if (rb.velocity.y > maxVerticalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxVerticalSpeed);
        }
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
