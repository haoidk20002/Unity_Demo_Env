using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class NPCNavigationAgent : Agent
{
    public Transform target;
    [SerializeField] private float moveSpeed;
    private Vector3 originalLocation;
    private int steps = 0;
    private float reward = 0f;
    public Transform[] waypoints; // Array to hold waypoint references
    private int currentWaypointIndex = 0;  // Index to track the current waypoint
    private float distanceToWaypoint, prevDistanceToWaypoint;
    private Vector3 moveVec;
    private float rayDistance = 1.5f;
    private float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private Rigidbody body;
    public override void Initialize()
    {
        // Store the original position of the NPC
        originalLocation = transform.localPosition;
        body = GetComponent<Rigidbody>();
    }
    private void EndTheEpisode()
    {
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
        ScoreManaging.Instance.reward = point;
    }

    public override void OnEpisodeBegin()
    {
        // Reset the NPC's position at the beginning of each episode
        moveVec = Vector3.zero;
        transform.localPosition = originalLocation;
        steps = 0;
        ScoreManaging.Instance.score = 0f;
        SetScrore(0f);
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
        int movement = actionBuffers.DiscreteActions[0];

        float directionX = 0, directionZ = 0, directionY = 0;

        // Look up the index in the movement action list:
        if (movement == 0) { }
        if (movement == 1) { directionX = -1; }
        if (movement == 2) { directionX = 1; }
        if (movement == 3) { directionZ = -1; }
        if (movement == 4) { directionZ = 1; }

        moveVec = new Vector3(directionX, directionY, directionZ);
        // Store the current position of the agent
        Vector3 prevPos = transform.position;
        prevDistanceToWaypoint = Vector3.Distance(prevPos, waypoints[currentWaypointIndex].position);
        // Debug.Log("Prev Distance to waypoint: " + prevDistanceToWaypoint);
        // Apply the movement to the agent
        //transform.Translate(moveVec * moveSpeed * Time.deltaTime);
        body.AddForce(moveVec * moveSpeed, ForceMode.VelocityChange);
        // Calculate distance to the current waypoint
        distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
        Debug.Log("Move distance: " + Vector3.Distance(prevPos, transform.position));
        // Reward for getting closer to the current waypoint
        if (reward >= 0){
            reward = 2*(prevDistanceToWaypoint - distanceToWaypoint);
        }else{
            reward = 4*(prevDistanceToWaypoint - distanceToWaypoint);
        }
        AddScore(reward);
        // Small penalty for each step
        AddScore(-0.0001f);
        // If the agent reaches the current waypoint
        if (distanceToWaypoint < 0.5f)
        {
            AddScore(200f);  // Small reward for reaching the waypoint
            //Debug.Log("+" + 200f);
            currentWaypointIndex++;

            // If all waypoints are reached, end the episode
            if (currentWaypointIndex == waypoints.Length)
            {
                AddScore(1000f);  // Large reward for reaching the final target
                //Debug.Log("+" + 1000f);
                
                EndTheEpisode();
            }
        }
        steps++;
        if (steps >= MaxStep)
        {
            EndTheEpisode();
            //Debug.Log("Failed");
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            //Debug.Log("Hit Obstacle");
            AddScore(-10f); // Negative reward for hitting obstacles
        }
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
