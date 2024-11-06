using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;



public class NavigatingAgent : Agent
{
    public Transform target;
    [SerializeField] private float moveSpeed;
    private Vector3 originalLocation;
    private int steps = 0;
    private float reward = 0f;
    public Transform[] waypoints; // Array to hold waypoint references
    private int currentWaypointIndex = 0;  // Index to track the current waypoint
    private float distanceToWaypoint, prevDistanceToWaypoint;
    public float TriggeringDistance;
    private Vector3 moveVec, currentPos, targetPos, initialPos;
    private float rayDistance = 1.5f;
    private float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private Rigidbody body;
    private Vector3 initialDirectionVector, directionVector;
    public override void Initialize()
    {
        // Store the original position of the NPC
        originalLocation = transform.position;
        body = GetComponent<Rigidbody>();
    }
    public override void OnEpisodeBegin()
    {
        // Reset the NPC's position at the beginning of each episode
        moveVec = Vector3.zero;
        transform.position = originalLocation;
        initialPos = transform.position;
        targetPos = waypoints[currentWaypointIndex].position;
        initialDirectionVector = initialPos - targetPos;
        steps = 0;
        ScoreManaging.Instance.score = 0f;
        SetScrore(0f);
    }
    private void EndTheEpisode()
    {
        body.velocity = Vector3.zero;
        currentWaypointIndex = 0;
        EndEpisode();
    }
    private void SetScrore(float point)
    {
        SetReward(point);
        ScoreManaging.Instance.score = point;
        ScoreManaging.Instance.reward = 0f;
    }
    private void AddScore(float point)
    {
        AddReward(point);
        ScoreManaging.Instance.score += point;
        if (ScoreManaging.Instance.score < 0)
        {
            //.Log("Score is negative, setting it to 0");
            ScoreManaging.Instance.score = Mathf.Clamp(ScoreManaging.Instance.score, 0f, 10000000f);
        }
        ScoreManaging.Instance.reward = point;
    }
    private void DistanceReward()
    {
        if (reward >= 0)
        {
            reward = 2 * (prevDistanceToWaypoint - distanceToWaypoint);
        }
        else
        {
            reward = 5 * (prevDistanceToWaypoint - distanceToWaypoint);
        }
        AddScore(reward);
    }
    private void CheckTriggering()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            directionVector = currentPos - targetPos;
            float dotProduct = Vector3.Dot(initialDirectionVector, directionVector);
            if (dotProduct < 0)
            {
                AddScore(250f);
                Debug.Log("Reached the waypoint");
                currentWaypointIndex++;
                if (currentWaypointIndex < waypoints.Length){
                    targetPos = waypoints[currentWaypointIndex].position;
                    initialDirectionVector = currentPos - targetPos;
                } else {
                    targetPos = target.position; // Red Cube Position
                }
            }
        }
        else
        {
            if (distanceToWaypoint < TriggeringDistance)
            {
                Debug.Log("Reached the final target");
                //Debug.Break();
                AddScore(1000f);  // Large reward for reaching the final target
                Debug.Log("Final score: " + ScoreManaging.Instance.score);
                EndTheEpisode();
            }
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Raycast to detect obstacles in the forward direction
        // Iterate through each angle and perform a raycast
        foreach (float angle in rayAngles)
        {
            // Calculate the direction of the raycast based on the angle
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            // Raycast in the specified direction
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayDistance))
            {
                // Add the normalized distance to the observation
                sensor.AddObservation(hit.distance / rayDistance);
            }
            else
            {
                // No hit, add 0 for this ray
                sensor.AddObservation(0f);
            }
        }
        // Add the main character's position as an observation
        sensor.AddObservation(transform.position);

        // Add the target's position as an observation
        sensor.AddObservation(target.position);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        steps++;
        AddScore(-1f);  // Small penalty for each step
        int movement = actionBuffers.DiscreteActions[0];
        // Calculate the movement vector based on the action
        moveVec = movement switch
        {
            1 => new Vector3(-1, 0, 0),
            2 => new Vector3(1, 0, 0),
            3 => new Vector3(0, 0, -1),
            4 => new Vector3(0, 0, 1),
            _ => Vector3.zero
        };
        // Apply the force to the agent, the movement takes effects in the next step
        body.velocity = Vector3.zero;
        body.AddForce(moveVec * moveSpeed, ForceMode.VelocityChange);
        // Calculate distance to the current waypoint
        currentPos = transform.position;
        distanceToWaypoint = Vector3.Distance(currentPos, targetPos);
        //
        // The distanceToWaypoint is calculated before the previousDistanceToWaypoint is updated, so at first step the score is improperly calculated
        // we don't want to calculate the reward at the first step
        if (StepCount != 1)
        {
            DistanceReward();
        }
        CheckTriggering();
        prevDistanceToWaypoint = distanceToWaypoint;
        if (steps >= MaxStep)
        {
            EndTheEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Implement keyboard input for testing the agent in the editor
        if (Input.GetKey(KeyCode.LeftArrow)) { discreteActionsOut[0] = 1; }
        else if (Input.GetKey(KeyCode.RightArrow)) { discreteActionsOut[0] = 2; }
        else if (Input.GetKey(KeyCode.DownArrow)) { discreteActionsOut[0] = 3; }
        else if (Input.GetKey(KeyCode.UpArrow)) { discreteActionsOut[0] = 4; }
        else { discreteActionsOut[0] = 0; }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // Draw raycasts in each direction
        foreach (float angle in rayAngles)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, direction * rayDistance);
        }
    }
}
