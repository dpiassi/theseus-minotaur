using UnityEngine;

/// <summary>
/// Rotates Transform every frame using a constant
/// angular velocity which is randomized at startup.
/// </summary>
public class RotateForever : MonoBehaviour
{
    // Auxiliar member:
    Vector3 angularVelocity;

    void Start()
    {
        angularVelocity = Random.rotation.eulerAngles;
    }

    void Update()
    {
        transform.Rotate(angularVelocity * Time.deltaTime);
    }
}
