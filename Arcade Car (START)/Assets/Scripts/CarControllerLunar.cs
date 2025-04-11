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

    [Header("AI Control Settings")]
    // Set this to true on the AI car instance.
    public bool isAI = false;
    // These values will be set externally if isAI is true.
    private float aiThrottle = 0f; // Expected range: 0 to 1 (0 = no throttle, 1 = full throttle)
    private float aiSteering = 0f; // Expected range: -1 (full left) to 1 (full right)

    [Header("Countdown Settings")]
    public float countdownDuration = 3f;  // Duration of countdown in seconds
    private float countdownTimer;
    public bool canMove = false;  // When false, input and forces are ignored

    private void Start()
    {
        // Detach the Rigidbody from the car GameObject (if needed)
        carRigidbody.transform.parent = null;

        // Start the countdown timer
        countdownTimer = countdownDuration;
        canMove = false;
    }

    private void Update()
    {
        // Handle countdown (same as before)
        if (!canMove)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0f)
            {
                canMove = true;  // Countdown finished – allow car movement
            }
            // During countdown, no input is processed.
            speedInput = 0f;
            return;
        }
        
        // Process input based on control mode (Player vs AI)
        if (!isAI)
        {
            // Player control using Input.GetAxis
            speedInput = 0f;
            if (Input.GetAxis("Vertical") > 0)
            {
                speedInput = Input.GetAxis("Vertical") * forwardAcceleration * 1000f;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                speedInput = Input.GetAxis("Vertical") * reverseAcceleration * 1000f;
            }
            turnInput = Input.GetAxis("Horizontal");
        }
        else
        {
            // AI control: use values provided by the AI controller
            // Here, aiThrottle is expected to be between 0 and 1.
            // Adjust the multiplication as needed for your desired behavior.
            speedInput = aiThrottle * forwardAcceleration * 1000f;
            // aiSteering is already expected to be in a [-1, 1] range.
            turnInput = aiSteering;
        }

        // Rotate the car if grounded
        if (isGrounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(0f, turnInput * turnStrength * Time.deltaTime * (isAI ? 1 : Input.GetAxis("Vertical")), 0f));
        }

        // Adjust wheel rotation for visuals
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x,
            (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x,
            turnInput * maxWheelTurn, rightFrontWheel.localRotation.eulerAngles.z);

        // Synchronize the car's position with the Rigidbody.
        transform.position = carRigidbody.transform.position;
    }

    private void FixedUpdate()
    {
        // Freeze the car during the countdown
        if (!canMove)
        {
            carRigidbody.velocity = Vector3.zero;
            return;
        }

        isGrounded = false;

        // Raycast to detect the ground or terrain.
        RaycastHit hit;
        if (Physics.Raycast(groundRayOrigin.position, -transform.up, out hit, groundRayLength, groundLayer))
        {
            isGrounded = true;
            // Align the car with the ground's surface normal.
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        Debug.DrawRay(groundRayOrigin.position, -transform.up * groundRayLength, Color.red);

        emissionRate = 0;

        if (isGrounded)
        {
            carRigidbody.drag = groundDrag;
            if (Mathf.Abs(speedInput) > 0)
            {
                carRigidbody.AddForce(transform.forward * speedInput);
                emissionRate = maxEmissionRate;
            }
        }
        else
        {
            carRigidbody.drag = 0.1f;
            carRigidbody.AddForce(Vector3.up * -gravityForce * 100f);
        }

        // Update dust trail particle effects.
        foreach (ParticleSystem trail in dustTrails)
        {
            var emissionModule = trail.emission;
            emissionModule.rateOverTime = emissionRate;
        }
    }

    // This function allows an external script (the AI controller) to set throttle and steering.
    public void SetAIInput(float throttle, float steering)
    {
        aiThrottle = Mathf.Clamp01(throttle);
        aiSteering = Mathf.Clamp(steering, -1f, 1f);
    }
}
