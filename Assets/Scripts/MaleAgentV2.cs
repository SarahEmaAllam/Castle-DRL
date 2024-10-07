using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MaleAgentV2 : Agent

{
    
    private RayPerceptionSensorComponent3D rayPerceptionSensor;
    // target variables for builing
    [SerializeField] private Transform buildTransform;
    // [SerializeField] private List<GameObject> spawnedBuildList = new List<GameObject>();
    
    private const float brickCostPerObject = 1000f;
    private string[] propNames = { "floor", "floorStairs", "column_mini" }; // List of props
    
    // private float RAY_RANGE = 5f;
    
    // agent variables
    [Header("TEAM")]

    public int teamID = 1;
    [SerializeField] private float moveSpeed = 1.0f;
    private float rotateSpeed = 100f;
    private Rigidbody rb;
    public BoxCollider boxCollider;
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
    private GameObject objectToDestroy = null;
    private bool canDestroy = false;
    private bool canPickUp = false;
    private bool canBuild = true;
    // Class-level variable to store the original rotation of the picked-up object
    private Quaternion startingRot;
    private Vector3 startingPos;

    
    // env variables
    private CastleArea castleArea;
    [SerializeField] private Transform envLocation;
    Material envMaterial;
    public GameObject env;



    public override void Initialize()
    {
        // // Initialize the Multi-Agent Group
        // m_AgentGroup = new SimpleMultiAgentGroup();
        // foreach (var agent in AgentList)
        // {
        //     m_AgentGroup.RegisterAgent(agent);
        //     castleArea.RandomlyPlaceObject(agent.gameObject, 10f, 3);
        // }
        rayPerceptionSensor = GetComponent<RayPerceptionSensorComponent3D>();

        if (rayPerceptionSensor == null)
        {
            Debug.LogError("RayPerceptionSensorComponent3D not found!");
        }
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        castleArea = transform.parent.GetComponent<CastleArea>();
        envMaterial = env.GetComponent<Renderer>().material;
        canPickUp = false;
        canBuild = true;
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
        
        // Register all agents in the group at the start of the episode
        // foreach (var agent in AgentList)
        // {
        //     m_AgentGroup.RegisterAgent(agent);
        //     castleArea.RandomlyPlaceObject(agent.gameObject, 10f, 3);
        // }
        
        // Vector3 spawnLocation = new Vector3(UnityEngine.Random.Range(-15f, 15f), 0, UnityEngine.Random.Range(-7f, 7f));
        //
        // bool distanceGood = classObject.CheckOverlap(female.transform.localPosition, spawnLocation, 5f);
        //
        // while (!distanceGood)
        // {
        //     spawnLocation = new Vector3(UnityEngine.Random.Range(-15f, 15f), 0, UnityEngine.Random.Range(-7f, 7f));
        //     distanceGood = classObject.CheckOverlap(female.transform.localPosition, spawnLocation, 5f);
        // }
        //
        // transform.localPosition = spawnLocation;
    // }

    

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
    
    private void checkRayCast()
    {
        // RayPerceptionSensorComponent3D m_rayPerceptionSensorComponent3D = GetComponent<RayPerceptionSensorComponent3D>();

        var rayOutputs = RayPerceptionSensor.Perceive(rayPerceptionSensor.GetRayPerceptionInput()).RayOutputs;
        int lengthOfRayOutputs = rayOutputs.Length;

        // Alternating Ray Order: it gives an order of
        // (0, -delta, delta, -2delta, 2delta, ..., -ndelta, ndelta)
        // index 0 indicates the center of raycasts
        for (int i = 0; i < lengthOfRayOutputs; i++)
        {
            GameObject goHit = rayOutputs[i].HitGameObject;
            if (goHit != null)
            {
                var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                var scaledRayLength = rayDirection.magnitude;
                float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

                // Print info:
                string dispStr = "";
                dispStr = dispStr + "__RayPerceptionSensor - HitInfo__:\r\n";
                dispStr = dispStr + "GameObject name: " + goHit.name + "\r\n";
                dispStr = dispStr + "Hit distance of Ray: " + rayHitDistance + "\r\n";
                dispStr = dispStr + "GameObject tag: " + goHit.tag + "\r\n";
                Debug.Log(dispStr);
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var rayOutputs = RayPerceptionSensor.Perceive(rayPerceptionSensor.GetRayPerceptionInput()).RayOutputs;
        int lengthOfRayOutputs = rayOutputs.Length;
        
        // Add observations and check for 'Female' tag
        for (int i = 0; i < lengthOfRayOutputs; i++)
        {
            var rayOutputResult = rayOutputs[i];
            Debug.Log($"Ray hit object: {rayOutputResult.HitTaggedObject}");
    
    
            // Add whether a hit was detected (1 if true, 0 if false)
            if (rayOutputResult.HasHit)
            {
                Debug.Log($"HASHIT: {rayOutputResult.HitGameObject.tag}");
                sensor.AddObservation(1);
                // Add the distance to the detected object as an observation
                sensor.AddObservation(rayOutputResult.HitFraction);
                // Add the tag of the detected object as an integer
                sensor.AddObservation(rayOutputResult.HitTagIndex);
            }
            else
            {
                sensor.AddObservation(0);
                // Add the distance to the detected object as an observation
                sensor.AddObservation(100);
                // Add the tag of the detected object as an integer
                sensor.AddObservation(100);
            }
            
            Debug.Log($"HAS HIT: {rayOutputResult.HasHit}");
    
            // Check if the detected object has the 'Female' tag and reward the agent
            if (rayOutputResult.HasHit && rayOutputResult.HitGameObject.gameObject.CompareTag("Female"))
            {
                // Reward the agent for detecting a 'Female' object
                castleArea.m_Team1AgentGroup.AddGroupReward(0.0001f);
                // m_AgentGroup.AddGroupReward(0.01f);
                
                Debug.Log("Detected a 'Female' object and rewarded the agent!");
                
                // Penalize the detected 'Female' agent
                GameObject femaleCollider = rayOutputResult.HitGameObject;
                CastleAgent femaleAgent = femaleCollider.GetComponent<CastleAgent>();
                if (femaleAgent != null)
                {
                    sensor.AddObservation(femaleAgent.transform.position);
                    castleArea.m_Team0AgentGroup.AddGroupReward(-0.0001f);
                    Debug.Log("Female agent detected by a male agent and penalized!");
                }
            }
            else
            {
                Vector3 infinity = new Vector3(100, 100, 100);
                sensor.AddObservation(infinity);
            }
        }
    
        // Add additional observations
        sensor.AddObservation(transform.position); // Agent's position
        sensor.AddObservation(CastleArea.numBricks); // Global number of bricks
        // Add other relevant observations if needed
    }
    
    // This function checks if the agent's next move will collide with any other object, including other agents
private bool WouldCollide(Vector3 moveVector)
{
    // Retrieve the BoxCollider component from the agent

    if (boxCollider == null)
    {
        Debug.LogError("BoxCollider not found on the agent.");
        return false; // If no collider is found, assume no collision for safety
    }

    // Use the bounds of the collider to get the correctly scaled size
    Vector3 boxSize = boxCollider.bounds.size;  // This gives the correct world size of the collider

    // Calculate the position to check for potential collisions
    // DrawWireCube(transform.position, boxSize, Color.yellow);
    Vector3 checkPosition = transform.position + moveVector;

    // Draw the bounding box of the agent at the intended new position using lines
    // DrawWireCube(checkPosition, boxSize, Color.green);

    // Perform collision detection using OverlapBox to gather colliders
    Collider[] hitColliders = Physics.OverlapBox(checkPosition, boxSize / 2, Quaternion.identity, ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);

    // Check if there are any colliders and log their details
    // foreach (Collider hitCollider in hitColliders)
    // {
    //     // Log the layer name and tag of the object with which it would collide
    //     Debug.Log($"Potential Collision with: {hitCollider.gameObject.name}, Layer: {LayerMask.LayerToName(hitCollider.gameObject.layer)}, Tag: {hitCollider.gameObject.tag}");
    //
    //     // Draw the collider of the potential colliding object if it has a BoxCollider
    //     BoxCollider otherBoxCollider = hitCollider as BoxCollider;
    //     if (otherBoxCollider != null)
    //     {
    //         // Use the bounds to get the correctly scaled size of the other collider
    //         Vector3 otherBoxSize = otherBoxCollider.bounds.size;
    //         Vector3 otherBoxPosition = otherBoxCollider.bounds.center; // Use bounds.center for accurate position
    //
    //         // Draw the bounding box of the object that would be collided with
    //         DrawWireCube(otherBoxPosition, otherBoxSize, Color.red);
    //     }
    //     else
    //     {
    //         // Draw bounds using DrawLine as a fallback for non-box colliders
    //         Debug.DrawLine(hitCollider.bounds.min, hitCollider.bounds.max, Color.red);
    //     }
    // }

    // Return true if there is any collider detected, indicating a potential collision
    return hitColliders.Length > 0;
}

// Helper method to draw a wireframe cube using Debug.DrawLine
private void DrawWireCube(Vector3 position, Vector3 size, Color color)
{
    Vector3 halfSize = size / 2;

    // Calculate all 8 corners of the cube
    Vector3[] corners = new Vector3[8];
    corners[0] = position + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
    corners[1] = position + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
    corners[2] = position + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
    corners[3] = position + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
    corners[4] = position + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
    corners[5] = position + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
    corners[6] = position + new Vector3(halfSize.x, halfSize.y, halfSize.z);
    corners[7] = position + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

    // Draw bottom square
    Debug.DrawLine(corners[0], corners[1], color);
    Debug.DrawLine(corners[1], corners[2], color);
    Debug.DrawLine(corners[2], corners[3], color);
    Debug.DrawLine(corners[3], corners[0], color);

    // Draw top square
    Debug.DrawLine(corners[4], corners[5], color);
    Debug.DrawLine(corners[5], corners[6], color);
    Debug.DrawLine(corners[6], corners[7], color);
    Debug.DrawLine(corners[7], corners[4], color);

    // Draw vertical lines
    Debug.DrawLine(corners[0], corners[4], color);
    Debug.DrawLine(corners[1], corners[5], color);
    Debug.DrawLine(corners[2], corners[6], color);
    Debug.DrawLine(corners[3], corners[7], color);
}



    
    // Mask or unmask the pick-up action based on collision status
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // Mask movement actions that would result in a collision
        for (int moveDirection = 1; moveDirection <= 2; moveDirection++) // Assuming 1: Forward, 2: Backward
        {
            Vector3 moveVector = Vector3.zero;

            switch (moveDirection)
            {
                case 1:
                    // Move forward
                    moveVector = transform.forward * moveSpeed;
                    break;
                case 2:
                    // Move backward
                    moveVector = -transform.forward * moveSpeed;
                    break;
            }

            // Mask the action if the movement would result in a collision
            if (WouldCollide(moveVector))
            {
                Debug.Log($"COLLIDES");
                actionMask.SetActionEnabled(0, moveDirection, false); // Assuming move actions are indexed at branch 0
            }
            else
            {
                Debug.Log($"DOES NOT COLLIDE");
                actionMask.SetActionEnabled(0, moveDirection, true);
            }
        }
        
        if (!canPickUp || pickedUpObject != null)
        {
            // Mask the pick-up action if the agent can't pick up or already holds something
            actionMask.SetActionEnabled(2, 0, false);
        }
        else
        {
            // Unmask the pick-up action if the agent can pick up
            actionMask.SetActionEnabled(2, 0, true);
        }
        
        if (pickedUpObject != null)
        {
            Collider objectCollider = pickedUpObject.GetComponent<Collider>();
            if (objectCollider != null)
            {
                Vector3 halfExtents = objectCollider.bounds.extents;
                Vector3 dropPosition = transform.position + transform.forward * (halfExtents.z + 0.5f);
                // There is enough space to place the gameobject and not collide with something else pickable
                if (Physics.CheckBox(dropPosition, halfExtents, Quaternion.identity, LayerMask.GetMask("Pickable")))
                {
                    // Mask the drop action (assuming the drop action index is 1)
                    actionMask.SetActionEnabled(2, 1, false);
                }
                else
                {
                    actionMask.SetActionEnabled(2, 1, true);
                }
            }
        }
        else
        {
            actionMask.SetActionEnabled(2, 1, false);
        }
        
        
        // Try to subtract the necessary Bricks
        if (CastleArea.CheckSubtractBricks(brickCostPerObject) && CastleArea.BricksTimeFunction())
        {
            canBuild = true;
            actionMask.SetActionEnabled(2, 3, true);
            // Debug.Log("CAN BUILD==================");
        }
        else
        {
            canBuild = false;
            actionMask.SetActionEnabled(2, 3, false);
        }
        
        // destroy pickable object
        if (objectToDestroy == null)
        {
            // Mask the destroy action (assuming the destroy action index is 4)
            actionMask.SetActionEnabled(2, 4, false);
        }
        else
        {
            // Unmask the destroy action if there is an object to destroy
            actionMask.SetActionEnabled(2, 4, true);
        }
    }
    
    
    // Handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is in the "Pickable" layer
        Debug.Log($"COLLI: {collision}");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pickable"))
        {
            canPickUp = true;
            objectToDestroy = collision.gameObject;
            Rigidbody rbObj = collision.gameObject.GetComponent<Rigidbody>();
            if (rbObj != null)
            {
                rbObj.isKinematic = true;  // Make the object stationary
            }
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pickable"))
        {
            if (collision.gameObject == objectToDestroy)
            {
                objectToDestroy = null;
            }
        }
    }

// Function to pick up the object
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

            // Save the object's original rotation
            // Set the original rotation to (0, 0, 0)
            startingRot = Quaternion.identity;

            // Disable the object's physics and collider to prevent distortion and interference with Raycasts
            Rigidbody rbPickable = pickedUpObject.GetComponent<Rigidbody>();
            if (rbPickable != null)
            {
                rbPickable.isKinematic = true;
                rbPickable.useGravity = false;
            }

            Collider objectCollider = pickedUpObject.GetComponent<Collider>();
            if (objectCollider != null)
            {
                objectCollider.enabled = false; // Disable the collider
            }

            // Attach the object to the agent, ensuring no interference with the agent's collider
            pickedUpObject.transform.SetParent(this.transform);
            pickedUpObject.transform.localPosition = new Vector3(0, 1, 1); // Adjust as needed
            pickedUpObject.transform.localRotation = Quaternion.identity; // Keeps the object aligned with the agent

            // Set the object's scale to default (1,1,1) to prevent any distortion
            pickedUpObject.transform.localScale = Vector3.one;

            canPickUp = false; // The agent can no longer pick up until it drops the current object
        }
    }
}

