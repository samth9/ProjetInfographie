using UnityEngine;

public class BallDamping : MonoBehaviour
{
    Rigidbody rb;

    public float velocityThreshold = 0.01f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Si la vitesse est très faible → freiner progressivement
        if (rb.linearVelocity.magnitude < 0.2f)
        {
            rb.linearVelocity *= 0.95f; // frein linéaire
        }

        // Si la rotation est très faible → amortir aussi
        if (rb.angularVelocity.magnitude < 0.2f)
        {
            rb.angularVelocity *= 0.9f; // frein rotationnel
        }

        // Si vraiment très faible → arrêt complet
        if (rb.linearVelocity.magnitude < velocityThreshold)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (rb.angularVelocity.magnitude < velocityThreshold)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
}
