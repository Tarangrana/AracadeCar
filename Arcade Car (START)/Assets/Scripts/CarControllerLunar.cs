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

    [Header("Countdown Settings")]
    public float countdownDuration = 3f;  // Duration of countdown in seconds
    private float countdownTimer;
    public bool canMove = false;  // When false, input and forces are ignored

    private void Start()
    {
        // Detach the Rigidbody from the car GameObject (if needed)
        carRigidbody.transform.parent = null;

        // Initialize countdown timer and disable movement initially
        countdownTimer = countdownDuration;
        canMove = false;
    }

    private void Update()
    {
        // Disable input during countdown.
        if (!canMove)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0f)
            {
                canMove = true;  // Countdown finished – allow car movement
            }
            // Prevent any input while counting down.
            speedInput = 0f;
            return;
        }
        
        // Handle movement input once allowed
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

        // Rotate the car if grounded.
        if (isGrounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Input.GetAxis("Vertical"), 0f));
        }

        // Adjust the wheel rotation for visuals.
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x,
            (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x,
            turnInput * maxWheelTurn, rightFrontWheel.localRotation.eulerAngles.z);

        // Synchronize the car's position with the Rigidbody.
        transform.position = carRigidbody.transform.position;
    }

    private void FixedUpdate()
    {
        // While the car is not allowed to move (during countdown), freeze its Rigidbody.
        if (!canMove)
        {
            carRigidbody.velocity = Vector3.zero;
            return;
        }

        isGrounded = false;

        // Raycast to detect ground.
        RaycastHit hit;
        if (Physics.Raycast(groundRayOrigin.position, -transform.up, out hit, groundRayLength, groundLayer))
        {
            isGrounded = true;
            // Align the car with the ground's surface normal.
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        // Debug visualization.
        Debug.DrawRay(groundRayOrigin.position, -transform.up * groundRayLength, Color.red);

        emissionRate = 0;

        if (isGrounded)
        {
            carRigidbody.drag = groundDrag;

            // Apply movement force if there is input.
            if (Mathf.Abs(speedInput) > 0)
            {
                carRigidbody.AddForce(transform.forward * speedInput);
                emissionRate = maxEmissionRate;
            }
        }
        else
        {
            // Use reduced drag and apply lunar gravity when in the air.
            carRigidbody.drag = 0.1f;
            carRigidbody.AddForce(Vector3.up * -gravityForce * 100f);
        }

        // Update particle trail emission rate.
        foreach (ParticleSystem trail in dustTrails)
        {
            var emissionModule = trail.emission;
            emissionModule.rateOverTime = emissionRate;
        }
    }
}
