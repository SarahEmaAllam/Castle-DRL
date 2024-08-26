using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MaleAgent : Agent

{
    
    // agent variables
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    
    // env variables
    [SerializeField] private Transform envLocation;
    Material envMaterial;
    public GameObject env;
    public GameObject female;
    public CastleAgent classObject;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
        // Add rotation constraints to prevent rotation on the x and z axes
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    
        // If you want to also freeze position on the y-axis (preventing vertical movement)
        // rb.constraints |= RigidbodyConstraints.FreezePositionY;

        // Ensure collision detection is set to Continuous to avoid falling through the floor
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
    

    public override void OnEpisodeBegin()
    {
        Vector3 spawnLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));

        bool distanceGood = classObject.CheckOverlap(female.transform.localPosition, spawnLocation, 5f);

        while (!distanceGood)
        {
            spawnLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
            distanceGood = classObject.CheckOverlap(female.transform.localPosition, spawnLocation, 5f);
        }

        transform.localPosition = spawnLocation;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        // sensor.AddObservation(targetTransform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        Vector3 moveDirection = transform.forward * moveForward * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + moveDirection);

        // Rotate the agent
        float rotationAngle = moveRotate * moveSpeed * Time.deltaTime; // Ensure rotation is frame-rate independent
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationAngle * moveSpeed, 0f)); 
        
        // Vector3 velocity = new Vector3(moveX, 0, moveZ);
        // velocity = velocity.normalized  * Time.deltaTime * moveSpeed;
        // transform.position += velocity;

    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxisRaw("Horizontal");
        actions[1] = Input.GetAxisRaw("Vertical");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.tag =="Female")
        {
            
            AddReward(30f);
            classObject.AddReward(-30f);
            envMaterial.color = Color.yellow;
            // end the caught agent's episode
            classObject.EndEpisode();
            // then end hunter episode
            EndEpisode();
            
        }
        
        if (other.gameObject.tag =="Wall")
        {
            envMaterial.color = Color.red;
            AddReward(-10f);
            EndEpisode();
            
        }
        
    }
    
}
