using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//script for dictating guard behavior
public class GuardNavMesh : MonoBehaviour
{
    public Transform[] goals;  

    public NavMeshAgent agent;

    private int currentGoal;

    public float reachedDistance;

    public LayerMask peopleMask;   //layermask to identify only agents and not walls.


    public float fovRadius;



    public float angle;

    public bool isHitmanSeen;

   
    void Start()
    {   
        currentGoal = 0;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(goals[currentGoal].position);          //set the first destination for the guard
    }

     void FixedUpdate()
    {
        //calculate distance between goal and agent
        float distance = Vector3.Distance(goals[currentGoal].position, transform.position);

                          
        //if distance to destination is less than the reached distance, move on the next goal
        if(distance <= reachedDistance) {
            agent.velocity = Vector3.zero;
            currentGoal++;
      
            //repeat patrol behavior by moving back to the first goal
            if(currentGoal >= goals.Length){
                currentGoal = 0;
            }

                agent.SetDestination(goals[currentGoal].position);
                      



        }

        //function to check which agents are in the field of view of a guard agent
        FieldOfViewCheckforPeople();
    }

    private void FieldOfViewCheckforPeople() {

        NavMeshHit hit;
        Collider[] objectsInRange; 
       
        //check which agents are inside the range of the FoV radius
         objectsInRange = Physics.OverlapSphere(agent.transform.position, fovRadius, peopleMask);


         for(int i = 0; i < objectsInRange.Length; i++){

               
            var directionToTarget = objectsInRange[i].transform.position - agent.transform.position;

            //check the angular difference between the forward angle and the direction to target to see if it;s within the FoV angle
            if(Vector3.Angle(transform.forward, directionToTarget) < (angle /2) ){

                    //cast a ray to check if there are any obstacles between the agents 
                    if(!agent.Raycast(objectsInRange[i].transform.position, out hit)){

                            
                            var rayDirection = objectsInRange[i].transform.position - agent.transform.position;
                            var currentDistance = Vector3.Distance(agent.transform.position, objectsInRange[i].transform.position);

                            //change behavior if hitman is seen
                            if(objectsInRange[i].name == "Hitman"){

                                    isHitmanSeen = true;
                                    Debug.DrawRay(agent.transform.position,  rayDirection * currentDistance , Color.red);

                                        //chase hitman at an increased speed after seeing him
                                        agent.SetDestination(GameObject.Find("Hitman").transform.position);
                                        agent.speed = 7.5f;                      
                            }

                        //friendly agent in view
                        else
                               {
                                Debug.DrawRay(agent.transform.position,  rayDirection * currentDistance , Color.green);


                               } 
            }


                    
            }

        
            
         }

        //always chase hitman after seeing him
         if(isHitmanSeen == true) {
            agent.SetDestination(GameObject.Find("Hitman").transform.position);
                                        agent.speed = 7.5f;
         }
    }



}
