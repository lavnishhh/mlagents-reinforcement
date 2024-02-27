using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class GoalAgent : Agent
{

    [SerializeField] private Transform goalPosition;
    [SerializeField] private Transform ballPosition;
    bool hasBall;

    public override void OnEpisodeBegin()
    {
        //transform.localPosition = new Vector3(0f, 0.35f, 0f);
        ballPosition.transform.localPosition = new Vector3(Random.Range(-1.5f, 1.5f), 0.3f, Random.Range(-1.5f, 1.5f));
        transform.localPosition = new Vector3(Random.Range(-1.5f, 1.5f), 0.3f, Random.Range(-1.5f, 1.5f));
        //ballPosition.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //ballPosition.transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        //base.OnEpisodeBegin();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.DiscreteActions[0];
        float moveZ = actions.DiscreteActions[1];
        float moveSpeed = 2f;

        transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

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
        sensor.AddObservation(transform.position.normalized);
        sensor.AddObservation(ballPosition.position.normalized);
        sensor.AddObservation(goalPosition.position.normalized);
        sensor.AddObservation(hasBall);
        //base.CollectObservations(sensor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.tag);
        switch (collision.gameObject.tag)
        {
            case "goal":
                AddReward(-5f);
                EndEpisode();
                break;
            case "wall":
                AddReward(-5f);
                EndEpisode();
                break;
            case "ball":
                AddReward(5f);
                hasBall = true;
                ballPosition.transform.SetParent(transform);
                ballPosition.transform.localPosition = new Vector3(0f, 1f, 0f);
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
