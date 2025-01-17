using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public interface IShipSpeed
{
    float Speed { get; }
}

public class Ship : MonoBehaviour, IShipSpeed
{
    private Rigidbody2D rb;

    private IGameplayManagerPrefabs gameplayManager;

    [SerializeField] private Transform shootingPoint;

    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float velocityRotationSpeed = 200f;
    [SerializeField] private float thrustStrength = 20f;
    [SerializeField] private float brakeStrength = 1f;
    [SerializeField] private float shootingRate = 5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float friction = .01f;

    private bool shootingEnabled = false;
    private float lastShotTime = -1;

    private Vector2 lastPos = Vector2.zero;
    private float deltaDistance = 0;
    private float deltaVelocity = 0; public float Speed => deltaVelocity;

    private bool isThrusting = false;

    public void Setup(IGameplayManagerPrefabs gameplayManager) {
        rb = GetComponent<Rigidbody2D>();

        this.gameplayManager = gameplayManager;
    }

    // Start is called before the first frame update
    // void Start()
    // {

    // }

    // // Update is called once per frame
    void Update()
    {
        // friction 0.005 when parallel to velocity vector, 0.01 when rotated 90deg
        // rb.velocity -= Vector2.one * friction;
        // if (rb.velocity.x <= 0 || rb.velocity.y <= 0) rb.velocity = new Vector2(Mathf.Max(rb.velocity.x, 0), Mathf.Max(rb.velocity.y, 0));
        rb.velocity *= (1 - friction);

        if (isThrusting)
        {
            Thrust();
        }

        if (shootingEnabled && Time.time - lastShotTime >= 1 / shootingRate) {
            Shoot();

            lastShotTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        deltaDistance = Vector2.Distance(lastPos, transform.position);
        deltaVelocity = deltaDistance / Time.deltaTime;
        // Debug.Log($"deltaDistance: {deltaDistance} | deltaVelocity: {deltaVelocity}");

        lastPos = transform.position;
    }

    public Vector2 Rotate(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    public void Rotate(float angle) {
        var rotZ = angle * rotationSpeed;
        var rotVec = new Vector3(0, 0, rotZ);

        transform.Rotate(rotVec);
        // rb.velocity *= Quaternion.Euler(0, 0, rotZ);
        rb.velocity = Rotate(rb.velocity, angle * velocityRotationSpeed * Mathf.Deg2Rad);
    }

    public void ThrustStart() {
        isThrusting = true;
    }

    public void ThrustEnd() {
        isThrusting = false;
    }

    void Thrust() {
        rb.AddForce(transform.up.normalized * thrustStrength);
    }

    public void Brake() {
        // // if (rb.velocity.x > 0 || rb.velocity.y > 0) {
        //     rb.AddForce(-rb.velocity.normalized * thrustStrength);

        //     // if (rb.velocity.x < 0 || rb.velocity.y < 0) {
        //     //     rb.velocity = new Vector2(Mathf.Max(0, rb.velocity.x), Mathf.Max(0, rb.velocity.y));
        //     // }
        //     if (rb.velocity.x < 0) rb.velocity = new Vector2(0, rb.velocity.y);
        //     if (rb.velocity.y < 0) rb.velocity = new Vector2(rb.velocity.x, 0);
        // // }

        rb.AddForce(-rb.velocity * brakeStrength);
    }

    public void ShootEnable() {
        shootingEnabled = true;
    }

    public void ShootDisable() {
        shootingEnabled = false;
    }

    void Shoot() {
        var projObj = Instantiate(gameplayManager.ProjectilePrefab, shootingPoint.position, transform.rotation);
        var proj = projObj.GetComponent<Projectile>();
        proj.Setup(speed: projectileSpeed);
    }

    static public float MpsToKmph(float mps)
    {
        return mps / (1000f / 3600f);
    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     Debug.Log($"ship other: other", other);
    // }

    void OnDrawGizmos()
    {
        Handles.Label(Vector2.zero, Mathf.RoundToInt(MpsToKmph(deltaVelocity)) + " km/h");
    }
}
