using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CastleAgent : Agent
{
    // target variabvles
    private RayPerceptionSensorComponent3D rayPerceptionSensor;
    [SerializeField] private Transform targetTransform;
    // [SerializeField] private List<GameObject> spawnedTargetList = new List<GameObject>();
    // public int targetCount;
    // public GameObject food;
    private float RAY_RANGE = 5f;
    
    // agent variables
    [Header("TEAM")]

    public int teamID = 0;
    [SerializeField] private float moveSpeed = 1.0f;
    private float rotateSpeed = 100f;
    private Rigidbody rb;
    BoxCollider boxCollider;
    
    // env variables
    private Transform envLocation;
    Material envMaterial;
    public GameObject env;
    
    // build variables
    private float penaltyForNothingAction = -0.00001f;
    // private string[] propNames = { "floor", "floorStairs", "column_mini", "Sphere", "SphereF" }; // List of props
    private string[] propNames = { "floor", "floorStairs", "column_mini", "floor Variant",
        "floor_stairs Variant","wallPaint_half_mini","wallPaint_flat Variant" }; // List of props

    private RaycastHit hit;
    // private RayPerceptionSensorComponent3D rayPerception;
    // Reference to the picked-up object
    private GameObject pickedUpObject;
    private GameObject objectToDestroy = null;
    private bool canDestroy = false;
    private bool canPickUp = false;
    private bool canBuild = true;
    // Class-level variable to store the original rotation of the picked-up object
    // // env variables
    private CastleArea castleArea;
    // Class-level variable to store the original rotation of the picked-up object
    private Quaternion startingRot;
    private Vector3 startingPos;
    
    // multi agents setup




    public override void Initialize()
    {
        // Initialize the Multi-Agent Group
        envLocation = transform.parent.gameObject.transform;
        rayPerceptionSensor = GetComponent<RayPerceptionSensorComponent3D>();

        if (rayPerceptionSensor == null)
        {
            Debug.LogError("RayPerceptionSensorComponent3D not found!");
        }
        
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        // castleArea = transform.parent.GetComponent<CastleArea>();
        castleArea = GetComponentInParent<CastleArea>();
        if (castleArea == null)
        {
            Debug.LogError("castleArea or m_Team0AgentGroup is not initialized.");
        }
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
    }
    

    // public override void OnEpisodeBegin()
    // {
    //     // Register all agents in the group at the start of the episode
    //     
    //     // transform.localPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
    //     
    //     CreateFoodProp();
    //     
    // }
    

    // private void CreateTarget()
    // {
    //     if (spawnedTargetList.Count != 0)
    //     {
    //         removeTarget(spawnedTargetList);
    //     }
    //     
    //     for (int i = 0; i < targetCount; i++)
    //     {
    //         int counter = 0;
    //         bool distanceGood;
    //         bool alreadyDecremented = false;
    //         //  spawning object target
    //         GameObject newTarget = Instantiate(food);
    //         // make target child of env 
    //         newTarget.transform.parent = envLocation;
    //         // give random spawn location
    //         // float x = r.Next(0, 5);
    //         // float z = r.Next(0, 5);
    //         Vector3 targetLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
    //         // check if distance is good
    //         if (spawnedTargetList.Count != 0)
    //         {
    //             for (int k = 0; k < spawnedTargetList.Count; k++)
    //             {
    //                 if (counter < 10)
    //                 {
    //                     distanceGood = CheckOverlap(targetLocation, spawnedTargetList[k].transform.localPosition, RAY_RANGE);
    //                     if (distanceGood == false)
    //                     {
    //                         targetLocation = new Vector3(UnityEngine.Random.Range(-20f, 20f), 0, UnityEngine.Random.Range(-10f, 10f));
    //                         k--;
    //                         alreadyDecremented = true;
    //
    //                     }
    //                     
    //                     distanceGood = CheckOverlap(targetLocation, transform.localPosition, RAY_RANGE);
    //                     if (distanceGood == false)
    //                     {
    //                         targetLocation = new Vector3(UnityEngine.Random.Range(-20f, 20f), 0, UnityEngine.Random.Range(-10f, 10f));
    //                         if (alreadyDecremented == false)
    //                         {
    //                             k--;
    //                         }
    //
    //                     }
    //
    //                     counter++;
    //                 }
    //                 else
    //                 {
    //                     // exit the loop
    //                     k = spawnedTargetList.Count;
    //                 }
    //             }
    //         }
    //         
    //         // spawn in new location
    //         newTarget.transform.localPosition = targetLocation;
    //         spawnedTargetList.Add(newTarget);
    //     } 
    // }

    // private void removeTarget(List<GameObject> targetsToDelete)
    // {
    //     
    //     foreach (var target in targetsToDelete)
    //     {
    //         Destroy(target.gameObject);
    //     }
    //     targetsToDelete.Clear();
    // }

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


        var rayOutputs = RayPerceptionSensor.Perceive(rayPerceptionSensor.GetRayPerceptionInput()).RayOutputs;
        int lengthOfRayOutputs = rayOutputs.Length;
        
        // Add observations and check for 'Female' tag
        for (int i = 0; i < lengthOfRayOutputs; i++)
        {
            var rayOutputResult = rayOutputs[i];
            // Add whether a hit was detected (1 if true, 0 if false)
            if (rayOutputResult.HasHit)
            {
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
            
            if (rayOutputResult.HasHit && rayOutputResult.HitGameObject.CompareTag("Female"))
            {
                // Reward the agent for detecting a 'Female' object
                if (castleArea.m_Team0AgentGroup != null)
                {
                    castleArea.m_Team0AgentGroup.AddGroupReward(0.0001f);
                }
                else
                {
                    Debug.LogError($"not init {castleArea.m_Team0AgentGroup}");
                }
                // m_AgentGroup.AddGroupReward(0.01f);
                // Debug.Log("Detected a 'FEMALE' object and rewarded the agent!");
            }
            
            else if (rayOutputResult.HasHit && rayOutputResult.HitGameObject.CompareTag("Male"))
            {
                // Reward the agent for detecting a 'Female' object
                castleArea.m_Team0AgentGroup.AddGroupReward(0.0001f);
                // m_AgentGroup.AddGroupReward(0.01f);
                // Debug.Log("Detected a 'MALE' object and rewarded the agent!");
            }

            if (!rayOutputResult.HasHit)
            {
                Vector3 infinity = new Vector3(100, 100, 100);
                sensor.AddObservation(infinity);
            }
        }

        // Add additional observations
        sensor.AddObservation(transform.position); // Agent's position
        sensor.AddObservation(CastleArea.numBricks); // Global number of bricks
        // Add other relevant observations if needed

        // if (targetTransform != null)
        // {
        //     sensor.AddObservation(targetTransform.position);   
        // }
        // else
        // {
        //     Vector3 infinity = new Vector3(100, 100, 100);
        //     sensor.AddObservation(infinity);
        // }
        
    }

    private bool WouldCollide(Vector3 moveVector)
    {
        // Define a bounding box for collision detection (adjust size and position as needed)
        // Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f); // Example size, adjust for your agent's dimensions
        // Retrieve the BoxCollider component from the agent
    
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider not found on the agent.");
            return false; // If no collider is found, assume no collision for safety
        }

        // Calculate the actual size of the BoxCollider, accounting for the object's scale
        Vector3 boxSize = boxCollider.bounds.size; 
        // Calculate the position to check for potential collisions
        Vector3 checkPosition = transform.position + moveVector;

        // Perform collision detection using OverlapBox to gather colliders
        Collider[] hitColliders = Physics.OverlapBox(checkPosition, boxSize / 2, Quaternion.identity, ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);

        // Check if there are any colliders and log their details
        // foreach (Collider hitCollider in hitColliders)
        // {
        //     // Log the layer name and tag of the object with which it would collide
        //     Debug.Log($"Potential Collision with: {hitCollider.gameObject.name}, Layer: {LayerMask.LayerToName(hitCollider.gameObject.layer)}, Tag: {hitCollider.gameObject.tag}");
        // }

        // Return true if there is any collider detected, indicating a potential collision
        return hitColliders.Length > 0;
    }
    
    // Mask or unmask the pick-up action based on collision status
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
                // Debug.Log($"COLLIDES");
                actionMask.SetActionEnabled(0, moveDirection, false); // Assuming move actions are indexed at branch 0
            }
            else
            {
                // Debug.Log($"DOES NOT COLLIDE");
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
        if (CastleArea.CheckSubtractBricks(CastleArea.brickCostPerObject) && CastleArea.BricksTimeFunction())
        {
            canBuild = true;
            // Debug.Log("CAN BUILD==================");
            // Now, set action masks for branch 3 (propAction)
            // First, unmask all propActions
            actionMask.SetActionEnabled(3, 1, true); // 'column_mini'
            actionMask.SetActionEnabled(3, 2, true); // 'floor Variant'
            actionMask.SetActionEnabled(3, 3, true); // 'floor_stairs Variant'
            actionMask.SetActionEnabled(3, 4, true); // 'wallPaint_half_mini'
            actionMask.SetActionEnabled(3, 5, true); // 'wallPaint_flat Variant'
            
            // Special case for 'floor_stairs Variant'
            if (CheckIfFloorBeneath())
            {
                // Mask the action to create 'floor_stairs Variant' (propAction index 2)
                actionMask.SetActionEnabled(3, 2, false);
            }
            else
            {
                actionMask.SetActionEnabled(3, 3, false);
            }
        }
        else
        {
            canBuild = false;
            // Mask all actions in branch 3
            for (int i = 1; i < 6; i++)
            {
                actionMask.SetActionEnabled(3, i, false);
            }
            
        }
        
        // destroy pickable object
        if (objectToDestroy == null)
        {
            // Mask the destroy action (assuming the destroy action index is 4)
            actionMask.SetActionEnabled(2, 3, false);
        }
        else
        {
            // Unmask the destroy action if there is an object to destroy
            actionMask.SetActionEnabled(2, 3, true);
        }
    }
    
    
    // Handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is in the "Pickable" layer
        
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
    // public GameObject CreateProp(string propName, Vector3 position, Quaternion rotation)
    // {
    //     // Check if the propName is valid
    //     if (System.Array.IndexOf(propNames, propName) < 0)
    //     {
    //         Debug.LogError("Invalid prop name.");
    //         return null;
    //     }
    //     
    //     // Load the prefab from the Resources folder
    //     GameObject propPrefab = Resources.Load<GameObject>($"Prefabs/{propName}");
    //
    //     if (propPrefab != null)
    //     {
    //         // Instantiate the prefab in the scene at the origin
    //         Instantiate(propPrefab, Vector3.zero, Quaternion.identity);
    //     }
    //     else
    //     {
    //         Debug.LogError($"Could not find the prop '{propName}' in the Props folder.");
    //     }
    //
    //     // Load the prefab from the Props folder
    //     // GameObject propPrefab = Resources.Load<GameObject>($"Prefabs/{propName}");
    //     // if (propPrefab == null)
    //     // {
    //     //     Debug.LogError($"Could not find the prop '{propName}' in the Props folder.");
    //     //     return null;
    //     // }
    //
    //     // Create the prop in the environment
    //     GameObject newProp = Instantiate(propPrefab, position, rotation);
    //
    //     // Set the layer of the new prop to 'Pickable'
    //     newProp.layer = LayerMask.NameToLayer("Pickable");
    //
    //     // Return the newly created GameObject
    //     return newProp;
    // }
    
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
        castleArea.CreateProp("column_mini", position, rotation);
        CastleArea.SubtractBricks(CastleArea.brickCostPerObject);
        // Debug.Log($"Take: {CastleArea.numBricks}");

        // m_AgentGroup.AddGroupReward(0.001f);
        castleArea.m_Team0AgentGroup.AddGroupReward(0.0001f);
    }
    
        // Function to handle prop creation based on propAction
    void CreatePropAction(int propAction)
    {
        Debug.Log($"CREATING PROP {propAction}");
        string propName = "";
        switch (propAction)
        {
            case 1:
                propName = "column_mini";
                CreatePropColumn();
                break;
            case 2:
                propName = "floor Variant";
                CreatePropFloor();
                break;
            case 3:
                propName = "floor_stairs Variant";
                CreatePropStairs();
                break;
            case 4:
                propName = "wallPaint_half_mini";
                CreatePropWall();
                break;
            case 5:
                propName = "wallPaint_flat Variant";
                CreatePropArch();
                break;
            default:
                Debug.LogError("Invalid prop action");
                return;
        }

    }

    // Function to create a new prop in the environment
    public GameObject CreateProp(string propName, Vector3 position, Quaternion rotation)
    {
        // Check if the propName is valid
        if (System.Array.IndexOf(propNames, propName) < 0)
        {
            Debug.LogError("Invalid prop name.");
            return null;
        }
    
        // Load the prefab from the Resources folder
        GameObject propPrefab = Resources.Load<GameObject>($"Prefabs/{propName}");

        if (propPrefab == null)
        {
            Debug.LogError($"Could not find the prop '{propName}' in the Prefabs folder.");
            return null;
        }

        // Instantiate the prefab at the desired position and rotation
        GameObject newProp = Instantiate(propPrefab, position, rotation);

        // Set the scale of the new prop to (1, 1, 1)
        newProp.transform.localScale = Vector3.one;

        // Set the layer of the new prop to 'Pickable'
        newProp.layer = LayerMask.NameToLayer("Pickable");

        // Return the newly created GameObject
        return newProp;
    }

    
    // Example usage of CreateProp function
    public void CreatePropColumn()
    {
        // Define the distance in front of the agent where the object will be placed
        float distanceInFront = 2.0f; // Adjust this value as needed

        // Calculate the position in front of the agent
        Vector3 position = transform.position + transform.forward * distanceInFront;

        // Keep the rotation the same as the agent's rotation
        Quaternion rotation = transform.rotation;

        // Create a 'floor' prop at the calculated position and rotation
        CreateProp("column_mini", position, rotation);
        CastleArea.SubtractBricks(CastleArea.brickCostPerObject);
        // Debug.Log($"Take: {CastleArea.numBricks}");

        // AddReward(0.001f);
        // if the team scores a goal
        castleArea.m_Team1AgentGroup.AddGroupReward(0.0001f);
    }
    
    
    // Function to create a floor prop
    public void CreatePropFloor()
    {
        // Define the distance in front of the agent where the object will be placed
        float distanceInFront = 2.0f; // Adjust this value as needed
    
        // Calculate the position in front of the agent
        Vector3 position = transform.position + transform.forward * distanceInFront;
    
        // Keep the rotation the same as the agent's rotation
        Quaternion rotation = transform.rotation;
    
        // Create a 'floor Variant' prop at the calculated position and rotation
        CreateProp("floor Variant", position, rotation);
        CastleArea.SubtractBricks(CastleArea.brickCostPerObject);
    
        // Add a small group reward
        castleArea.m_Team1AgentGroup.AddGroupReward(0.0001f);
    }
    
    // Function to create a stairs prop
    public void CreatePropStairs()
    {
        // Define the distance in front of the agent where the object will be placed
        float distanceInFront = 2.0f; // Adjust this value as needed
    
        // Calculate the position in front of the agent
        Vector3 position = transform.position + transform.forward * distanceInFront;
    
        // Adjust rotation if needed for stairs alignment
        Quaternion rotation = transform.rotation;
    
        // Create a 'floor_stairs Variant' prop at the calculated position and rotation
        CreateProp("floor_stairs Variant", position, rotation);
        CastleArea.SubtractBricks(CastleArea.brickCostPerObject);
    
        // Add a small group reward
        castleArea.m_Team1AgentGroup.AddGroupReward(0.0001f);
    }
    
    // Function to create a wall prop
    public void CreatePropWall()
    {
        // Define the distance in front of the agent where the object will be placed
        float distanceInFront = 2.0f; // Adjust this value as needed
    
        // Calculate the position in front of the agent
        Vector3 position = transform.position + transform.forward * distanceInFront;
    
        // Keep the rotation the same as the agent's rotation
        Quaternion rotation = transform.rotation;
    
        // Create a 'wallPaint_half_mini' prop at the calculated position and rotation
        CreateProp("wallPaint_half_mini", position, rotation);
        CastleArea.SubtractBricks(CastleArea.brickCostPerObject);
    
        // Add a small group reward
        castleArea.m_Team1AgentGroup.AddGroupReward(0.0001f);
    }
    
    // Function to create an arch prop
    public void CreatePropArch()
    {
        // Define the distance in front of the agent where the object will be placed
        float distanceInFront = 2.0f; // Adjust this value as needed
    
        // Calculate the position in front of the agent
        Vector3 position = transform.position + transform.forward * distanceInFront;
    
        // Keep the rotation the same as the agent's rotation
        Quaternion rotation = transform.rotation;
    
        // Create a 'wallPaint_flat Variant' prop at the calculated position and rotation
        CreateProp("wallPaint_flat Variant", position, rotation);
        CastleArea.SubtractBricks(CastleArea.brickCostPerObject);
    
        // Add a small group reward
        castleArea.m_Team1AgentGroup.AddGroupReward(0.0001f);
    }

    
    // Check if there's a 'Floor' tagged object beneath the agent
    bool CheckIfFloorBeneath()
    {
        RaycastHit hit;
        float distanceToGround = 1.0f; // Adjust as needed
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distanceToGround))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                return true;
            }
        }
        return false;
    }
    
    
    // public void CreateFoodProp()
    // {
    //     if (spawnedTargetList.Count != 0)
    //     {
    //         removeTarget(spawnedTargetList);
    //     }
    //     for (int i = 0; i < targetCount; i++)
    //     {
    //         Vector3 randomLocalPosition =
    //             new Vector3(UnityEngine.Random.Range(-25, 25), 0, UnityEngine.Random.Range(-7, 7));
    //         // Keep the rotation the same as the agent's rotation
    //         Quaternion rotation = transform.localRotation;
    //
    //         // Create a 'floor' prop at the calculated position and rotation
    //         GameObject newTarget = CreateProp("SphereF", randomLocalPosition, rotation);
    //         newTarget.transform.localPosition.Scale(transform.localScale);
    //         castleArea.RandomlyPlaceObject(newTarget, 20, 10);
    //         newTarget.transform.parent = envLocation;
    //         spawnedTargetList.Add(newTarget);
    //         Debug.Log("Created sphere");
    //     }
    //
    // }
    
    // Function to destroy the object
    private void DestroyObject()
    {
        if (objectToDestroy != null)
        {
            // Add the brick cost to the Bricks variable
            CastleArea.AddBricks(CastleArea.brickCostPerObject);
            // Debug.Log($"Add: {CastleArea.numBricks}");
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
            int discreteAction = actions.DiscreteActions[2]; // Pick up/drop/destroy actions
            int propAction = actions.DiscreteActions[3];     

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
                        // Debug.Log($"STOP: { moveStep}");
                        break;
                    case 1:
                        // Move forward
                        moveVector = transform.forward * moveStep;
                        // Debug.Log($"FORWARD: { moveVector}");
                        break;
                    case 2:
                        // Move backward
                        moveVector = -transform.forward * moveStep;
                        // Debug.Log($"BACK: { moveVector}");
                        break;
                    default:
                        // No movement by default
                        break;
                }
                // Debug.Log($"MOVE: { moveVector}");

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
                // Debug.Log($"ROTATION: {newRotation}");

                // transform.Rotate(0f, rotationAngle, 0f, Space.Self);
            }
            
            // Get the discrete actions for pick up and drop
            bool pickUpAction;
            bool dropAction;
            bool noBuildAction = false;
            if (discreteAction == 0)
            {   
                // Debug.Log("ACTION: PICKUP");
                pickUpAction = true;
                noBuildAction = true;
            } else { pickUpAction = false;}
            if (discreteAction == 1)
            {
                // Debug.Log("ACTION: DROP");
                dropAction = true;
                noBuildAction = true;
            } else { dropAction = false;}
            if (discreteAction == 2)
            {
                // Debug.Log("ACTION: NOTHING");
                castleArea.m_Team1AgentGroup.AddGroupReward(penaltyForNothingAction);
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
                noBuildAction = true;
            }
            // discreteActionBuild = 0 is no build
            Debug.Log($"discreteaction build: {propAction}");
            if (propAction != 0 && noBuildAction == false)
            {
                // Build action
                CreatePropAction(propAction);
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
        // Debug.Log("ENTERED SPHEREF");
        if (other.gameObject.tag == "SphereF")
        {
            Debug.Log("Tagged SphereF");
            
            castleArea.spawnedTargetListF.Remove(other.gameObject);
            Destroy(other.gameObject);
            // Debug.Log($"NameAgent : {envLocation.gameObject.name}");
            castleArea.CreateFoodPropFemale(envLocation);
            castleArea.m_Team0AgentGroup.AddGroupReward(0.1f);
            
            if (castleArea.spawnedTargetListF.Count == 0)
            {
                envMaterial.color = Color.green;
                castleArea.m_Team0AgentGroup.AddGroupReward(0.5f);
                // removeTarget(spawnedTargetList);
                castleArea.m_Team0AgentGroup.EndGroupEpisode();
            }
            
        }
        
        if (other.gameObject.tag =="Male")
        {
            
            castleArea.m_Team0AgentGroup.AddGroupReward(-0.1f);
            envMaterial.color = Color.yellow;
            // Debug.Log("Caught male");
            castleArea.ResetAgent(this.gameObject);
        }
        
        if (other.gameObject.tag =="Boundary")
        {
            // Debug.Log("hit wall");
            envMaterial.color = Color.red;
            // AddReward(-0.5f);
            castleArea.m_Team0AgentGroup.AddGroupReward(-0.5f);
            castleArea.UpdateScore(GetCumulativeReward());
            Debug.Log("hit wall");
            // agentArea.ResetAgent(this.gameObject);
            castleArea.ResetAgent(this.gameObject);
        
        }
    }
}
