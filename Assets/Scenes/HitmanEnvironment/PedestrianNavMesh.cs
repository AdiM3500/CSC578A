using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//script for dictating pedestrian behavior
public class PedestrianNavMesh : MonoBehaviour
{
    public Transform exit;          //exit goal
    public Transform[] goals;

    public NavMeshAgent agent;

    private int currentGoal;

    public float reachedDistance;

    public LayerMask peopleMask;        //layermask to identify only agents and not walls.

    public float fovRadius;

    float avg;          //average framerate
    public float angle;     //fov angle

//bools for statemachine
    public bool isHitmanSeen;

    public bool isPatrolling;     //bool to check if guard is patrolling or evacuating

    public bool IsGuardNotified = false;
    void Start()
    {   
        currentGoal = 0;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(goals[currentGoal].position);       //set the first destination for the pedestrian
        isPatrolling = true;
    }

       void FixedUpdate()
    {
        float distance = Vector3.Distance(goals[currentGoal].position, transform.position);

        //calculate distance between goal and agent
        if(distance <= reachedDistance) {
            agent.velocity = Vector3.zero;

        //if distance to destination is less than the reached distance, do either of these two things below

            if(goals[currentGoal].name == "pedestrian_goal1_bathroom"){
                    

                        //if the goal is located in the bathroom, run a coroutine to make the agent wait for some time at this goal before moving onto the next goal
                StartCoroutine(WaitInBathroom());

            }
            else {
                    //move on to the next goal
                    currentGoal++;
           
           //once all goals have been reached, repeat the patrol
            if(currentGoal >= goals.Length){
                currentGoal = 0;
            }

                agent.SetDestination(goals[currentGoal].position);
            }

        }

        FieldOfViewCheckforPeople();



            //conceptual state machine code
            //if Hitman is seen and agent is still patrolling, begin to evacuate by setting the the exit goal as the destination
        if(isHitmanSeen == true && isPatrolling == true){
            
            isPatrolling = false;
           agent.SetDestination(exit.position);
           float exit_distance =  Vector3.Distance(exit.position, transform.position);
           if(exit_distance <= reachedDistance) {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
           }

        }
       
    }




void Update(){
            //display the average framerate on the screen
     var fps = 1.0/Time.deltaTime;
        Debug.Log(fps);
}

//coroutine to wait in bathroom for 5 seconds before going to next goal
IEnumerator WaitInBathroom(){

    yield return new WaitForSeconds(5);
    currentGoal++;
    if(currentGoal >= goals.Length){
                currentGoal = 0;
            }

                agent.SetDestination(goals[currentGoal].position);

                       // Debug.Log(distance);
            yield break;
}

//do a field of view check 
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

                        //looking at hitman, toggle hitman seen to true
                        if(objectsInRange[i].name == "Hitman"){

                                    isHitmanSeen = true;
                                    Debug.DrawRay(agent.transform.position,  rayDirection * currentDistance , Color.red);
                            }

                        //looking at people or guards
                        else
                        {
                                    Debug.DrawRay(agent.transform.position,  rayDirection * currentDistance , Color.green);

                                    //if hitman is seen
                                    if(isHitmanSeen == true) {

                                        //check if agent in view is a guard
                                       NavMeshAgent  agents= objectsInRange[i].GetComponent<NavMeshAgent>();


                                    //if guard is in view, inform them about hitman
                                    if(agents.tag == "guard") {
                                        
                                        IsGuardNotified = true;
                                        agents.SetDestination(GameObject.Find("Hitman").transform.position);
                                        agents.speed = 7.5f;
                                        Debug.DrawRay(agent.transform.position,  rayDirection * currentDistance , Color.yellow);

                                    }
                                       

                                    //if another pedestrian, inform them about hitman and they shall begin evacuating too.
                                    else{   

                                                agents.SetDestination(exit.position);
                                    float exit_distance =  Vector3.Distance(exit.position, agents.transform.position);
                                    if(exit_distance <= reachedDistance) {
                                    agent.velocity = Vector3.zero;
                                    agent.isStopped = true;
                                    }

                                    }
                                        


                                }
                        }
                            
            }


                    
            }

        
            
         }
    }



}