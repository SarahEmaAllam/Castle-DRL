using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MaleAgentV2 : Agent

{
    
    // target variables for builing
    [SerializeField] private Transform buildTransform;
    // [SerializeField] private List<GameObject> spawnedBuildList = new List<GameObject>();
    // public int buildCount;
    // public GameObject bricks;
    // private float RAY_RANGE = 5f;
    
    // agent variables
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    // [SerializeField]
    // private LayerMask pickableLayerMask;
    // [SerializeField]
    // private Transform playerCameraTransform;
    
    [SerializeField]
    [Min(1)]
    private float hitRange = 3;
    // if raycast hits something
    private RaycastHit hit;
    // private RayPerceptionSensorComponent3D rayPerception;
    // Reference to the picked-up object
    private GameObject pickedUpObject;
    private bool canPickUp = false;
    
    // env variables
    private CastleArea agentArea;
    [SerializeField] private Transform envLocation;
    Material envMaterial;
    public GameObject env;
    public GameObject female;
    public CastleAgent classObject;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        agentArea = transform.parent.GetComponent<CastleArea>();
        envMaterial = env.GetComponent<Renderer>().material;
        canPickUp = false;
        pickedUpObject = null;
        // Add rotation constraints to prevent rotation on the x and z axes
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    
        // If you want to also freeze position on the y-axis (preventing vertical movement)
        // rb.constraints |= RigidbodyConstraints.FreezePositionY;

        // Ensure collision detection is set to Continuous to avoid falling through the floor
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        // rayPerception = GetComponent<RayPerceptionSensorComponent3D>();
    }
    

    // public override void OnEpisodeBegin()
    // {
    //     canPickUp = false;
    //     pickedUpObject = null;
    //     Vector3 spawnLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
    //
    //     bool distanceGood = classObject.CheckOverlap(female.transform.localPosition, spawnLocation, 3f);
    //
    //     while (!distanceGood)
    //     {
    //         spawnLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
    //         distanceGood = classObject.CheckOverlap(female.transform.localPosition, spawnLocation, 5f);
    //     }
    //
    //     transform.localPosition = spawnLocation;
    //
    //     CreateBuild();
    // }
    
    // private void CreateBuild()
    // {
    //     bool distanceGood;
    //     GameObject newBuild = Instantiate(build);
    //     // make target child of env 
    //     newBuild.transform.parent = envLocation;
    //     // give random spawn location
    //     // float x = r.Next(0, 5);
    //     // float z = r.Next(0, 5);
    //     Vector3 buildLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
    //
    //     
    //     distanceGood = CheckOverlap(buildLocation, spawnedTargetList[k].transform.localPosition, RAY_RANGE);
    //     if (distanceGood == false)
    //     {
    //         targetLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
    //         k--;
    //         newBuild.transform.localPosition = buildLocation;
    //
    //     } // spawn in new location
    //     
    //     spawnedBuildList.Add(newBuild);
    //     
    // }
    
    private void removeTarget(List<GameObject> targetsToDelete)
    {
        
        foreach (var target in targetsToDelete)
        {
            Destroy(target.gameObject);
        }
        targetsToDelete.Clear();
    }

    public bool CheckOverlap(Vector3 objAvoidOverlap, Vector3 existingObj, float minDistance)
    {
        float distanceBetweenObj = Vector3.Distance(objAvoidOverlap, existingObj);

        if (minDistance <= distanceBetweenObj)
        {
            return true;
        }

        return false;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add raycast perception observations for stumps and walls
        float rayDistance = 20f;
        float[] rayAngles = { 90f };
        string[] detectableObjects = { "Floor", "Wall", "Female", "Male" };
        // Define the RayPerceptionInput
        RayPerceptionInput rayInput = new RayPerceptionInput
        {
            RayLength = rayDistance,
            Angles = rayAngles,
            DetectableTags = detectableObjects,
            StartOffset = 0f,
            EndOffset = 0f,
            // You may need to set additional fields depending on your version of ML-Agents
            Transform = this.transform, // Example: setting the transform of the agent
            CastRadius = 0f, // Example: setting a cast radius if needed
        };

        // Use the RayPerceptionInput struct with Perceive
        // Call the static Perceive method using the class name
        RayPerceptionOutput rayOutput = RayPerceptionSensor.Perceive(rayInput);

        // Add the observation
        // sensor.AddObservation(rayOutput);
        // Add the relevant information from RayPerceptionOutput as observations
        foreach (var rayOutputResult in rayOutput.RayOutputs)
        {
            // Add the distance to the detected object as an observation
            sensor.AddObservation(rayOutputResult.HitFraction);

            // Optionally, add whether a hit was detected (true/false)
            sensor.AddObservation(rayOutputResult.HasHit ? 1 : 0);
    
            // Optionally, add the tag of the detected object as an integer (if tags are mapped to integers)
            sensor.AddObservation(rayOutputResult.HitTagIndex);
            
        }

        sensor.AddObservation(transform.position);
        // sensor.AddObservation(targetTransform.position);
        

    }
    
    // Mask or unmask the pick-up action based on collision status
    // Mask or unmask the pick-up action based on collision status
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!canPickUp || pickedUpObject != null)
        {
            // Mask the pick-up action if the agent can't pick up or already holds something
            actionMask.SetActionEnabled(0, 0, false);
        }
        else
        {
            // Unmask the pick-up action if the agent can pick up
            actionMask.SetActionEnabled(0, 0, true);
        }

        if (pickedUpObject == null)
        {
            // Mask the drop action if the agent is not holding anything
            actionMask.SetActionEnabled(0, 1, false);
        }
        else
        {
            // Unmask the drop action if the agent is holding something
            actionMask.SetActionEnabled(0, 1, true);
        }
    }
    
    
    // Handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is in the "Pickable" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pickable"))
        {
            canPickUp = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // If the agent exits collision with a pickable object
        if (collision.gameObject == pickedUpObject)
        {
            canPickUp = false;
            pickedUpObject = null;
        }
    }

