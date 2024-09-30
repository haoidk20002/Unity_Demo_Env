using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class NPCNavigationAgent : Agent
{
    public Transform target;
    [SerializeField] private float moveSpeed;
    private Vector3 originalLocation;
    private int steps = 0;
    private float reward = 0f;
    public Transform[] waypoints; // Array to hold waypoint references
    private int currentWaypointIndex = 0;  // Index to track the current waypoint
    private float distanceToWaypoint,prevDistanceToWaypoint;

    public override void Initialize()
    {
        // Store the original position of the NPC
        originalLocation = transform.localPosition;
    }
    private void EndTheEpisode()
    {
        currentWaypointIndex = 0;
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        // Reset the NPC's position at the beginning of each episode
        transform.position = originalLocation;
        steps = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the NPC's position as an observation
        sensor.AddObservation(transform.localPosition);

        // Add the target's position as an observation
        sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int movement = actionBuffers.DiscreteActions[0];

        float directionX = 0, directionZ = 0;

        // Look up the index in the movement action list:
        if (movement == 0) { }
        if (movement == 1) { directionX = -1; }
        if (movement == 2) { directionX = 1; }
        if (movement == 3) { directionZ = -1; }
        if (movement == 4) { directionZ = 1; }
        Vector3 moveVec = new Vector3(directionX, 0.0f, directionZ);

        Vector3 prevPos = transform.position;
        // Apply the movement to the agent
        transform.position += moveVec * moveSpeed * Time.deltaTime;
        // Calculate distance to the current waypoint
        distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
        prevDistanceToWaypoint = Vector3.Distance(prevPos, waypoints[currentWaypointIndex].position);
        // Reward for getting closer to the current waypoint
        reward = prevDistanceToWaypoint - distanceToWaypoint;
        AddReward(reward);
        // Small penalty for each step
        // AddReward(-0.001f);
        // If the agent reaches the current waypoint
        if (distanceToWaypoint < 1.0f)
        {
            AddReward(500f);  // Small reward for reaching the waypoint
            Debug.Log("+" + 10f);
            currentWaypointIndex++;

            // If all waypoints are reached, end the episode
            if (currentWaypointIndex == waypoints.Length)
            {
                AddReward(1000f);  // Large reward for reaching the final target
                Debug.Log("+" + 1000f);
                EndTheEpisode();
            }
        }
        steps++;
        if (steps >= MaxStep)
        {
            EndTheEpisode();
            Debug.Log("Failed");
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
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-1f);  // Negative reward for hitting obstacles
        }
    }
}
