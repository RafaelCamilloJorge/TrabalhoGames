using UnityEngine;
// using UnityEngine.InputSystem; // Removed: switching to Input.GetAxisRaw

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveForce = 1000f;
    [SerializeField] float maxSpeed = 25f;             

    [Header("Respawn")]
    [SerializeField] Vector3 respawnPosition = new Vector3(126f, 99f, 116f);

    [Header("Queda / Gravidade")]
    [SerializeField] float gravityMultiplier = 2f;
    [SerializeField] float linearDrag = 0f; 

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearDamping = linearDrag;
    }


    void Update()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.GameEnded)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.ManualReset(respawnPosition, rb);
            else
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = respawnPosition;
                CollectibleStar.ResetAll();
                ScoreService.Reset();
            }
        }
    }

    void FixedUpdate()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.GameEnded)
            return;

        // Input (Input Manager)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f) input.Normalize();

        rb.AddForce(input * moveForce, ForceMode.Acceleration);

        if (gravityMultiplier != 1f)
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        Vector3 vel = rb.linearVelocity;
        Vector3 planar = new Vector3(vel.x, 0f, vel.z);
        float planarMag = planar.magnitude;
        if (planarMag > maxSpeed)
        {
            Vector3 capped = planar.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(capped.x, vel.y, capped.z);
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider;
        if (other.CompareTag("ChaoPrincipal"))
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.HandlePlayerFall(respawnPosition, rb);
            else
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = respawnPosition;

                CollectibleStar.ResetAll();
                ScoreService.Reset();
            }
        }
    }

}
