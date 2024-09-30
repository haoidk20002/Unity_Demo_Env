using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointColor : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1f); // Visualize the waypoint
    }
}
