using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float ScrollSpeed = 15f;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    // Update is called once per frame
    void Update()
    {
        if (Input.mousePosition.y >= Screen.height && transform.position.y < maxBounds.y)
        {
            transform.Translate(Vector3.up * Time.deltaTime * ScrollSpeed, Space.World);
        }
        if (Input.mousePosition.y <= 0 && transform.position.y > minBounds.y)
        {
            transform.Translate(Vector3.down * Time.deltaTime * ScrollSpeed, Space.World);
        }
        if (Input.mousePosition.x >= Screen.width && transform.position.x < maxBounds.x)
        {
            transform.Translate(Vector3.right * Time.deltaTime * ScrollSpeed, Space.World);
        }
        if (Input.mousePosition.x <= 0 && transform.position.x > minBounds.x)
        {
            transform.Translate(Vector3.left *Time.deltaTime * ScrollSpeed, Space.World);
        }
    }
}
