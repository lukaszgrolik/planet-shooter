using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;

    public void Setup(float speed)
    {
        rb = GetComponent<Rigidbody2D>();

        rb.velocity = transform.up.normalized * speed;
    }
}
