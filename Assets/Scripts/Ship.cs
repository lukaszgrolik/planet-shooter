using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    private Rigidbody2D rb;

    private IGameplayManagerPrefabs gameplayManager;

    [SerializeField] private Transform shootingPoint;

    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float thrustStrength = 1f;
    [SerializeField] private float shootingRate = 5f;

    [SerializeField] private float projectileSpeed = 10f;

    private bool shootingEnabled = false;
    private float lastShotTime = -1;

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
        if (shootingEnabled && Time.time - lastShotTime >= 1 / shootingRate) {
            Shoot();

            lastShotTime = Time.time;
        }
    }

    public void Rotate(float angle) {
        transform.Rotate(new Vector3(0, 0, angle * rotationSpeed));
    }

    public void Thrust() {
        rb.AddForce(transform.up.normalized * thrustStrength);
    }

    public void Brake() {
        // if (rb.velocity.x > 0 || rb.velocity.y > 0) {
            rb.AddForce(-rb.velocity.normalized * thrustStrength);

            // if (rb.velocity.x < 0 || rb.velocity.y < 0) {
            //     rb.velocity = new Vector2(Mathf.Max(0, rb.velocity.x), Mathf.Max(0, rb.velocity.y));
            // }
            if (rb.velocity.x < 0) rb.velocity = new Vector2(0, rb.velocity.y);
            if (rb.velocity.y < 0) rb.velocity = new Vector2(rb.velocity.x, 0);
        // }
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

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     Debug.Log($"ship other: other", other);
    // }
}
