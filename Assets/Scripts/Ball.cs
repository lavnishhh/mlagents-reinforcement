using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class Ball : MonoBehaviour
{
    //[SerializeField] private GoalAgent player;
    private GoalAgent player;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform transform in transform.parent.transform)
        {
            if (transform.CompareTag("player"))
            {
                player = transform.GetComponent<GoalAgent>();
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject);
        //GoalAgent player = transform.parent.Find("Capsule").GetComponent<GoalAgent>();
        switch (other.gameObject.tag) {
           
            case "goal":
                player.AddReward(1f);
                player.EndEpisode();
                break;
            default:
                break;
        }
    }
}