// Function to drop the object
private void DropObject()
{
    if (pickedUpObject != null)
    {
        Collider objectCollider = pickedUpObject.GetComponent<Collider>();
        if (objectCollider != null)
        {
            Vector3 halfExtents = objectCollider.bounds.extents;
            Vector3 dropPosition = transform.position + transform.forward * (halfExtents.z + 0.5f);

            if (!Physics.CheckBox(dropPosition, halfExtents, Quaternion.identity, LayerMask.GetMask("Pickable")))
            {
                pickedUpObject.transform.SetParent(null);
                pickedUpObject.transform.position = dropPosition;
                pickedUpObject.transform.rotation = Quaternion.identity;

                Rigidbody rbPickable = pickedUpObject.GetComponent<Rigidbody>();
                if (rbPickable != null)
                {
                    rbPickable.isKinematic = false;
                    rbPickable.useGravity = true;
                }

                objectCollider.enabled = true;

                pickedUpObject.transform.localScale = Vector3.one;
                pickedUpObject = null;
                canPickUp = true;
            }
            else
            {
                Debug.Log("Not enough space to drop the object safely!");
            }
        }
    }
}


// Function to create a new prop in the environment
    public GameObject CreateProp(string propName, Vector3 position, Quaternion rotation)
    {
        
        Debug.Log($"CAN BUILD: {canBuild}");
        // Check if the propName is valid
        if (System.Array.IndexOf(propNames, propName) < 0)
        {
            Debug.LogError("Invalid prop name.");
            return null;
        }
        
        // Load the prefab from the Resources folder
        GameObject propPrefab = Resources.Load<GameObject>($"Prefabs/{propName}");

        if (propPrefab != null)
        {
            // Instantiate the prefab in the scene at the origin
            Instantiate(propPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Could not find the prop '{propName}' in the Props folder.");
        }

        // Load the prefab from the Props folder
        // GameObject propPrefab = Resources.Load<GameObject>($"Prefabs/{propName}");
        // if (propPrefab == null)
        // {
        //     Debug.LogError($"Could not find the prop '{propName}' in the Props folder.");
        //     return null;
        // }

        // Create the prop in the environment
        GameObject newProp = Instantiate(propPrefab, position, rotation);

        // Set the layer of the new prop to 'Pickable'
        newProp.layer = LayerMask.NameToLayer("Pickable");
        // Return the newly created GameObject
        return newProp;
    }
    
    // Example usage of CreateProp function
    public void CreateColumnProp()
    {
        // Define the distance in front of the agent where the object will be placed
        float distanceInFront = 2.0f; // Adjust this value as needed

        // Calculate the position in front of the agent
        Vector3 position = transform.position + transform.forward * distanceInFront;

        // Keep the rotation the same as the agent's rotation
        Quaternion rotation = transform.rotation;

        // Create a 'floor' prop at the calculated position and rotation
        CreateProp("column_mini", position, rotation);
        CastleArea.SubtractBricks(brickCostPerObject);
        Debug.Log($"Take: {CastleArea.numBricks}");

        // AddReward(0.001f);
        // if the team scores a goal
        castleArea.m_Team1AgentGroup.AddGroupReward(0.0001f);
    }
    
    // Function to destroy the object
    private void DestroyObject()
    {
        if (objectToDestroy != null)
        {
            // Add the brick cost to the Bricks variable
            CastleArea.AddBricks(brickCostPerObject);
            Debug.Log($"Add: {CastleArea.numBricks}");
            // Destroy the game object
            Destroy(objectToDestroy);

            // Clear the reference
            objectToDestroy = null;
        }
    }


        public override void OnActionReceived(ActionBuffers actions)
        {
            // float moveRotate = actions.ContinuousActions[0];
            // Debug.Log($"ROT PRED: {moveRotate}");
            // float moveForward = actions.ContinuousActions[1];
            // // Move the agent forward/backward
            // if (rb != null)
            // {
            //     Vector3 moveDirection = transform.forward * moveForward * moveSpeed * Time.deltaTime;
            //     rb.MovePosition(transform.position + moveDirection);
            //     Debug.Log($"MOVE: {moveDirection}");
            // }
            //
            // // Rotate the agent
            // if (rb != null)
            // {
            //     float rotationAngle = moveRotate * rotateSpeed * Time.deltaTime; // Ensure rotation is frame-rate independent
            //     rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationAngle, 0f)); // Usi
            // }
            
            int moveDirection = actions.DiscreteActions[0];  // Discrete action for movement
            int rotationDirection = actions.DiscreteActions[1];  // Discrete action for rotation

            // Define movement and rotation step sizes
            float moveStep = moveSpeed * Time.deltaTime;
            float rotationStep = rotateSpeed * Time.deltaTime;  // Assuming you have a rotationSpeed variable
            // Handle movement based on discrete actions
            if (rb != null)
            {
                Vector3 moveVector = Vector3.zero;

                switch (moveDirection)
                {
                    case 0:
                        // No movement
                        Debug.Log($"STOP: { moveStep}");
                        break;
                    case 1:
                        // Move forward
                        moveVector = transform.forward * moveStep;
                        Debug.Log($"FORWARD: { moveVector}");
                        break;
                    case 2:
                        // Move backward
                        moveVector = -transform.forward * moveStep;
                        Debug.Log($"BACK: { moveVector}");
                        break;
                    default:
                        // No movement by default
                        break;
                }
                Debug.Log($"MOVE: { moveVector}");

                // Apply movement
                rb.MovePosition(transform.position + moveVector);
            }

            // Handle rotation based on discrete actions
            if (rb != null)
            {
                float rotationAngle = 0f;

                switch (rotationDirection)
                {
                    case 0:
                        // No rotation
                        break;
                    case 1:
                        // Rotate left
                        rotationAngle = -rotationStep;
                        break;
                    case 2:
                        // Rotate right
                        rotationAngle = rotationStep;
                        break;
                    default:
                        // No rotation by default
                        break;
                }

                // Apply rotation
                Quaternion deltaRotation = Quaternion.Euler(0f, rotationAngle * rotationStep , 0f);
                Quaternion newRotation = rb.rotation * deltaRotation; // Apply the change in rotation relative to self

                // Fix x and z axis rotations to 0, keeping only the y-axis rotation
                newRotation = Quaternion.Euler(0f, newRotation.eulerAngles.y, 0f);

                // Apply the constrained rotation to the Rigidbody
                rb.MoveRotation(newRotation);
                Debug.Log($"ROTATION: {newRotation}");

                // transform.Rotate(0f, rotationAngle, 0f, Space.Self);
            }
            
            // Get the discrete actions for pick up and drop
            int discreteAction = actions.DiscreteActions[2];
            bool pickUpAction;
            bool dropAction;
            bool noBuildAction;
            if (discreteAction == 0)
            {   
                Debug.Log("ACTION: PICKUP");
                pickUpAction = true;
            } else { pickUpAction = false;}
            if (discreteAction == 1)
            {
                Debug.Log("ACTION: DROP");
                dropAction = true;
            } else { dropAction = false;}
            if (discreteAction == 2)
            {
                Debug.Log("ACTION: NOTHING");
                noBuildAction = true;
            } else { noBuildAction = false;}
            if (discreteAction == 3)
            {
                Debug.Log("ACTION: CREATE");
                // Debug.Log("SELECTED BUILD ACTION.");
                CreateColumnProp();
            }

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
            
            if (discreteAction == 4)
            {
                DestroyObject();
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
            
            castleArea.m_Team1AgentGroup.AddGroupReward(-0.1f);
            
            Debug.Log("Caught female");
            castleArea.ResetAgent(this.gameObject);
            
        }
        
        // if (other.gameObject.tag =="Wall")
        // {
        //     // Debug.Log("hit wall");
        //     envMaterial.color = Color.red;
        //     // AddReward(-0.5f);
        //     AddReward(-0.5f);
        //     castleArea.UpdateScore(GetCumulativeReward());
        //     Debug.Log("hit wall");
        //     // agentArea.ResetAgent(this.gameObject);
        //     m_AgentGroup.EndGroupEpisode();
        //     castleArea.ResetArea();
        //
        // }
        
    }
    
    
}
