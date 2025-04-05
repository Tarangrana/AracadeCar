using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerLunar : MonoBehaviour
{
    [Header("Physics Settings")]
    public Rigidbody carRigidbody;
    public float forwardAcceleration = 8f;
    public float reverseAcceleration = 4f;
    public float maxSpeed = 50f;
    public float turnStrength = 180f;
    public float gravityForce = 1.62f; // Lunar gravity
    public float groundDrag = 3f;

    [Header("Input and Movement")]
    public float speedInput;
    private float turnInput;
    private bool isGrounded;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundRayLength = 0.5f;
    public Transform groundRayOrigin;

    [Header("Wheel Settings")]
    public Transform leftFrontWheel;
    public Transform rightFrontWheel;
    public float maxWheelTurn = 25f;

    [Header("Effects")]
    public ParticleSystem[] dustTrails;
    public float maxEmissionRate = 25f;
    private float emissionRate;

    private void Start()
    {
        // Detach the Rigidbody from the car GameObject
        carRigidbody.transform.parent = null;
    }

    private void Update()
    {
        // Handle movement input
        speedInput = 0f;
        if (Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * forwardAcceleration * 1000f;
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAcceleration * 1000f;
        }

        // Handle turning input
        turnInput = Input.GetAxis("Horizontal");

        // Rotate the car if grounded
        if (isGrounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Input.GetAxis("Vertical"), 0f));
        }

        // Adjust wheel rotation for visuals
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x,
            (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x,
            turnInput * maxWheelTurn, rightFrontWheel.localRotation.eulerAngles.z);

        // Synchronize car position with Rigidbody
        transform.position = carRigidbody.transform.position;
    }

    private void FixedUpdate()
    {
        isGrounded = false;

        // Raycasting to detect the ground or terrain
        RaycastHit hit;
        if (Physics.Raycast(groundRayOrigin.position, -transform.up, out hit, groundRayLength, groundLayer))
        {
            isGrounded = true;

            // Align the car with the ground's surface normal
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        // Visualize the raycast for debugging
        Debug.DrawRay(groundRayOrigin.position, -transform.up * groundRayLength, Color.red);

        emissionRate = 0;

        if (isGrounded)
        {
            carRigidbody.drag = groundDrag;

            // Apply movement force
            if (Mathf.Abs(speedInput) > 0)
            {
                carRigidbody.AddForce(transform.forward * speedInput);
                emissionRate = maxEmissionRate;
            }
        }
        else
        {
            // Simulate air drag and custom gravity for the Moon
            carRigidbody.drag = 0.1f;
            carRigidbody.AddForce(Vector3.up * -gravityForce * 100f);
        }

        // Update particle effects
        foreach (ParticleSystem trail in dustTrails)
        {
            var emissionModule = trail.emission;
            emissionModule.rateOverTime = emissionRate;
        }
    }
}
