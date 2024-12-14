using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CastleAgent : Agent
{
    private GameObject stairsObject;

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
    // Room reward
    // Number of rays to cast around the point
    private int numberOfRays = 36;
    // Maximum distance for each raycast
    private float rayDistance = 2.5f;
    // Tag to detect as wall
    private string wallTag = "Wall";
    // Threshold proportion to consider the space enclosed
    private float enclosureThreshold = 0.75f;




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
        // Room Reward function
        // Assume the agent's position is the point of interest
        Vector3 point = transform.position;

        // Calculate the reward
        float enclosureReward = CalculateEnclosureReward(point);

        Debug.Log($"ROOM REWARD: {enclosureReward}");

        // Use the reward in your reinforcement learning algorithm
        castleArea.m_Team1AgentGroup.AddGroupReward(enclosureReward);
        
    }

// This function checks if the agent's next move will collide with any other object, including other agents
private bool WouldCollide(Vector3 moveVector)
{
    // Retrieve all Collider components from the agent
    Collider[] colliders = GetComponents<Collider>();

    if (colliders == null || colliders.Length == 0)
    {
        Debug.LogError("No Colliders found on the agent.");
        return false; // If no colliders are found, assume no collision for safety
    }

    // Iterate through each collider attached to the agent
    foreach (Collider agentCollider in colliders)
    {
        // Skip trigger colliders if they should not be considered for movement collision
        if (agentCollider.isTrigger)
        {
            continue;
        }

        // Use the bounds of the collider to get the correctly scaled size
        Vector3 boxSize = agentCollider.bounds.size;  // This gives the correct world size of the collider

        // Calculate the position to check for potential collisions
        Vector3 colliderCenterOffset = agentCollider.bounds.center - transform.position;
        Vector3 checkPosition = transform.position + moveVector + colliderCenterOffset;

        // Use the agent's rotation for accurate collision detection
        Quaternion rotation = transform.rotation;

        // Perform collision detection using OverlapBox to gather colliders
        Collider[] hitColliders = Physics.OverlapBox(
            checkPosition,
            boxSize / 2,
            rotation,
            ~LayerMask.GetMask("Ground"), // Adjust the layer mask as needed
            QueryTriggerInteraction.Ignore
        );

        // Iterate through the colliders to check for collisions with other objects
        foreach (Collider hitCollider in hitColliders)
        {
            // Ignore self-collision (colliders attached to this game object)
            if (hitCollider.gameObject != gameObject && !hitCollider.isTrigger)
            {
                // Collision detected with another object
                return true;
            }
        }
    }

    // No collision detected for any collider
    return false;
}

    
    // Mask or unmask the pick-up action based on collision status
    // Mask or unmask the pick-up action based on collision status
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
{
    Debug.Log("Step Count: " + Academy.Instance.StepCount);
    Debug.Log(Academy.Instance.EnvironmentParameters);
    Debug.Log("Total Step Count: " + Academy.Instance.TotalStepCount);
    Debug.Log("Max Step: " + MaxStep);
    Debug.Log("Immobility Value: " + castleArea.maleImmobility);

    // Our action space indices:
    // 0: Do Nothing
    // 1: Move Forward
    // 2: Move Backward
    // 3: Rotate Left
    // 4: Rotate Right
    // 5: Pick Up
    // 6: Drop
    // 7: Destroy
    // 8: Use Stairs
    // 9: column_mini
    // 10: floor
    // 11: floor_stairs
    // 12: wallPaint_half_mini
    // 13: wallPaint_flat variant

    int totalActions = 14; // indices 0 to 13

    // Enable all actions by default
    for (int i = 0; i < totalActions; i++)
    {
        actionMask.SetActionEnabled(0, i, true);
    }

    // If immobile, only allow "Do Nothing" (index 0)
    if (castleArea.maleImmobility >= 1.0f)
    {
        for (int i = 1; i < totalActions; i++)
        {
            actionMask.SetActionEnabled(0, i, false);
        }
        return;
    }

    // Check movement collisions
    // Forward (1)
    if (WouldCollide(transform.forward * moveSpeed))
    {
        actionMask.SetActionEnabled(0, 1, false);
    }
    // Backward (2)
    if (WouldCollide(-transform.forward * moveSpeed))
    {
        actionMask.SetActionEnabled(0, 2, false);
    }

    // Pick Up (5): only if canPickUp and not holding something
    // If the agent cannot pick up or is already holding something, mask the pickup action
    if (!canPickUp || pickedUpObject != null)
    {
        actionMask.SetActionEnabled(0, 5, false); // Assuming 5 is the index for pickup
    }
    else
    {
        actionMask.SetActionEnabled(0, 5, true);
    }

    // Drop (6): only if holding something and can place it
    if (pickedUpObject == null)
    {
        actionMask.SetActionEnabled(0, 6, false);
    }
    else
    {
        Collider objectCollider = pickedUpObject.GetComponent<Collider>();
        if (objectCollider != null)
        {
            Vector3 halfExtents = objectCollider.bounds.extents;
            Vector3 dropPosition = transform.position + transform.forward * (halfExtents.z + 0.5f);
            if (Physics.CheckBox(dropPosition, halfExtents, Quaternion.identity, LayerMask.GetMask("Pickable")))
            {
                actionMask.SetActionEnabled(0, 6, false);
            }
        }
    }

    // Destroy (7): only if objectToDestroy is not null
    if (objectToDestroy == null)
    {
        actionMask.SetActionEnabled(0, 7, false);
    }
    else
    {
        actionMask.SetActionEnabled(0, 7, true);
    }

    // Use Stairs (8): only if CheckForStairs()
    if (!CheckForStairs())
    {
        actionMask.SetActionEnabled(0, 8, false);
    }

    // Disable always the 'Use Stairs' after checking? If desired:
    // actionMask.SetActionEnabled(0, 8, false); // If you want to always disable as previous code did.

    // Building actions (9 to 13): only if we can build
    bool canBuild = CastleArea.CheckSubtractBricks(CastleArea.brickCostPerObject) && CastleArea.BricksTimeFunction();
    if (!canBuild)
    {
        // Disable all builds
        for (int i = 9; i <= 13; i++)
        {
            actionMask.SetActionEnabled(0, i, false);
        }
    }
    else
    {
        // Special flooring logic:
        bool floorBeneath = CheckIfFloorBeneath();

        // If floor beneath, we might need to disable certain build actions.
        // Original logic: If floor beneath, can't place 'floor_stairs' variant or must disable one of them.
        // For simplicity, let's follow original logic as closely as possible:
        // The original code masked floor or floor_stairs based on floor presence:
        // If floor beneath, mask floor_stairs (index 11)
        // If no floor beneath, mask floor (index 10)
        if (floorBeneath)
        {
            actionMask.SetActionEnabled(0, 11, false); // floor_stairs variant disabled if floor beneath
        }
        else
        {
            actionMask.SetActionEnabled(0, 10, false); // floor variant disabled if no floor beneath
        }
    }
}

    
    
    // Handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is in the "Pickable" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pickable"))
        {
            // If the agent is not currently holding an object, it can pick up this object
            if (pickedUpObject == null)
            {
                canPickUp = true;
            }

            // This object can be considered for destruction as we are in contact with it
            objectToDestroy = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // If we exit collision with a pickable object
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pickable"))
        {
            // If we are leaving the object we could destroy, reset it
            if (objectToDestroy == collision.gameObject)
            {
                objectToDestroy = null;
            }

            // If we are not currently holding any object, we cannot pick up anything now since we're not in contact
            if (pickedUpObject == null)
            {
                canPickUp = false;
            }
        }
    }


