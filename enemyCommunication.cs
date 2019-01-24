//Name of coder: Sativel "Nathan" Thamilvanan
//Latest Update: 24/1/2019
//Script Name: enemyCommunication.cs
//Script Description: Contains logic that calculates and finds the closest enemy to the enemy that saw the player 
//                    and requesting for backup when the player is discovered


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyCommunication : MonoBehaviour {

    private List<GameObject> enemyGOs = new List<GameObject>();//Stores all the enemy gameObjects in a list
    private GameObject playerGO;//Player GameObject reference
    private GameObject enemyWhoSawPlayerGO;//Reference to enemy who first detected the player
    private GameObject enemyToCallForBackup;//Reference to enemy who will be called for backup
    private int totalNumOfEnemies;//Total number of enemies
    private float realClosestDistance;//Closest non-alerted enemy to the alerted enemy

    public static bool alert;//Determines if enter alert phase or not. Static because only want 1 instance of it, don't want multiple instances of alert being called from different enemies
    public static Vector3 playerPos;//Stores the last known position of the player. Static because only want 1 instance of it, don't want multiple instance of playerPos being assigned, which could mess up the logic of the enemy when searching for player
    public static bool callOnlyOne;//Ensures backup can only be called once

	// Use this for initialization
	void Start ()
    {
        alert = false;
        callOnlyOne = false;
        totalNumOfEnemies = 3;
        playerGO = GameObject.FindGameObjectWithTag("Player");
        GameObject[] enemyHold = GameObject.FindGameObjectsWithTag("Enemy");//Get all enemies with tag "Enemy"
        for(int i = 0; i < totalNumOfEnemies; i++)
        {
            enemyGOs.Add(enemyHold[i]);
            //Debug.Log("Enemy " + i + ": " + enemyHold[i].name);
        }
	}
	
    //Called to get the closest distance to the enemy who saw the player
    private void getClosestBackupAvailable()
    {
        //Loop 1: Find and store reference to enemy who saw the player(Enemy who saw the player will have enemyDetected set to true)
        for(int a = 0; a < totalNumOfEnemies; a++)
        {
            if(enemyGOs[a].GetComponent<Enemy>().enemyDetected == true)
            {
                enemyWhoSawPlayerGO = enemyGOs[a];
                realClosestDistance = 10000.0f;//Assign any value since we need a value to compare to in Loop 2

                //Loop 2: Once we know which enemy saw the player first, find the distance enemy that is closest to the distance of enemy that saw the player
                for(int b = 0; b < totalNumOfEnemies; b++)
                {
                    //Only checks enemies who haven't seen the player
                    if (enemyGOs[b].GetComponent<Enemy>().enemyDetected == false)
                    {
                        Vector3 closestDistance = enemyGOs[b].transform.position - enemyWhoSawPlayerGO.transform.position;
                        float closestDistanceFloat = closestDistance.magnitude;

                        if (closestDistanceFloat < realClosestDistance)
                            realClosestDistance = closestDistanceFloat;

                    }
                } 
            }
        }
    }

    //Called to determine the enemy to be called as backup
    private void compareDistances(float realDist)
    {
        //Compare the distances between enemy who saw player and the closest enemy to the enemy who saw player by looping through all the enemies
        for(int e = 0; e < totalNumOfEnemies; e++)
        {
            Vector3 holdDist = enemyGOs[e].transform.position - enemyWhoSawPlayerGO.transform.position;
            float holdFloatDist = holdDist.magnitude;

            if(Mathf.Approximately(holdFloatDist, realDist))//If the distances are nearly equal, then assign the enemy with the nearly equal distance as the enemy to call for backup
            {
                enemyToCallForBackup = enemyGOs[e];
                Debug.Log("Enemy To Call for Backup : " + enemyToCallForBackup.name);
                break;
            }
        }
    }


	// Update is called once per frame
	void Update ()
    {

        if (alert)//Only run following code in alert phase, hence saving performance by not constantly running it every frame
        {
            getClosestBackupAvailable();
            compareDistances(realClosestDistance);

            if (!callOnlyOne)//If backup hasn't been called yet
            {
                Debug.Log(enemyToCallForBackup.name + "responds to backup request");
                enemyToCallForBackup.GetComponent<Enemy>().currentEnemyState = Enemy.enemyStates.RESPONDBACKUP;//Switch the enemy to be called for backup to its RESPONDBACKUP state
                playerPos = playerGO.transform.position;//Store position of player, but not updated every frame. If updated every frame, enemy will definitely know where the player's exact position is all the time
                callOnlyOne = true;//Signals that backup has been called
                alert = false;//Set false as we don't want to keep running the code unneccessarily and make unwanted updates
                
            }

        }
	}
}
