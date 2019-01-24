using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    public GameObject playerGO;//Reference to player GameObject
    public Vector3 camOffset;//How far the camera must be from the player?
    public Transform directionReference;//Stores the Transform component of the directionReference gameObject
    public float camSpeed;//Camera move speed

    private float rotY;//Stores the y axis rotation of the camera in euler angles

    private void Awake ()
    {
        //Initialize values
        transform.position = playerGO.transform.position + camOffset;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        transform.LookAt(playerGO.transform);
    }
    
    // Update is called once per frame
    void Update ()
    {
        transform.position = playerGO.transform.position + camOffset;//Updates the position offset of the camera from the player every frame

        float x = Input.GetAxis("Mouse X") * Time.deltaTime;//Get Mouse X axis value

        //Rotates the camera at a set angle (angle specified at x * camSpeed) around a specified axis (axis is playerGO.transform.up)
        camOffset = Quaternion.AngleAxis(x * camSpeed, playerGO.transform.up) * camOffset;//Note: multiply with camOffset is needed as we can't assign a Quartenion to a vector variable, hence multiplying with a vector converts Quartenion to a vector
        transform.LookAt(playerGO.transform);//transform.LookAt is required to make the camera always face the player, else the camera will 
                                             //just rotate around the player while looking away from the player

        //Get the y axis rotation of the camera in euler angles
        rotY = transform.rotation.eulerAngles.y;

        Quaternion refRot = Quaternion.Euler(0.0f, rotY, 0.0f);//Modify only the Y axis rotation value using camera's y axis rotation, X and Z will be constant 0.0f
        directionReference.rotation = refRot;//Assign rotation to directionReference rotation so that the player will move in the direction the camera is facing
        
	}
    
}