// Function to pick up the object
    // Function to pick up the object
    private void PickUpObject()
{
    // Only pick up if currently allowed (in contact with an object) and not holding anything
    if (pickedUpObject == null && canPickUp)
    {
        // Find a pickable object within the agent's collider range
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Pickable"));
        if (colliders.Length > 0)
        {
            pickedUpObject = colliders[0].gameObject;

            // Disable physics on the picked up object
            Rigidbody rbPickable = pickedUpObject.GetComponent<Rigidbody>();
            if (rbPickable != null)
            {
                rbPickable.isKinematic = true;
                rbPickable.useGravity = false;
            }

            Collider objectCollider = pickedUpObject.GetComponent<Collider>();
            if (objectCollider != null)
            {
                objectCollider.enabled = false; // Disable the collider so it doesn't interfere
            }

            // Attach the object to the agent
            pickedUpObject.transform.SetParent(this.transform);
            pickedUpObject.transform.localPosition = new Vector3(0, 1, 1);
            pickedUpObject.transform.localRotation = Quaternion.identity;
            pickedUpObject.transform.localScale = Vector3.one;

            // Once the agent has picked something up, it can no longer pick another immediately
            canPickUp = false;
        }
        else
        {
            // If no overlapping pickable objects are found, it can't pick up
            canPickUp = false;
        }
    }
}

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

                // After dropping an object, the agent must collide with another object before picking up again
                canPickUp = false;
            }
            else
            {
                Debug.Log("Not enough space to drop the object safely!");
            }
        }
    }
}

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
    // public void CreateColumnProp()
    // {
    //     // Define the distance in front of the agent where the object will be placed
    //     float distanceInFront = 2.0f; // Adjust this value as needed
    //
    //     // Calculate the position in front of the agent
    //     Vector3 position = transform.position + transform.forward * distanceInFront;
    //
    //     // Keep the rotation the same as the agent's rotation
    //     Quaternion rotation = transform.rotation;
    //
    //     // Create a 'floor' prop at the calculated position and rotation
    //     castleArea.CreateProp("column_mini", position, rotation);
    //     
    //     CastleArea.SubtractBricks(CastleArea.brickCostPerObject);
    //     // Debug.Log($"Take: {CastleArea.numBricks}");
    //
    //     // m_AgentGroup.AddGroupReward(0.001f);
    //     castleArea.m_Team0AgentGroup.AddGroupReward(0.0001f);
    // }
    
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
        GameObject newProp = CreateProp("column_mini", position, rotation);
        newProp.transform.position = position; // Adjust as needed
        newProp.transform.localRotation = rotation; // Keeps the object aligned with the agent


        if (newProp != null)
        {
            newProp.tag = "Column";
            // **Set the layer of the new prop to 'Pickable'**
            newProp.layer = LayerMask.NameToLayer("Pickable");
        }
        // CreateProp("column_mini", position, rotation);
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
        // Create a 'floor' prop at the calculated position and rotation
        GameObject newProp = CreateProp("column_mini", position, rotation);

        if (newProp != null)
        {
            newProp.tag = "Floor";
            // **Set the layer of the new prop to 'Pickable'**
        }
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
        // Create a 'floor' prop at the calculated position and rotation
        GameObject newProp = CreateProp("floor_stairs Variant", position, rotation);

        if (newProp != null)
        {
            newProp.tag = "Stairs";
            // **Set the layer of the new prop to 'Pickable'**
        }
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
        GameObject newProp = CreateProp("wallPaint_half_mini", position, rotation);

        if (newProp != null)
        {
            newProp.tag = "Wall";
            // **Set the layer of the new prop to 'Pickable'**
            newProp.layer = LayerMask.NameToLayer("Pickable");
        }
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
        GameObject newProp = CreateProp("wallPaint_flat Variant", position, rotation);

        if (newProp != null)
        {
            newProp.tag = "Arch";
            // **Set the layer of the new prop to 'Pickable'**
            newProp.layer = LayerMask.NameToLayer("Pickable");
        }
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
    
    private bool CheckForStairs()
    {
        float checkRadius = 1.0f; // Adjust as needed
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Stairs"))
            {
                stairsObject = hitCollider.gameObject;
                return true;
            }
        }
        stairsObject = null;
        return false;
    }

    private void UseStairs()
    {
        if (stairsObject != null)
        {
            // Get the height of the stairs object
            Collider stairsCollider = stairsObject.GetComponent<Collider>();
            if (stairsCollider != null)
            {
                Bounds stairsBounds = stairsCollider.bounds;
                float stairsHeight = stairsBounds.size.y;

                // Calculate the new position at the top of the stairs
                Vector3 newPosition = stairsObject.transform.position;
                newPosition.y += stairsHeight;

                // Optional: Adjust position to be slightly forward on the stairs
                newPosition += stairsObject.transform.forward * 0.5f; // Adjust as needed

                // Move the agent to the new position
                rb.MovePosition(newPosition);

                // Optionally, adjust agent's rotation to match the stairs' rotation
                rb.MoveRotation(stairsObject.transform.rotation);

                // Clear the stairsObject reference
                stairsObject = null;
            }
        }
    }




    public override void OnActionReceived(ActionBuffers actions)
    {
        if (castleArea.maleImmobility >= 1.0f && castleArea.m_ResetTimer % castleArea.MaxEnvSteps <= 400)
        {
            Debug.Log($"MaleAgent is immobile and training is paused at StepCount: {StepCount}");
            return; // Prevents training for the first 400 steps
        }

        // Get the chosen action from the single branch
        int chosenAction = actions.DiscreteActions[0];

        // Define movement and rotation step sizes
        float moveStep = moveSpeed * Time.deltaTime;
        float rotationStep = rotateSpeed * Time.deltaTime;

        bool noBuildAction = false;

        switch (chosenAction)
        {
            case 0:
                // Do Nothing
                // Add penalty for nothing action if desired
                castleArea.m_Team1AgentGroup.AddGroupReward(penaltyForNothingAction);
                break;

            case 1:
                // Move Forward
                if (rb != null)
                {
                    Vector3 forwardMove = transform.forward * moveStep;
                    rb.MovePosition(transform.position + forwardMove);
                }

                break;

            case 2:
                // Move Backward
                if (rb != null)
                {
                    Vector3 backwardMove = -transform.forward * moveStep;
                    rb.MovePosition(transform.position + backwardMove);
                }

                break;

            case 3:
                // Rotate Left
                if (rb != null)
                {
                    float rotationAngleLeft = -rotationStep * rotationStep;
                    Quaternion leftRotation = rb.rotation * Quaternion.Euler(0f, rotationAngleLeft, 0f);
                    leftRotation = Quaternion.Euler(0f, leftRotation.eulerAngles.y, 0f);
                    rb.MoveRotation(leftRotation);
                }

                break;

            case 4:
                // Rotate Right
                if (rb != null)
                {
                    float rotationAngleRight = rotationStep * rotationStep;
                    Quaternion rightRotation = rb.rotation * Quaternion.Euler(0f, rotationAngleRight, 0f);
                    rightRotation = Quaternion.Euler(0f, rightRotation.eulerAngles.y, 0f);
                    rb.MoveRotation(rightRotation);
                }

                break;

            case 5:
                // Pick Up
                if (canPickUp)
                {
                    PickUpObject();
                }

                noBuildAction = true;
                Debug.Log($"Action: PICKUP. Currently holding: {(pickedUpObject == null ? "None" : pickedUpObject.name)}");

                break;

            case 6:
                // Drop
                if (pickedUpObject != null)
                {
                    DropObject();
                }

                noBuildAction = true;
                break;

            case 7:
                // Destroy
                DestroyObject();
                noBuildAction = true;
                break;

            case 8:
                // Use Stairs
                UseStairs();
                noBuildAction = true;
                break;
            // Build actions
            case 9:
                //column_mini
                CreatePropAction(1);
                break;
            case 10:
                // floor
                CreatePropAction(2);
                break;
            case 11:
                //floor_stairs Variant
                CreatePropAction(3);
                break;
            case 12:
                //wallPaint_half_mini
                CreatePropAction(4);
                break;
            case 13:
                // wallPaint_flat Variant
                CreatePropAction(5);
                break;

            default:
                Debug.LogError("Invalid prop action");
                break;
        }
        Debug.Assert(transform.childCount <= 2, $"Agent has more than one picked up object {transform.childCount}!");

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
    
    public float CalculateEnclosureReward(Vector3 point)
    {
        int hitCount = 0;

        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate the direction of the ray
            float angle = i * (360f / numberOfRays);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // Perform the raycast
            Ray ray = new Ray(point, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                // If we hit something, draw the ray only up to the hit point
                Debug.DrawRay(point, direction * hit.distance, Color.red, 0.1f);
    
                // Check if it's a wall
                if (hit.collider.CompareTag(wallTag))
                {
                    hitCount++;
                }
            }
            else
            {
                // If we didn't hit anything, draw the full length
                Debug.DrawRay(point, direction * rayDistance, Color.red, 0.1f);
            }
        }

        // Calculate the proportion of rays that hit walls
        float hitProportion = (float)hitCount / numberOfRays;

        // Calculate the reward based on the hit proportion
        float reward = Mathf.Clamp01(hitProportion / enclosureThreshold)/10000;

        return reward;
    }
}
