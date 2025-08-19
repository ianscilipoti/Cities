using UnityEngine;
using System.Collections;

public class TownResidentActor : MonoBehaviour
{
    Animator animator;
    Transform player;
    float lookAtDistanceSqr = 25;
    bool withinInteractionDistance = false;
    public TownResident data;
    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.GetComponent<PlayerInteraction>().RegisterResident(this);

    }

    // Update is called once per frame
    void Update()
    {
        if (player) {
            if ((player.position - transform.position).sqrMagnitude < lookAtDistanceSqr)
            {
                transform.forward = Vector3.Slerp(transform.forward, Vector3.Scale((player.position - transform.position), new Vector3(1, 0, 1)), 0.01f);
                withinInteractionDistance = true;
            }
            else
            {
                if (withinInteractionDistance)
                {
                    player.SendMessage("endInteraction", SendMessageOptions.DontRequireReceiver);
                    animator.SetBool("isTalking", false);
                }
                withinInteractionDistance = false;
            }
        }
    }

    public void RecieveInteraction ()
    {
        Debug.Log("message recieved from town guy");
        animator.SetBool("isTalking", true);

        if (withinInteractionDistance && data != null)
        {
            player.SendMessage("successfulInteraction", data.name + ": " + data.backstory[Random.Range(0, data.backstory.Count)], SendMessageOptions.DontRequireReceiver);
        }
    } 
}
