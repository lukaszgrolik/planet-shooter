using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Ship ship;

    public void Setup(Ship ship)
    {
        this.ship = ship;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A)) {
            ship.Rotate(Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D)) {
            ship.Rotate(-Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W)) {
            ship.Thrust();
        }

        if (Input.GetKey(KeyCode.S)) {
            ship.Brake();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            ship.ShootEnable();
        }
        else if (Input.GetKeyUp(KeyCode.Space)) {
            ship.ShootDisable();
        }
    }
}
