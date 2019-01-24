using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;//Required to access the NavMesh / NavMeshAgent library

public class Enemy : MonoBehaviour {

    private Vector3 playerDirection;//Stores the direction of from the enemy to the player
    private Vector3 playerPosition;//Stores the position of the player in the world
    private float dotProduct;//Stores the dot product value of playerDirection and enemy transform.forward
    private int patrolIndex;//Index used to control the patrolPoints[] array
    private float waitTime;//How long an enemy needs to wait after reaching backup point?
    private float searchTime;//How long an enemy needs to search for after reaching player's last known position?
   
    public NavMeshAgent enemyAgent;//Stores the nav mesh agent
    public GameObject[] patrolPoints;//Array to store the patrol points
    public GameObject playerGO;//Player GameObject
    public bool enemyDetected;//Determines whether the enemy has seen the player
    public enum enemyStates {PATROL, CHASE, CALLBACKUP, RESPONDBACKUP, GOLASTKNOWNPOSITION};//Enumerations to form a simple state machine
    public enemyStates currentEnemyState;//Stores the current state of the enemy    

	private void Awake ()
    {
        enemyAgent = GetComponent<NavMeshAgent>();	
	}

    private void Start()
    {
        waitTime = 2.0f;
        searchTime = 5.0f;
        currentEnemyState = enemyStates.PATROL;
        enemyDetected = false;
        patrolIndex = 0;
        enemyAgent.destination = patrolPoints[patrolIndex].transform.position;
    }

    //Calculates the dotProduct between enemy's transform.forward and playerDir. dotProduct used to determine if player is within line of sight of enemy
    //Arguments are passed-by-reference to modify the variables playerDirection and dotProduct values directly
    private void calculateDotProduct(ref Vector3 playerDir, ref float dProduct)
    {
        playerDir = playerGO.transform.position - transform.position;//Create a line from enemy to player
        playerDir = playerDirection.normalized;//Normalize the vector to magnitude 1(can be used to move the enemy in direction of player)

        dProduct = Vector3.Dot(transform.forward, playerDir);
        dProduct = Mathf.Acos(dProduct);
        dProduct = dProduct * Mathf.Rad2Deg;//Convert radian value to degrees
        //Debug.Log("Vision Angle : " + dotProduct);

    }

    //Called while enemy is in PATROL state
    private void patrol()
    {
        //If enemy hasn't see the player
        if (!enemyDetected)
        {
               //If enemy has reached current patrol point, update to its next patrol point
               if (enemyAgent.remainingDistance == 0.0f)
               {
                   //Debug.Log("Reached destination");
                   if (patrolIndex != patrolPoints.Length - 1)
                       patrolIndex++;
                   else
                       patrolIndex = 0;

                   enemyAgent.destination = patrolPoints[patrolIndex].transform.position;
               }

        }
    }

    //Called whenever we want the enemy to look for the player, either while patrolling, when searching for enemy, etc...
    private void sensePlayer()
    {
        
        calculateDotProduct(ref playerDirection, ref dotProduct);
        Vector3 lineToPlayer = playerGO.transform.position - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, lineToPlayer, Vector3.up);//Vector3.SignedAngle will include positive & negative values, unlike Vector3.Angle which only returns positive values
                                                                                       //SignedAngle is useful to determine if an object is in front or behind another object

        int layerMask = 1 << 9;//Bitshift through 9 bits and replace 9th bit from 0 with a 1, therefore activating layer 9


