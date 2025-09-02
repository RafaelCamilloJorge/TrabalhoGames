using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveTorque = 200f;          // ↑ torque = aceleração mais forte
    [SerializeField] float maxAngularVelocity = 200f;
    [SerializeField] float maxSpeed = 200f;            // ↑ teto de velocidade

    [Header("2D-like Handling (freio)")]
    [SerializeField] float brakeGain = 0.8f;          // ↓ freio contínuo (menos freio = mais rápido)
    [SerializeField] float passiveFrictionAccel = 3f; // ↓ atrito sem input
    [SerializeField] float oppositeInputBrakeBoost = 2f;
    [SerializeField] float inputDeadzone = 0.1f;
    [SerializeField] float noInputAngularDrag = 3f;

    [Header("Boost")]
    [SerializeField] float assistForceAccel = 1000f;    // força extra além do torque
    [SerializeField] float sprintMultiplier = 1.6f;   // Shift/RT = turbo

    [Header("Ground Check")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckDistance = 0.6f;

    [Header("Jump (opcional)")]
    [SerializeField] bool enableJump = false;
    [SerializeField] float jumpImpulse = 6f;

    Rigidbody rb;
    float baseAngularDrag;

    InputAction moveAction, jumpAction;
    bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.maxAngularVelocity = maxAngularVelocity;
        baseAngularDrag = rb.angularDamping;

        moveAction = new InputAction("Move", InputActionType.Value);
        var comp = moveAction.AddCompositeBinding("2DVector");
        comp.With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        comp.With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable() { moveAction.Enable(); jumpAction.Enable(); }
    void OnDisable() { moveAction.Disable(); jumpAction.Disable(); }

    void Update()
    {
        if (enableJump && jumpAction.triggered) jumpRequested = true;
    }

    void FixedUpdate()
    {
        Vector2 mv = moveAction.ReadValue<Vector2>();
        Vector3 dirPlanar = new Vector3(mv.x, 0f, mv.y);
        bool hasInput = dirPlanar.sqrMagnitude > (inputDeadzone * inputDeadzone);

        // Sprint: Shift (kb) ou RT (gamepad)
        bool sprint = (Keyboard.current?.leftShiftKey.isPressed ?? false) ||
                      (Gamepad.current?.rightTrigger.ReadValue() > 0.5f);
        float spdMul = sprint ? sprintMultiplier : 1f;

        // Movimento por torque (rolagem) + força assistida (mais punch)
        Vector3 torque = new Vector3(dirPlanar.z, 0f, -dirPlanar.x) * moveTorque * spdMul;
        rb.AddTorque(torque, ForceMode.Acceleration);

        if (hasInput)
            rb.AddForce(dirPlanar.normalized * assistForceAccel * spdMul, ForceMode.Acceleration);

        // Freios 2D-like (mais leves)
        Vector3 vel = rb.linearVelocity;
        Vector3 planar = new Vector3(vel.x, 0f, vel.z);
        float planarMag = planar.magnitude;

        if (planarMag > 0.001f)
            rb.AddForce(-planar * brakeGain, ForceMode.Acceleration);

        if (!hasInput && IsGrounded() && planarMag > 0.05f)
            rb.AddForce(-planar.normalized * passiveFrictionAccel, ForceMode.Acceleration);

        if (hasInput && planarMag > 0.1f && Vector3.Dot(planar.normalized, dirPlanar.normalized) < 0f)
            rb.AddForce(-planar.normalized * oppositeInputBrakeBoost, ForceMode.Acceleration);

        rb.angularDamping = hasInput ? baseAngularDrag : noInputAngularDrag;

        // Clamp de velocidade (respeita sprint)
        float maxPlanar = maxSpeed * spdMul;
        if (planarMag > maxPlanar)
        {
            Vector3 capped = planar.normalized * maxPlanar;
            rb.linearVelocity = new Vector3(capped.x, vel.y, capped.z);
        }

        if (enableJump && jumpRequested && IsGrounded())
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.VelocityChange);
        jumpRequested = false;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f,
                               Vector3.down, groundCheckDistance, groundMask,
                               QueryTriggerInteraction.Ignore);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Star"))
            other.gameObject.SetActive(false);
    }
}
