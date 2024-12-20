using Unity.MLAgents;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CastleArea : MonoBehaviour
{
    [Header("Area Objects")]
    // public GameObject env;
    public GameObject CastleAgent;
    public GameObject MaleAgent;
    public GameObject ground;
    public Material successMaterial;
    public Material failureMaterial;
    public TextMeshPro scoreText;
    public int m_ResetTimer;
    public int MaxEnvSteps = 1000;
    private int MaxTraining = 10000;
    private float rangeZ = 8;
    private float rangeX = 29;
    
    [Header("Prefabs")]
    public GameObject agentPrefab;
    public GameObject wallPrefab;

    // [HideInInspector]
    public int targetCountF = 2;
    public int targetCountM = 2;
    public float maleImmobility = 1.0f;
    
    public static float numBricks = 10f;
    public static float maxBricks = 0f;
    public const float brickCostPerObject = 1f;
    [HideInInspector]
    public static int numAgents;
    [HideInInspector]
    public float spawnRange = 20;
    
    private List<GameObject> spawnedWalls;
    
    [Serializable]
    public class FemaleInfo
    {
        public CastleAgent Agent;
        [HideInInspector]
        public Vector3 startingPos;
        [HideInInspector]
        public Quaternion startingRot;
        [HideInInspector]
        public Rigidbody rb;
        [HideInInspector]
        public Collider boxCollider;
        [HideInInspector]
        public int TeamID;
    }
    
    [Serializable]
    public class MaleInfo
    {
        public MaleAgentV2 Agent;
        [HideInInspector]
        public Vector3 startingPos;
        [HideInInspector]
        public Quaternion startingRot;
        [HideInInspector]
        public Rigidbody rb;
        [HideInInspector]
        public Collider boxCollider;
        [HideInInspector]
        public int TeamID;
    }
    
    public SimpleMultiAgentGroup m_Team0AgentGroup;
    public SimpleMultiAgentGroup m_Team1AgentGroup;
    public List<FemaleInfo> Team0Players;
    public List<MaleInfo> Team1Players;
    [SerializeField] public List<GameObject> spawnedTargetListF = new List<GameObject>();
    [SerializeField] public List<GameObject> spawnedTargetListM = new List<GameObject>();
    private string[] propNames = { "floor", "floorStairs", "column_mini", "Sphere", "SphereF", "SphereM" }; // List of props

    // A list of (position, radius) tuples of occupied spots in the area
    private List<Tuple<Vector3, float>> occupiedPositions;

    private Renderer groundRenderer;
    private Material groundMaterial;
    private bool m_Initialized;
    
    void Start()
    {
        Initialize();
        Academy.Instance.OnEnvironmentReset += UpdateEnvironmentParameters;

    }
    
    private void UpdateEnvironmentParameters()
    {
        // Get the current value of 'targetCount' from the Environment Parameters
        // targetCountF = Mathf.RoundToInt(Academy.Instance.EnvironmentParameters.GetWithDefault("targetCountF", 2));
        // targetCountM = Mathf.RoundToInt(Academy.Instance.EnvironmentParameters.GetWithDefault("targetCountM", 2));
    }
    
    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        Academy.Instance.OnEnvironmentReset -= UpdateEnvironmentParameters;
    }
    
    private void Initialize()
    {
        // Get the ground renderer so we can change the material when a goal is scored
        groundRenderer = ground.GetComponent<Renderer>();

        // Store the starting material
        groundMaterial = groundRenderer.material;
        
        m_Team0AgentGroup = new SimpleMultiAgentGroup();
        m_Team1AgentGroup = new SimpleMultiAgentGroup();
        //INITIALIZE AGENTS
        foreach (var item in Team0Players)
        {
            item.Agent.Initialize();
            item.Agent.teamID = 0;
            item.TeamID = 0;
            m_Team0AgentGroup.RegisterAgent(item.Agent);
        }
        foreach (var item in Team1Players)
        {
            item.Agent.Initialize();
            item.Agent.teamID = 1;
            item.TeamID = 1;
            m_Team1AgentGroup.RegisterAgent(item.Agent);
        }
        m_Initialized = true;
        ResetArea();
    }

    /// <summary>
    /// Resets the area
    /// </summary>
    /// <param name="agents"></param>
    public void ResetArea()
    {
        
        occupiedPositions = new List<Tuple<Vector3, float>>();
        ResetAgents();
        // ResetResources();
        if (spawnedTargetListM.Count != 0)
        {
            removeTarget(spawnedTargetListM);
        }
        
        if (spawnedTargetListF.Count != 0)
        {
            removeTarget(spawnedTargetListF);
        }
        CreateFoodPropFemale(transform);
        CreateFoodPropMale(transform);
        

        // ResetWalls();
    }

    public void ResetResources()
    {
        numBricks = 0;
        maxBricks = 0;
        // m_ResetTimer = 0;
        // remove all agent-built prefabs as well

    }
    
    void FixedUpdate()
    {
        if (!m_Initialized) return;
        //RESET SCENE IF WE MaxEnvironmentSteps
        m_ResetTimer += 1;
        Debug.Log($"m_ResetTimer {m_ResetTimer}, MaxEnvSteps {MaxEnvSteps}, % {m_ResetTimer % MaxEnvSteps}");
        
        if (m_ResetTimer % MaxEnvSteps >= 400)
        {
            maleImmobility = 0.0f;
            targetCountM = 4;
            targetCountF = 4;
            CreateFoodPropMale(transform);
            CreateFoodPropFemale(transform);
        }
        else
        {
            maleImmobility = 1.0f;
            targetCountM = 0;
            targetCountF = 6;
            CreateFoodPropMale(transform);
            CreateFoodPropFemale(transform);
        }
        
        if (m_ResetTimer % MaxEnvSteps == 0)
        {
            m_Team0AgentGroup.EndGroupEpisode();
            m_Team1AgentGroup.EndGroupEpisode();
            ResetArea();
        }
        
        if (m_ResetTimer % MaxTraining == 0)
        {
            int stage = m_ResetTimer / MaxTraining;
            SaveAsPrefab(stage);
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!m_Initialized)
        {
            Initialize();
        }
    }

    // private void FixedUpdate()
    // {
    //     // Make sure the pig has not left the area
    //     // Vector3 agentLocalPosition = CastleAgent.transform.localPosition;
    //     // if (Mathf.Abs(agentLocalPosition.x) > 13f || Mathf.Abs(agentLocalPosition.z) > 13f)
    //     // {
    //     //     Debug.LogWarning("Agent out of the pen!");
    //     //     CastleAgent castleAgentComponent = CastleAgent.GetComponent<CastleAgent>();
    //     //     castleAgentComponent.SetReward(-5f);
    //     //     ResetArea();
    //     // }
    // }
    
    public List<GameObject> GetBricksObjects()
    {
        return spawnedWalls;
    }
    
    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    public IEnumerator SwapGroundMaterial(bool success)
    {
        if (success)
        {
            groundRenderer.material = successMaterial;
        }
        else
        {
            groundRenderer.material = failureMaterial;
        }

        yield return new WaitForSeconds(0.5f);
        groundRenderer.material = groundMaterial;
    }

    public static bool BricksTimeFunction()
    {
        if ( maxBricks < 10000f)
        {
            numBricks += 0.0001f;
            maxBricks += 0.0001f;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void AddBricks(float amount)
    {
        numBricks += amount;
    }
    
    public static bool CheckSubtractBricks(float amount)
    {
        if (numBricks >= amount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public static void SubtractBricks(float amount)
    {
            numBricks -= amount;
    }
    
    public void UpdateScore(float score)
    {
        scoreText.text = score.ToString("0.00");
    }
    
    /// <summary>
    /// Reset the agent
    /// </summary>
    public void ResetAgent(GameObject agent)
    {
        // Reset location and rotation
        RandomlyPlaceObject(agent, rangeX, rangeZ, 10, transform);
    }

    /// <summary>
    /// Resets all walls in the area
    /// </summary>
    private void ResetWalls()
    {
        if (spawnedWalls != null)
        {
            // Destroy any walls remaining from the previous run
            foreach (GameObject spawnedWall in spawnedWalls.ToArray())
            {
                Destroy(spawnedWall);
            }
        }

        spawnedWalls = new List<GameObject>();

        for (int i = 0; i < numBricks; i++)
        {
            // Create a new wall instance and place it randomly
            GameObject wallInstance = Instantiate(wallPrefab, transform);
            RandomlyPlaceObject(wallInstance,  rangeX, rangeZ, 10, transform);
            spawnedWalls.Add(wallInstance);
        }
    }
    
    /// <summary>
    /// Resets all stumps in the area
    /// </summary>
    private void ResetAgents()
    {
        foreach (var item in Team0Players)
        {
            item.Agent.gameObject.SetActive(true);
            ResetAgent(item.Agent.gameObject);
            m_Team0AgentGroup.RegisterAgent(item.Agent);
        }
        foreach (var item in Team1Players)
        {
            item.Agent.gameObject.SetActive(true);
            ResetAgent(item.Agent.gameObject);
            m_Team1AgentGroup.RegisterAgent(item.Agent);
        }

    }
    
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

        if (propPrefab != null)
        {
            // Instantiate the prefab in the scene at the origin
            GameObject newProp = Instantiate(propPrefab, transform, false);
            // Set the layer of the new prop to 
            if (propName != "SphereF" && propName != "SphereM")
            {
                newProp.layer = LayerMask.NameToLayer("Pickable");   
            }
            

            // Return the newly created GameObject
            return newProp;
        }
        else
        {
            Debug.LogError($"Could not find the prop '{propName}' in the Props folder.");
            return null;
        }
        

    }
    
    private void removeTarget(List<GameObject> targetsToDelete)
    {
        
        foreach (var target in targetsToDelete)
        {
            Destroy(target.gameObject);
        }
        targetsToDelete.Clear();
    }
    
    public void CreateFoodPropMale(Transform envLocation)
    {
        // if (spawnedTargetListM.Count != 0)
        // {
        //     removeTarget(spawnedTargetListM);
        // }
        int numberOfNewTargetsNeeded = targetCountM - spawnedTargetListM.Count;
        Debug.Log($"numberOfNewTargetsNeeded MALE: {numberOfNewTargetsNeeded} and {spawnedTargetListM.Count}");
        for (int i = 0 ; i < numberOfNewTargetsNeeded; i++)
        {
            // Debug.Log($"Create ball M no: {i} to {targetCount}");
            Vector3 randomLocalPosition =
                new Vector3(UnityEngine.Random.Range(-20, 20), 0, UnityEngine.Random.Range(-7, 7));
            // Keep the rotation the same as the agent's rotation
            Quaternion rotation = transform.localRotation;

            // Create a 'floor' prop at the calculated position and rotation
            Debug.Log("Creating Food Prop Male");
            GameObject newTarget = CreateProp("SphereM", randomLocalPosition, rotation);
            RandomlyPlaceObject(newTarget,  rangeX, rangeZ, 20, transform);
            // newTarget.transform.SetParent(envLocation, false);
            // Debug.Log($"envlocation: {envLocation.gameObject.name}");
            // // newTarget.transform.parent = envLocation;
            // newTarget.transform.localPosition.Scale(transform.localScale);
            spawnedTargetListM.Add(newTarget);
            // Debug.Log("Created sphere");
        }

    }

    
    public void CreateFoodPropFemale(Transform envLocation)
    {
        // if (spawnedTargetListF.Count != 0)
        // {
        //     removeTarget(spawnedTargetListF);
        // }
        // Debug.Log($"Name : {envLocation.gameObject.name}");
        int numberOfNewTargetsNeeded = targetCountF - spawnedTargetListF.Count;
        Debug.Log($"numberOfNewTargetsNeeded FEMALE: {numberOfNewTargetsNeeded}");
        for (int i = 0 ; i < numberOfNewTargetsNeeded; i++)
        {
            // Debug.Log($"Create ball F no: {i} to {targetCount}");
            Vector3 randomLocalPosition =
                new Vector3(UnityEngine.Random.Range(-rangeX, rangeX), 0, UnityEngine.Random.Range(-rangeZ, rangeZ));
            // Keep the rotation the same as the agent's rotation
            Quaternion rotation = transform.localRotation;

            // Create a 'floor' prop at the calculated position and rotation
            Debug.Log("Create food prop female");
            GameObject newTarget = CreateProp("SphereF", randomLocalPosition, rotation);
            RandomlyPlaceObject(newTarget,  rangeX, rangeZ, 20, transform);
            // newTarget.transform.SetParent(envLocation, false);
            // newTarget.transform.localPosition.Scale(transform.localScale);
            spawnedTargetListF.Add(newTarget);
            // Debug.Log("Created sphere");
        }

    }
    
        // spawnedAgents = new List<GameObject>();
        //
        // for (int i = 0; i < numAgents; i++)
        // {
        //     // Create a new stump instance and place it randomly
        //     GameObject agentInstance = Instantiate(agentPrefab, transform);
        //     RandomlyPlaceObject(agentInstance, spawnRange, 50);
        //     spawnedAgents.Add(agentInstance);
        // }
    // }
    
        /// <summary>
    /// Attempts to randomly place an object by checking a sphere around a potential location for collisions
    /// </summary>
    /// <param name="objectToPlace">The object to be randomly placed</param>
    /// <param name="range">The range in x and z to choose random points within.</param>
    /// <param name="maxAttempts">Number of times to attempt placement</param>
    public void RandomlyPlaceObject(GameObject objectToPlace, float rangeX, float rangeZ, int maxAttempts, Transform envTransform)
    {
        // Ensure the object has a collider
        Collider objCollider = objectToPlace.GetComponent<Collider>();
        if (objCollider == null)
        {
            Debug.LogError($"Object {objectToPlace.name} does not have a Collider component.");
            return;
        }

        // Temporarily disable collision on the object to place
        bool colliderWasEnabled = objCollider.enabled;
        objCollider.enabled = false;

        // Collision detection varies depending on the type of the object 
         
        // Spherical objects: Calculate test radius 10% larger than the collider extents
        // Small buffer to ensure no overlap
        float buffer = 0.5f;
        float testRadius = GetColliderRadius(objectToPlace) * 1.1f + buffer;
        
        // Rectangular objects: Calculate half of the collider size in each dimension 
        Vector3 halfSize = objectToPlace.gameObject.transform.localScale / 2;
        
        // Set a random rotation (local rotation relative to Env)
        objectToPlace.transform.localRotation = Quaternion.Euler(
            new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f)
        );

        bool placedSuccessfully = false;

        // Make several attempts at randomly placing the object
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            // Generate random local position within the range
            Vector3 randomLocalPosition = new Vector3(
                UnityEngine.Random.Range(-rangeX, rangeX),
                0f,
                UnityEngine.Random.Range(-rangeZ, rangeZ) // Using 'range' since 'rangeZ' was undefined
            );

            // Check if the position is open (relative to Env's local position)
            if (CheckIfPositionIsOpen(randomLocalPosition, testRadius, halfSize, envTransform, objectToPlace))
            {
                // Set the object's local position relative to the Env
                objectToPlace.transform.localPosition = randomLocalPosition;

                // Record the occupied position (world position)
                Vector3 worldPosition = envTransform.TransformPoint(randomLocalPosition);
                occupiedPositions.Add(new Tuple<Vector3, float>(worldPosition, testRadius));

                placedSuccessfully = true;
                Debug.Log($"Placed {objectToPlace.name} at {worldPosition}");
                break;
            }
            else if (attempt == maxAttempts)
            {
                Debug.LogError($"{objectToPlace.name} couldn't be placed randomly after {maxAttempts} attempts.");
            }
        }

        // Re-enable collision
        objCollider.enabled = colliderWasEnabled;

        if (placedSuccessfully)
        {
            // Optionally, perform additional setup after successful placement
        }
        else
        {
            objectToPlace.GetComponent<Renderer>().material.color = Color.red;
        }
    }



    // public void RandomlyPlaceObject(GameObject objectToPlace, float range, float maxAttempts, Transform envLocation)
    // {
    //     // Temporarily disable collision
    //     objectToPlace.GetComponent<Collider>().enabled = false;
    //
    //     // Calculate test radius 10% larger than the collider extents
    //     float testRadius = GetColliderRadius(objectToPlace) * 1.1f;
    //
    //     // Set a random rotation
    //     objectToPlace.transform.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f));
    //
    //     // Make several attempts at randomly placing the object
    //     int attempt = 1;
    //     while (attempt <= maxAttempts)
    //     {
    //         Vector3 randomLocalPosition = new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-rangeZ, rangeZ));
    //         // randomLocalPosition.Scale(transform.localScale);
    //
    //         //if (!Physics.CheckSphere(transform.position + randomLocalPosition, testRadius, notGroundLayerMask))
    //         if (CheckIfPositionIsOpen(transform.localPosition + randomLocalPosition, testRadius))
    //         {
    //             objectToPlace.transform.localPosition = randomLocalPosition;
    //             occupiedPositions.Add(new Tuple<Vector3, float>(objectToPlace.transform.localPosition, testRadius));
    //             Debug.Log($"occupiedPosition: {transform.localPosition + randomLocalPosition}");
    //             break;
    //         }
    //         else if (attempt == maxAttempts)
    //         {
    //             Debug.LogError(string.Format("{0} couldn't be placed randomly after {1} attempts.", objectToPlace.name, maxAttempts));
    //             break;
    //         }
    //
    //         attempt++;
    //     }
    //
    //     // Enable collision
    //     objectToPlace.GetComponent<Collider>().enabled = true;
    // }
        
    /// <summary>
    /// Gets a local space radius that draws a circle on the X-Z plane around the boundary of the collider
    /// </summary>
    /// <param name="obj">The game object to test</param>
    /// <returns>The local space radius around the collider</returns>
    private static float GetColliderRadius(GameObject obj)
    {
        // Collider col = obj.GetComponent<Collider>();
        //
        // Vector3 boundsSize = Vector3.zero; 
        // if (col.GetType() == typeof(MeshCollider))
        // {
        //     boundsSize = ((MeshCollider)col).sharedMesh.bounds.size;
        // }
        // else if (col.GetType() == typeof(BoxCollider))
        // {
        //     boundsSize = col.bounds.size;
        // }
        //
        // boundsSize.Scale(obj.transform.localScale);
        // return Mathf.Max(boundsSize.x, boundsSize.z) / 2f;
        // NEW COPIED FUNCTION
        
        Collider col = obj.GetComponent<Collider>();

        if (col is SphereCollider sphereCollider)
        {
            // Account for lossy scale to calculate world-space radius
            return sphereCollider.radius * Mathf.Max(
                obj.transform.lossyScale.x,
                obj.transform.lossyScale.y,
                obj.transform.lossyScale.z
            );
        }
        else if (col is BoxCollider boxCollider)
        {
            // Approximate radius as the maximum half-extent
            Vector3 halfExtents = boxCollider.size / 2;
            halfExtents.Scale(obj.transform.lossyScale);
            return Mathf.Max(halfExtents.x, halfExtents.z);
        }
        else if (col is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            Vector3 boundsSize = meshCollider.sharedMesh.bounds.size;
            boundsSize.Scale(obj.transform.lossyScale);
            return Mathf.Max(boundsSize.x, boundsSize.z) / 2f;
        }

        Debug.LogError("Unsupported collider type!");
        return 0f;
    }
    
    /// <summary>
    /// Detects if a test position has a radius of clear space around it
    /// </summary>
    /// <param name="testPosition">The world position to test</param>
    /// <param name="testRadius">The radius to test</param>
    /// <returns><c>true</c> if the position is open</returns>
    // private bool CheckIfPositionIsOpen(Vector3 testPosition, float testRadius)
    // {
    //     foreach (Tuple<Vector3, float> occupied in occupiedPositions)
    //     {
    //         Vector3 occupiedPosition = occupied.Item1;
    //         float occupiedRadius = occupied.Item2;
    //         Debug.Log($"new location: {testPosition}, radius: {occupiedRadius} ");
    //         if (Vector3.Distance(testPosition, occupiedPosition) - occupiedRadius <= testRadius)
    //         {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }
    
    private bool CheckIfPositionIsOpen(Vector3 localPosition, float testRadius, Vector3 halfSize, Transform envTransform, GameObject objectToPlace)
    {
        // Convert the test local position to world position
        Vector3 testWorldPosition = envTransform.TransformPoint(localPosition);

        // Collision check depends on the shape of the object
        Collider[] colliders; 
        // Assume that SphereM and SphereF are the only Spherical objects that can be spawned
        String objectToPlaceTag = objectToPlace.tag;
        if (objectToPlaceTag.Equals("SphereM") || objectToPlaceTag.Equals("SphereF"))
        {
            Vector3 lossyScale = objectToPlace.transform.lossyScale;
            float worldRadius = testRadius * Mathf.Max(
                lossyScale.x,
                lossyScale.y,
                lossyScale.z
            );
            colliders = Physics.OverlapSphere(testWorldPosition, worldRadius); 
        }
        // Assume the only other object shapes are boxes 
        else
        {
            colliders = Physics.OverlapBox(testWorldPosition, halfSize);
        }

        foreach (var collider in colliders)
        {
            // Exclude the object being placed
            if (collider.gameObject == objectToPlace)
                continue;

            // Optionally, exclude triggers if necessary
            if (collider.isTrigger)
                continue;

            if (collider.CompareTag("Floor"))
                continue;

            // If any other collider is found, the position is not open
            return false;
        }

        // No overlapping colliders found
        return true;
    }


    
    public void SaveAsPrefab(int stage)
    {
        // Path where the prefab will be saved
        string prefabPath = $"Assets/Resources/EnvTrained_{stage}.prefab";
        // Create the prefab from the Env GameObject
        PrefabUtility.SaveAsPrefabAsset(this.gameObject, prefabPath);

        Debug.Log("Environment saved as a prefab at: " + prefabPath);
    }
    // public void LoadEnvironment(string filePath)
    //     {
    //         if (File.Exists(filePath))
    //         {
    //             string json = File.ReadAllText(filePath);
    //             TransformData transformData = JsonUtility.FromJson<TransformData>(json);
    //
    //             env.transform.position = transformData.position;
    //             env.transform.rotation = transformData.rotation;
    //             env.transform.localScale = transformData.scale;
    //
    //             Debug.Log("Environment loaded from: " + filePath);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("Save file not found: " + filePath);
    //         }
    
}
