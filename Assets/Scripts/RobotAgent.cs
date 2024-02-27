using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
public class RobotAgent : Agent
{

    [SerializeField] private Transform goalPosition;
    [SerializeField] private Transform ballPosition;
    [SerializeField] private GameObject obstacleParent;
    [SerializeField] private Transform ground;
    [SerializeField] private int numberOfRays = 8;
    [SerializeField] private float raycastDistance = 1;
    [Range(0.0f, 1.0f)] public float randomness;
    [SerializeField] int numberOfObstacles= 0;
    private Dictionary<int, int> directionMapping = new Dictionary<int, int> { {1, -1}, { 0, 0}, { 2, 1} };
    public bool timePenalty;
    [SerializeField] private int maxTime;
    private int counter;
    Collider objectCollider;

    Vector3 previousPosition;
    private bool hasBall;

    public bool log;


    private void Start()
    {
        objectCollider = ground.GetComponent<MeshCollider>();
    }

    public override void OnEpisodeBegin()
    {
        //transform.localPosition = new Vector3(0f, 0.35f, 0f);
        ballPosition.transform.parent = transform.parent;
        ballPosition.transform.localPosition = new Vector3(Random.Range(-5f, 5f) * randomness , 0.3f, Random.Range(-5f, 5f) * randomness);
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0.3f, Random.Range(-5f, 5f));
        previousPosition = transform.localPosition;
        hasBall = false;
        //ballPosition.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //ballPosition.transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        //base.OnEpisodeBegin();
        counter = maxTime;
        DistributeObstacles();
    }

    void DistributeObstacles()
    {

        //clear children first
        foreach (Transform child in obstacleParent.transform)
        {
            Destroy(child.gameObject);
        }

        if (objectCollider == null)
        {
            Debug.LogError("Object needs a collider for obstacle distribution.");
            return;
        }

        for (int i = 0; i < numberOfObstacles; i++)
        {
            Vector3 randomPosition = GetRandomPointOnObject();
            Quaternion randomRotation = Random.rotation;

            GameObject cubeInstance = GameObject.CreatePrimitive(PrimitiveType.Cube); // Create a cube
            cubeInstance.transform.position = randomPosition;
            cubeInstance.transform.rotation = randomRotation;
            cubeInstance.tag = "obstacle";

            Rigidbody cubeRigidbody = cubeInstance.AddComponent<Rigidbody>();
            cubeRigidbody.isKinematic = true;

            cubeInstance.transform.parent = obstacleParent.transform;
        }
    }

    Vector3 GetRandomPointOnObject()
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(objectCollider.bounds.min.x, objectCollider.bounds.max.x),
            0.3f,
            Random.Range(objectCollider.bounds.min.z, objectCollider.bounds.max.z)
        );

        if (Vector3.Distance(randomPoint, transform.position) < 1) {
            randomPoint = GetRandomPointOnObject();
        }

        if (Vector3.Distance(randomPoint, ballPosition.position) < 1)
        {
            randomPoint = GetRandomPointOnObject();
        }

        return randomPoint;
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = directionMapping[actions.DiscreteActions[0]];
        float moveZ = directionMapping[actions.DiscreteActions[1]];
        float moveSpeed = 2f;

        transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
        if (timePenalty)
        {
            counter -= 1;
            
            if (counter <= 0)
            {
                EndEpisode();
                return;
            }

            AddReward(-1 / counter);
           
        }
        base.OnActionReceived(actions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        switch (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"))) {
            case -1:
                discreteActions[0] = 1; break;
            case 0:
                discreteActions[0] = 0; break;
            case +1:
                discreteActions[0] = 2; break;
        }

        switch (Mathf.RoundToInt(Input.GetAxisRaw("Vertical")))
        {
            case -1:
                discreteActions[1] = 1; break;
            case 0:
                discreteActions[1] = 0; break;
            case +1:
                discreteActions[1] = 2; break;
        }
        //continuousActions[0] = Input.GetAxisRaw("Horizontal");
        //continuousActions[1] = Input.GetAxisRaw("Vertical");
        //base.Heuristic(actionsOut);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(Vector3.Distance(transform.position, goalPosition.position));
        sensor.AddObservation(hasBall ? goalPosition.localPosition: ballPosition.localPosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(1 / Vector3.Distance(transform.localPosition, ballPosition.localPosition));

        float rew = 0.01f * (Vector3.Distance(ballPosition.transform.localPosition, previousPosition) - Vector3.Distance(ballPosition.transform.localPosition, transform.localPosition));
        AddReward(rew);


        if (log)
        {
            print(ballPosition.localPosition);
            print(transform.localPosition);
            print(1 /Vector3.Distance(transform.localPosition, ballPosition.localPosition));
            print(rew);
        }
        // Calculate angle between each ray
        float angleStep = 360f / numberOfRays;

        // Loop through each ray
        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate the direction of the ray using trigonometry
            float angle = i * angleStep;
            Vector3 direction = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0f, Mathf.Cos(Mathf.Deg2Rad * angle));

            // Perform raycast
            RaycastHit hit;
            float distance = raycastDistance;
            if (Physics.Raycast(transform.position, direction, out hit, raycastDistance))
            {
                if(hit.transform.tag == "obstacle")
                {
                    distance = hit.distance;
                }
                // If raycast hits something, do something (e.g., debug log)
                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
            else
            {
                // If raycast doesn't hit anything, draw debug line to end of raycast distance
                Debug.DrawLine(transform.position, transform.position + direction * raycastDistance, Color.green);
            }
            sensor.AddObservation(distance / raycastDistance);
        }


        //base.CollectObservations(sensor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "goal":
                AddReward(-5f);
                EndEpisode();
                break;
            case "obstacle":
                AddReward(-5f);
                EndEpisode();
                break;
            case "ball":
                AddReward(5f);
                ballPosition.transform.SetParent(transform);
                //ballPosition.transform.localPosition = new Vector3(0f, 1f, 0f);
                hasBall = true;
                //EndEpisode();
                break;
            default:
                break;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "ball") { 
            hasBall = false;
        }
    }
}
