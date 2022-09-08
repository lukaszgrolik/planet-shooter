using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpointLine : MonoBehaviour
{
    public event System.Action<TrackCheckpointLine> collided;

    void OnTriggerEnter2D(Collider2D other)
    {
        // var trackCheckpointLine = other.GetComponentInParent<TrackCheckpointLine>();

        collided?.Invoke(this);
    }
}