        //Check if player is within enemy field of vision and if player is in front of enemy
        if (dotProduct < 55.0f || dotProduct < 120.0f && angle > -0.1f)
        {
            //If enemy view is blocked by obstacle like walls
            if (Physics.Raycast(transform.position, playerDirection, 1000.0f, layerMask))//This raycast will detect only layer 9
            {
                Debug.DrawRay(transform.position, playerDirection * 1000.0f, Color.red);//Red raycast if enemy line of sight is blocked
                enemyDetected = false;
            }
            //Else if enemy view is not blocked and can see player clearly
            else
            {
                Debug.DrawRay(transform.position, playerDirection * 1000.0f, Color.green);//Green RayCast if enemy can see player clearly
                enemyDetected = true;
                enemyAgent.destination = playerGO.transform.position;

                //Depending on which state the enemy is when they saw the player, switch to the new proper state
                if (currentEnemyState == enemyStates.RESPONDBACKUP)
                {
                    enemyCommunication.alert = false;
                    currentEnemyState = enemyStates.CHASE;
                }
                else if (currentEnemyState == enemyStates.PATROL)
                    currentEnemyState = enemyStates.CALLBACKUP;
                else if (currentEnemyState == enemyStates.CHASE)
                    currentEnemyState = enemyStates.CHASE;

            }
        }
        //If player is not within enemy's line of sight at all
        else
            enemyDetected = false;  
        
    }

    //Called whenever the enemy is in the CHASE state
    private void chasePlayer()
    {
        calculateDotProduct(ref playerDirection, ref dotProduct);
        Vector3 lineToPlayer = playerGO.transform.position - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, lineToPlayer, Vector3.up);//Vector3.SignedAngle will include positive & negative values, unlike Vector3.Angle which only returns positive values
                                                                                       //SignedAngle is useful to determine if an object is in front or behind another object

        int layerMask = 1 << 9;//Bitshift through 9 bits and replace 9th bit from 0 with a 1, therefore activating layer 9



        if (dotProduct < 55.0f || dotProduct < 120.0f && angle > -0.1f)//Check if player is within enemy field of vision and if player is in front of enemy
        {
            //If enemy line of sight to player is blocked while enemy is chasing the player, switch state to GOLASTKNOWNPOSITION
            if (Physics.Raycast(transform.position, playerDirection, 1000.0f, layerMask))//This raycast will detect only layer 9
            {
                Debug.DrawRay(transform.position, playerDirection * 1000.0f, Color.red);//Red raycast if enemy line of sight is blocked
                enemyDetected = false;
                enemyCommunication.alert = false;//Reset alert phase
                enemyCommunication.callOnlyOne = false;//Reset backup phase
                currentEnemyState = enemyStates.GOLASTKNOWNPOSITION;
                
            }
            //If enemy can see the player clearly while chasing player, keep chasing the player
            else
            {
                Debug.DrawRay(transform.position, playerDirection * 1000.0f, Color.green);//Green RayCast if enemy can see player clearly
                enemyDetected = true;
                playerPosition = playerGO.transform.position;
                enemyAgent.destination = playerGO.transform.position;

            }
        }
        //If player not within enemy's line of sight at all while chasing, switch to GOLASTKNOWNPOSITION state
        else
        {
            enemyDetected = false;
            enemyCommunication.alert = false;//Reset alert phase
            enemyCommunication.callOnlyOne = false;//Reset backup phase
            currentEnemyState = enemyStates.GOLASTKNOWNPOSITION;
           
        }
        
    }

    //Called whenever the enemy is in GOLASTKNOWNPOSITION state
    private void searchForPlayer()
    {
        enemyAgent.destination = playerPosition;//Go to player's last known position indicated by playerPosition
        calculateDotProduct(ref playerDirection, ref dotProduct);
        Vector3 lineToPlayer = playerGO.transform.position - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, lineToPlayer, Vector3.up);//Vector3.SignedAngle will include positive & negative values, unlike Vector3.Angle which only returns positive values
                                                                                       //SignedAngle is useful to determine if an object is in front or behind another object
                                                                                       //Debug.Log("Angle : " + angle);                                                 //In this case, it is used to determine whether the player is in front of the enemy or not

        int layerMask = 1 << 9;//Bitshift through 9 bits and replace 9th bit from 0 with a 1, therefore activating layer 9

        //Check if player is within enemy field of vision and if player is in front of enemy
        if (dotProduct < 55.0f || dotProduct < 120.0f && angle > -0.1f)
        {
            //If enemy view is blocked and can't see player while searching for player, make enemy search for player for a set amount of time before returning to PATROL state and resetting alert phase
            if (Physics.Raycast(transform.position, playerDirection, 1000.0f, layerMask))//This raycast will detect only layer 9
            {
                Debug.DrawRay(transform.position, playerDirection * 1000.0f, Color.red);
                enemyDetected = false;
                enemyCommunication.alert = false;//Reset alert phase
                enemyCommunication.callOnlyOne = false;//Reset backup phase
                if (enemyAgent.remainingDistance <= 10.0f && !enemyDetected)
                {
                    searchTime -= Time.deltaTime;
                    //Debug.Log("Wait Time : " + waitTime);
                    if (searchTime <= 0.0f)
                    {
                        searchTime = 5.0f;
                        Debug.Log("Search Completed. Returning to Patrol Route");
                        enemyCommunication.alert = false;//Reset alert phase
                        enemyCommunication.callOnlyOne = false;//Reset backup call
                        currentEnemyState = enemyStates.PATROL;
                        
                    }
                }

            }
            //If enemy has found player while searching, reset the searchTime and switch to CHASE state
            else
            {
                Debug.DrawRay(transform.position, playerDirection * 1000.0f, Color.green);
                enemyDetected = true;
                searchTime = 2.0f;
                playerPosition = playerGO.transform.position;
                enemyAgent.destination = playerGO.transform.position;
                currentEnemyState = enemyStates.CHASE;

            }
        }
        //If player not within enemy's line of sight at all, make enemy search for a set amount of time before returning to PATROL state
        else
        {
            enemyDetected = false;
            enemyCommunication.alert = false;
            enemyCommunication.callOnlyOne = false;
            if (enemyAgent.remainingDistance <= 10.0f && !enemyDetected)
            {
                searchTime -= Time.deltaTime;
                //Debug.Log("Wait Time : " + waitTime);
                if (searchTime <= 0.0f)
                {
                    searchTime = 5.0f;
                    Debug.Log("Search Completed.No enemy found. Returning to Patrol Route");
                    enemyCommunication.alert = false;//Reset alert phase
                    enemyCommunication.callOnlyOne = false;//Reset backup call
                    currentEnemyState = enemyStates.PATROL;

                }
            }
        }
    }

    //Called whenever the enemy is in RESPONDBACKUP state
    private void backup()
    {
        //Make enemy move to position where player was spotted
        enemyAgent.destination = enemyCommunication.playerPos;
        //Once enemy reached where player was spotted, wait for a few seconds before resetting alert phase and returning to PATROL state
        if (enemyAgent.remainingDistance <= 15.0f && enemyDetected == false)
        {
            waitTime -= Time.deltaTime;
            //Debug.Log("Wait Time : " + waitTime);
            if (waitTime <= 0.0f)
            {
                waitTime = 2.0f;
                Debug.Log("Reached Backup Point");
                enemyCommunication.alert = false;//Reset alert phase
                enemyCommunication.callOnlyOne = false;//Reset backup call
                currentEnemyState = enemyStates.PATROL;

            }

        }
    }

   // Update is called once per frame
   private void Update ()
   {
       //A simple State Machine which forms the basis of the enemy AI
       switch(currentEnemyState)
       {
           case enemyStates.PATROL:
               patrol();
               sensePlayer();
               break;

            case enemyStates.CHASE:
                chasePlayer();
                break;

            case enemyStates.GOLASTKNOWNPOSITION:
                searchForPlayer();
                break;

            case enemyStates.CALLBACKUP:
                enemyCommunication.alert = true;
                currentEnemyState = enemyStates.CHASE;
                break;

            case enemyStates.RESPONDBACKUP:
                sensePlayer();
                backup();
                break;
                
       }

    }

}

