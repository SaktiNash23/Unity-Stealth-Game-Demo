//Name of coder: Sativel "Nathan" Thamilvanan
//Latest Update: 24/1/2019
//Script Name: securityCam.cs
//Script Description: Contains logic that simulates the behaviour of a security camera. As of now, the camera can only rotate between 2 target rotation
//                    and detect if player is within line of sight. In the future, I will make the camera be able to send alerts to enemy units and other
//                    features as well.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class securityCam : MonoBehaviour {

    private Quaternion rot1;//Store first target rotation
    private Quaternion rot2;//Store second target rotation
    private float totalCamSpeed;//Actual rotation speed
    private Vector3 origin;//Origin point of where the raycast will start

    public float camSpeed;//Cam speed. Set in editor
    public float delayTime;//Delay in between rotations. Set in editor
    public float radius;//Radius of the sphere cast. Set in editor
    public float rayLength;//Length of sphere cast. Set in editor

    private void Awake()
    {
        //Get the camera's target rotations by searching for the name of the gameObjects Position1 and Position2 which are child objects of the camera GameObject
        rot1 = gameObject.transform.Find("Rotation1").rotation;
        rot2 = gameObject.transform.Find("Rotation2").rotation;
        
    }

    // Use this for initialization
    void Start()
    { 
        totalCamSpeed = camSpeed * Time.deltaTime;//Set cam rotation speed
        StartCoroutine(securityCamCoroutine(rot2));//Start the coroutine with one of the target rotations
    }

    //Coroutine to rotate the camera between 2 rotations. Argument q is the current target rotation
    IEnumerator securityCamCoroutine(Quaternion q)
    {
        //While camera rotation is not equal to the target rotation
        while(transform.rotation != q)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, totalCamSpeed);//Rotate camera towards the target rotation
            yield return null;
        }

        Debug.Log("Reached new rotation");
        yield return new WaitForSeconds(delayTime);//Wait for a few seconds before starting next coroutine. Helps give the feeling of a real security camera

        //After reaching the target rotation as indicated by variable q, start a new coroutine with the other target rotation
        if (q == rot2)
        {
            StartCoroutine(securityCamCoroutine(rot1));
            Debug.Log("Next Coroutine : Rotation1");
        }
        else if (q == rot1)
        {
            StartCoroutine(securityCamCoroutine(rot2));
            Debug.Log("Next Coroutine : Rotation2");
        }
            
    }

    // Update is called once per frame
    private void Update ()
    {
        origin = transform.position;
        RaycastHit hit;//Store the hit result of the raycast
        
        //Fire a raycast from origin towards forward direction of camera
        if (Physics.Raycast(origin, transform.forward, out hit, rayLength))
        {
            Debug.DrawRay(transform.position, transform.forward * rayLength, Color.red);

            //If raycast hist the collider of gameObject with tag "Player"
            if (hit.collider.tag == "Player")
                Debug.Log("Raycast hits player");
        }

        //Fire a spherecast from origin towards forward direction of camera
        if (Physics.SphereCast(origin, radius, transform.forward, out hit, rayLength))
        {
            //If sphere cast hits collider tagged "Player", draw a blue ray cast towards the player to indicate that the camera has seen the player
            if (hit.collider.tag == "Player")
            {
                Debug.Log("Spherecast hits Player");
                Vector3 playerDir = hit.transform.position - transform.position;
                Debug.DrawRay(origin, playerDir * hit.distance, Color.blue);
            }
        }
    }

    //Draws Gizmos in the scene to visualize the raycasts and spherecasts that are used
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + transform.forward * rayLength);
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(origin + transform.forward * rayLength, radius);
    }

}
