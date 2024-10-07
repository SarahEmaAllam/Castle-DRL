using Unity.MLAgents;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CastleArea : MonoBehaviour
{
    [Header("Pig Area Objects")]
    public GameObject CastleAgent;
    public GameObject MaleAgent;
    public GameObject ground;
    public Material successMaterial;
    public Material failureMaterial;
    public TextMeshPro scoreText;
    private int m_ResetTimer;
    public int MaxEnvironmentSteps = 1000;
    
    [Header("Prefabs")]
    public GameObject agentPrefab;
    public GameObject wallPrefab;

    // [HideInInspector]
    public static float numBricks = 0f;
    public static float maxBricks = 0f;
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

    // A list of (position, radius) tuples of occupied spots in the area
    private List<Tuple<Vector3, float>> occupiedPositions;

    private Renderer groundRenderer;
    private Material groundMaterial;
    private bool m_Initialized;
    
    void Start()
    {
        Initialize();
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
        ResetResources();

        // ResetWalls();
    }

    public void ResetResources()
    {
        numBricks = 0;
        maxBricks = 0;
        m_ResetTimer = 0;
        // remove all agent-built prefabs as well

    }
    
    void FixedUpdate()
    {
        if (!m_Initialized) return;
        //RESET SCENE IF WE MaxEnvironmentSteps
        m_ResetTimer += 1;
        if (m_ResetTimer % 100 == 0)
        {
            m_Team0AgentGroup.EndGroupEpisode();
            m_Team1AgentGroup.EndGroupEpisode();
        }
        
        if (m_ResetTimer >= MaxEnvironmentSteps)
        {
            ResetArea();
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
            numBricks += 0.01f;
            maxBricks += 0.01f;
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
        RandomlyPlaceObject(agent, 20, 10);
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
            RandomlyPlaceObject(wallInstance, 20, 10);
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
    public void RandomlyPlaceObject(GameObject objectToPlace, float range, float maxAttempts)
    {
        // Temporarily disable collision
        objectToPlace.GetComponent<Collider>().enabled = false;

        // Calculate test radius 10% larger than the collider extents
        float testRadius = GetColliderRadius(objectToPlace) * 1.1f;

        // Set a random rotation
        objectToPlace.transform.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f));

        // Make several attempts at randomly placing the object
        int attempt = 1;
        while (attempt <= maxAttempts)
        {
            Vector3 randomLocalPosition = new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range));
            randomLocalPosition.Scale(transform.localScale);

            //if (!Physics.CheckSphere(transform.position + randomLocalPosition, testRadius, notGroundLayerMask))
            if (CheckIfPositionIsOpen(transform.position + randomLocalPosition, testRadius))
            {
                objectToPlace.transform.localPosition = randomLocalPosition;
                occupiedPositions.Add(new Tuple<Vector3, float>(objectToPlace.transform.position, testRadius));
                Debug.Log($"occupiedPosition: {transform.position + randomLocalPosition}");
                break;
            }
            else if (attempt == maxAttempts)
            {
                Debug.LogError(string.Format("{0} couldn't be placed randomly after {1} attempts.", objectToPlace.name, maxAttempts));
                break;
            }

            attempt++;
        }

        // Enable collision
        objectToPlace.GetComponent<Collider>().enabled = true;
    }
        
    /// <summary>
    /// Gets a local space radius that draws a circle on the X-Z plane around the boundary of the collider
    /// </summary>
    /// <param name="obj">The game object to test</param>
    /// <returns>The local space radius around the collider</returns>
    private static float GetColliderRadius(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();

        Vector3 boundsSize = Vector3.zero; 
        if (col.GetType() == typeof(MeshCollider))
        {
            boundsSize = ((MeshCollider)col).sharedMesh.bounds.size;
        }
        else if (col.GetType() == typeof(BoxCollider))
        {
            boundsSize = col.bounds.size;
        }

        boundsSize.Scale(obj.transform.localScale);
        return Mathf.Max(boundsSize.x, boundsSize.z) / 2f;
    }
    
    /// <summary>
    /// Detects if a test position has a radius of clear space around it
    /// </summary>
    /// <param name="testPosition">The world position to test</param>
    /// <param name="testRadius">The radius to test</param>
    /// <returns><c>true</c> if the position is open</returns>
    private bool CheckIfPositionIsOpen(Vector3 testPosition, float testRadius)
    {
        foreach (Tuple<Vector3, float> occupied in occupiedPositions)
        {
            Vector3 occupiedPosition = occupied.Item1;
            float occupiedRadius = occupied.Item2;
            Debug.Log($"new location: {testPosition}, radius: {occupiedRadius} ");
            if (Vector3.Distance(testPosition, occupiedPosition) - occupiedRadius <= testRadius)
            {
                return false;
            }
        }

        return true;
    }
    
}
