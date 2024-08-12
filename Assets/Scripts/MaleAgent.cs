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
        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
        
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
            Debug.Log("Caught Female");
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
