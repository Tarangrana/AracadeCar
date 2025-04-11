using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Waypoint Route")]
    // Assign the WaypointRoute component from your scene.
    public WaypointRoute route;
    
    [Header("AI Parameters")]
    public float reachThreshold = 5f;       // Distance at which the car will switch to the next waypoint
    public float maxSteerInput = 1f;          // Maximum steering input (expected range: -1 to 1)
    public float constantThrottle = 1f;       // Constant throttle value (0 to 1)

    private int currentWaypointIndex = 0;
    private CarControllerLunar carController; // Reference to the car's controller

    // Optional: For smoothing the steering (if needed)
    public float steeringSmoothing = 5f;
    private float currentSteerInput = 0f;

    private void Start()
    {
        carController = GetComponent<CarControllerLunar>();
        
        // Error checking for route settings.
        if (route == null || route.waypoints.Length == 0)
        {
            Debug.LogError("AICarController: No waypoint route set or the route is empty!");
        }
    }

    private void Update()
    {
        if (route == null || route.waypoints.Length == 0)
            return;
        
        // Get the current target waypoint.
        Transform targetWaypoint = route.waypoints[currentWaypointIndex];
        
        // Determine the direction vector from the car to the waypoint.
        Vector3 targetDirection = targetWaypoint.position - transform.position;
        
        // Calculate the signed angle between the car's forward vector and the target direction.
        float angleToTarget = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
        
        // Normalize the steering input by dividing by an angle that you consider "maximum."
        // For example, if 45° means full steering then divide the angle by 45.
        float desiredSteerInput = Mathf.Clamp(angleToTarget / 45f, -maxSteerInput, maxSteerInput);
        
        // Optionally smooth the steering input.
        currentSteerInput = Mathf.Lerp(currentSteerInput, desiredSteerInput, Time.deltaTime * steeringSmoothing);

        // Check if the car is close enough to the waypoint to switch to the next one.
        if (targetDirection.magnitude < reachThreshold)
        {
            // Advance to the next waypoint in the route, wrapping around if needed.
            currentWaypointIndex = (currentWaypointIndex + 1) % route.waypoints.Length;
        }
        
        // Send the computed throttle and steering to the car controller.
        carController.SetAIInput(constantThrottle, currentSteerInput);
    }
}
