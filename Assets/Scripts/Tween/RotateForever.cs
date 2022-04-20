using UnityEngine;

public class RotateForever : MonoBehaviour
{
    Vector3 angularVelocity;

    // Start is called before the first frame update
    void Start()
    {
        angularVelocity = Random.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(angularVelocity * Time.deltaTime);
    }
}
