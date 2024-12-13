using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;


public class NavigatingAgent : Agent
{
    public Transform target;
    [SerializeField] private float moveSpeed;
    private Vector3 originalLocation;
    private int steps = 0;
    private float reward = 0f;
    private Vector3 moveVec, direction, targetPos, newPosition;
    private Rigidbody body;
    private bool hitWall = false, hitTarget = false;
    private List<Vector3> oldPositions = new List<Vector3>();
    public float rayDistance;
    private float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    public override void Initialize()
    {
        // Store the original position of the NPC
        originalLocation = transform.position;
        body = GetComponent<Rigidbody>();
    }
    public override void OnEpisodeBegin()
    {
        oldPositions.Clear();
        // Reset the main's position at the beginning of each episode
        moveVec = Vector3.zero;
        transform.position = originalLocation;
        //initialPos = transform.position;
        steps = 0;
        ScoreManaging.Instance.score = 0f;
        GiveReward(0f);
    }
    private void GiveReward(float point)
    {
        SetReward(point);
        // ScoreManaging.Instance.score = point;
        // if (ScoreManaging.Instance.score < 0)
        // {
        //     //.Log("Score is negative, setting it to 0");
        //     ScoreManaging.Instance.score = Mathf.Clamp(ScoreManaging.Instance.score, 0f, 10000000f);
        // }
        ScoreManaging.Instance.reward = point;
    }
    // private void AddScore(float point)
    // {
    //     //AddReward(point);
    //     SetReward(point);
    //     ScoreManaging.Instance.score += point;
    //     if (ScoreManaging.Instance.score < 0)
    //     {
    //         //.Log("Score is negative, setting it to 0");
    //         ScoreManaging.Instance.score = Mathf.Clamp(ScoreManaging.Instance.score, 0f, 10000000f);
    //     }
    //     ScoreManaging.Instance.reward = point;
    // }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the main character's position as an observation
        Vector3 position = new Vector3(Mathf.Round(transform.position.x * 10) / 10, Mathf.Round(transform.position.y * 10) / 10, Mathf.Round(transform.position.z * 10) / 10);
        sensor.AddObservation(position);

        // // Add the target's position as an observation
        // sensor.AddObservation(target.position);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int movement = actionBuffers.DiscreteActions[0];
        // Calculate the movement vector based on the action
        moveVec = movement switch
        {
            0 => new Vector3(-1, 0, 0),
            1 => new Vector3(1, 0, 0),
            2 => new Vector3(0, 0, -1),
            3 => new Vector3(0, 0, 1),
            _ => Vector3.zero
        };
        // Store current pos as old pos if it is not in the list
        Vector3 position = new Vector3(Mathf.Round(transform.position.x * 10) / 10, Mathf.Round(transform.position.y * 10) / 10, Mathf.Round(transform.position.z * 10) / 10);

        if (!oldPositions.Contains(position))
        {
            oldPositions.Add(position);
        }
        // if collide to the target, get 10. Not , get -0,05.
        // if hit wall, -0.75. Return to old position, get -0.25. 
        // Priority: hit wall > hit exit > return > not exit
        // Raycast in the specified direction
        if (Physics.Raycast(transform.position, moveVec, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                hitWall = true;
                Debug.Log("Hit wall");
                GiveReward(-0.75f);
            }
            else if (hit.collider.CompareTag("Target"))
            {
                transform.position += moveVec;
                GiveReward(1000f);
                EndEpisode();
            }
        }
        // one raycast only for chosen direction
        // If raycast doesn't detect wall, move the main character and store new pos to check if it is in the list
        if (hitWall == false)
        {
            transform.position += moveVec;
            // get rounded position with 1 decimal point
            newPosition = new Vector3(Mathf.Round(transform.position.x * 10) / 10, Mathf.Round(transform.position.y * 10) / 10, Mathf.Round(transform.position.z * 10) / 10);
            // Add a negative reward if the main character returns to a previous position
            if (oldPositions.Contains(newPosition))
            {
                GiveReward(-0.25f);
                Debug.Log("Return to old position");
            }
            else
            {
                GiveReward(-0.05f);
                Debug.Log("Not exit");
            }
        }
        hitWall = false;
        
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
