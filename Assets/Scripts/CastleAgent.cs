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
    [SerializeField] private Transform targetTransform;
    [SerializeField] private List<GameObject> spawnedTargetList = new List<GameObject>();
    public int targetCount;
    public GameObject food;
    private float RAY_RANGE = 5f;
    
    // agent variables
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    
    // env variables
    [SerializeField] private Transform envLocation;
    Material envMaterial;
    public GameObject env;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }
    

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));

        CreateTarget();
    }

    private void CreateTarget()
    {
        if (spawnedTargetList.Count != 0)
        {
            removeTarget(spawnedTargetList);
        }
        
        for (int i = 0; i < targetCount; i++)
        {
            int counter = 0;
            bool distanceGood;
            bool alreadyDecremented = false;
            //  spawning object target
            GameObject newTarget = Instantiate(food);
            // make target child of env 
            newTarget.transform.parent = envLocation;
            // give random spawn location
            // float x = r.Next(0, 5);
            // float z = r.Next(0, 5);
            Vector3 targetLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
            // check if distance is good
            if (spawnedTargetList.Count != 0)
            {
                for (int k = 0; k < spawnedTargetList.Count; k++)
                {
                    if (counter < 10)
                    {
                        distanceGood = CheckOverlap(targetLocation, spawnedTargetList[k].transform.localPosition, RAY_RANGE);
                        if (distanceGood == false)
                        {
                            targetLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
                            k--;
                            alreadyDecremented = true;

                        }
                        
                        distanceGood = CheckOverlap(targetLocation, transform.localPosition, RAY_RANGE);
                        if (distanceGood == false)
                        {
                            targetLocation = new Vector3(UnityEngine.Random.Range(-7f, 7f), 0, UnityEngine.Random.Range(-7f, 7f));
                            if (alreadyDecremented == false)
                            {
                                k--;
                            }

                        }

                        counter++;
                    }
                    else
                    {
                        // exit the loop
                        k = spawnedTargetList.Count;
                    }
                }
            }
            
            // spawn in new location
            newTarget.transform.localPosition = targetLocation;
            spawnedTargetList.Add(newTarget);
        } 
    }

    private void removeTarget(List<GameObject> targetsToDelete)
    {
        
        foreach (var target in targetsToDelete)
        {
            Debug.Log("before DEBUG");
            Destroy(target.gameObject);
            Debug.Log("afyter DEBUG");
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
        if (other.gameObject.tag == "Sphere")
        {
            
            spawnedTargetList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);
            
            
            // Debug.Log("FOUND TARGET");
            Debug.Log(GetCumulativeReward());
            if (spawnedTargetList.Count == 0)
            {
                envMaterial.color = Color.green;
                AddReward(5f);
                removeTarget(spawnedTargetList);
                EndEpisode();
            }
            
        }
        
        if (other.gameObject.tag =="Wall")
        {
            envMaterial.color = Color.red;
            removeTarget(spawnedTargetList);
            AddReward(-10f);
            // Debug.Log("hit wall");
            Debug.Log(GetCumulativeReward());
            EndEpisode();
            
        }
        
    }
}