// Function to pick up the object
private void PickUpObject()
{
    if (pickedUpObject == null && canPickUp)
    {
        // Assuming the agent is colliding with a pickable object
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Pickable"));
        if (colliders.Length > 0)
        {
            pickedUpObject = colliders[0].gameObject;

            // Disable the object's physics temporarily
            Rigidbody rbPickable = pickedUpObject.GetComponent<Rigidbody>();
            if (rbPickable != null)
            {
                rbPickable.isKinematic = true;
                rbPickable.useGravity = false;
            }

            // Attach the object to the agent but ensure no interference with the agent's collider
            pickedUpObject.transform.SetParent(this.transform);
            pickedUpObject.transform.localPosition = new Vector3(0, 1, 1); // Adjust as needed
            pickedUpObject.transform.localRotation = Quaternion.identity;

            // Ensure the object's collider doesn't interfere with the agent's collider
            Collider objectCollider = pickedUpObject.GetComponent<Collider>();
            Collider agentCollider = GetComponent<Collider>();
            if (objectCollider != null && agentCollider != null)
            {
                Physics.IgnoreCollision(agentCollider, objectCollider, false); // Enable collision detection
            }

            canPickUp = false; // The agent can no longer pick up until it drops the current object
        }
    }
}

// Function to drop the object
private void DropObject()
{
    if (pickedUpObject != null)
    {
        // Detach the object from the agent
        pickedUpObject.transform.SetParent(null);

        // Re-enable the object's physics
        Rigidbody rbPickable = pickedUpObject.GetComponent<Rigidbody>();
        if (rbPickable != null)
        {
            rbPickable.isKinematic = false;
            rbPickable.useGravity = true;
        }

        // Ensure the object's collider can now collide with the agent again
        Collider objectCollider = pickedUpObject.GetComponent<Collider>();
        Collider agentCollider = GetComponent<Collider>();
        if (objectCollider != null && agentCollider != null)
        {
            Physics.IgnoreCollision(agentCollider, objectCollider, false); // Re-enable collision detection between agent and object
        }

        // Clear the reference to the picked-up object
        pickedUpObject = null;
    }
}


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        // Move the agent forward/backward
        if (rb != null)
        {
            Vector3 moveDirection = transform.forward * moveForward * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + moveDirection);
        }

        // Rotate the agent
        if (rb != null)
        {
            float rotationAngle = moveRotate * moveSpeed * Time.deltaTime; // Ensure rotation is frame-rate independent
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationAngle * moveSpeed, 0f)); // Usi
        }
        // Vector3 velocity = new Vector3(moveX, 0, moveZ);
        // velocity = velocity.normalized  * Time.deltaTime * moveSpeed;
        // transform.position += velocity;
        
        // Get the discrete actions for pick up and drop
        int discreteAction = actions.DiscreteActions[0];
        bool pickUpAction;
        bool dropAction;
        bool noBuildAction;
        if (discreteAction == 0)
        {
            pickUpAction = true;
        } else { pickUpAction = false;}
        if (discreteAction == 1)
        {
            dropAction = true;
        } else { dropAction = false;}
        if (discreteAction == 2)
        {
            noBuildAction = true;
        } else { noBuildAction = false;}

        // If the pick-up action is selected and the agent can pick up, execute the pick-up
        if (pickUpAction == true && canPickUp)
        {
            PickUpObject();
        }

        // If the drop action is selected and the agent is holding something, drop it
        if (dropAction == true && pickedUpObject != null)
        {
            DropObject();
        }

    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actionsMovement = actionsOut.ContinuousActions;
        actionsMovement[0] = Input.GetAxisRaw("Horizontal");
        actionsMovement[1] = Input.GetAxisRaw("Vertical");
        
        ActionSegment<int> actionsBuild = actionsOut.DiscreteActions;
        // pick up item
        if (Input.GetKey("e"))
        {
            actionsBuild[0] = 0;
        }
        // put down item
        else if (Input.GetKey("d"))
        {
            actionsBuild[0] = 1;
        }
        else
        {
            actionsBuild[0] = 2;
        }
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        
        // Determine state
        if (GetCumulativeReward() <= -500f)
        {
            // Reward is too negative, give up
            EndEpisode();

            // Indicate failure with the ground material
            StartCoroutine(agentArea.SwapGroundMaterial(success: false));

            // Reset
            agentArea.ResetArea();
        }
        // else if (buildCount >= agentArea.GetBricksObjects().Count)
        // {
        //     // All truffles collected, success!
        //     EndEpisode();
        //
        //     // Indicate success with the ground material
        //     StartCoroutine(agentArea.SwapGroundMaterial(success: true));
        //
        //     // Reset
        //     agentArea.ResetArea();
        // }
        // else
        // {
        //     // Encourage movement with a tiny time penalty and pdate the score text display
        //     AddReward(-.001f);
        //     agentArea.UpdateScore(GetCumulativeReward());
        // }
        
        if (other.gameObject.tag =="Female")
        {
            
            AddReward(30f);
            classObject.AddReward(-30f);
            Debug.Log("Caught Female");
            envMaterial.color = Color.yellow;
            agentArea.UpdateScore(GetCumulativeReward());
            // end the caught agent's episode
            classObject.EndEpisode();
            // then end hunter episode
            EndEpisode();
            
        }
        
        if (other.gameObject.tag =="Wall")
        {
            Debug.Log("hit wall");
            envMaterial.color = Color.red;
            AddReward(-10f);
            agentArea.UpdateScore(GetCumulativeReward());
            Debug.Log("hit wall");
            // agentArea.ResetAgent(this);
            EndEpisode();
            
        }
        
    }
    
    
}
