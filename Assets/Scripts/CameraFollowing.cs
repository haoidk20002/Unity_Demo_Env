using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    public Transform main;   
    public int cameraSpeed; 
    public int positionDifference;
    private Camera mainCamera;
    private Vector3 desiredLocalLocation;
    private Vector3 desiredLocation;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        desiredLocation = transform.position;
    }
    private void Update()
    {
        desiredLocation.z = main.position.z;
    }
    private void MoveCamera()
    {
        if((main.position.z > transform.position.z + positionDifference) 
        || (main.position.z < transform.position.z - positionDifference)){
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        MoveCamera();
    }
}