using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class RocketFlightController : MonoBehaviour
{
    [SerializeField] private RocketInputModeController inputModeController;
    [SerializeField, Min(0.1f)] private float maxVerticalSpeed = 8f;

    private Rigidbody2D _rb;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public float HeightFromStart => transform.position.y - _startPosition.y;
    public float MaxVerticalSpeed => maxVerticalSpeed;

    public float CurrentUpwardSpeed
    {
        get
        {
            if (_rb == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, _rb.velocity.y);
        }
    }

    public float CurrentSpeed01
    {
        get
        {
            if (maxVerticalSpeed <= 0.0001f)
            {
                return 0f;
            }

            return Mathf.Clamp01(CurrentUpwardSpeed / maxVerticalSpeed);
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        float power01 = inputModeController == null
            ? 0f
            : inputModeController.CurrentPower01;

        _rb.velocity = new Vector2(_rb.velocity.x, power01 * maxVerticalSpeed);
    }

    public void ResetToStart()
    {
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.SetPositionAndRotation(_startPosition, _startRotation);
    }

    public void SetCurrentAsStart()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }
}