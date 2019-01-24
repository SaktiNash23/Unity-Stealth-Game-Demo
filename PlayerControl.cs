//Name of coder: Sativel "Nathan" Thamilvanan
//Latest Update: 24/1/2019
//Script Name: PlayerControl.cs
//Script Description: Controls the movement of the character and sets animation parameters to be used by the animator controller

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    private float moveX;//Store horizontal axis value
    private float moveY;//Store vertical axis value
    private float moveSpd;//Move Speed of character
    private Vector3 vectorX;//Stores X movement vector
    private Vector3 vectorZ;//Stores Z movement vector

    public Transform camDirection;//Stores reference to GameObject directionReference's Transform component
    public Animator characterAnim;//Stores reference to character's Animator component

    void Awake()
    {
        //Get component(s) & initialize values
        moveSpd = 5.0f;
        characterAnim = GetComponent<Animator>();
        characterAnim.SetBool("crouchAnim", false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        moveX = Input.GetAxis("Horizontal");//Get horizontal axis value (A,D or Left Arrow, Right Arrow)
        moveY = Input.GetAxis("Vertical");//Get vertical axis value(W,S or Up Arrow, Down Arrow)

        vectorX = camDirection.right * moveX * moveSpd * Time.deltaTime;//Create vector based on direction given by camDirection.right (left & right)
        vectorZ = camDirection.forward * moveY * moveSpd * Time.deltaTime;//Create vector based on direction given by camDirection.forward(forward & backward)

        Vector3 dirVector = (vectorX + vectorZ);//Add both vectors to get resultant vector
        dirVector = dirVector.normalized;//Set vector magnitude to 1 while maintaining its direction
        
        //While user holds down Left Shift Key, play crouch animation
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("Crouching");
            characterAnim.SetBool("crouchAnim", true);
            moveSpd = 2.0f;
        }
        //While user does not hold down Left Shift Key, return to Standing/Idle animation
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            Debug.Log("Standing");
            characterAnim.SetBool("crouchAnim", false);
            moveSpd = 5.0f;
        }

        //Controls the  movement animation of the character and the transitions between animations depending on the moveX and moveY values
        if (moveX != 0.0f || moveY != 0.0f)
        {
            if (moveX <= 0.5f && moveY >= 0.1f)//Move Forward
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Vertical"));
            else if (moveX <= 0.5f && moveY <= -0.1f)//Move Backward
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Vertical") * -1.0f);
            else if (moveX >= 0.1f && moveY <= 0.5f)//Move Right
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Horizontal"));
            else if (moveX <= -0.1f && moveY <= 0.1f)//Move Left
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Horizontal") * -1.0f);
            else if (moveX >= 0.5f && moveY >= 0.5f)//Move Up-Right
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Horizontal"));
            else if (moveX >= 0.5f && moveY <= -0.5f)//Move Down-Right
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Horizontal"));
            else if(moveX <= 0.5f && moveY <= 0.5f)//Move Down-Left
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Horizontal") * -1.0f);
            else if(moveX <= -0.5f && moveY > 0.0f)//Move-Up-Left
                characterAnim.SetFloat("moveSpeedAnim", Input.GetAxis("Vertical"));
            
            transform.rotation = Quaternion.LookRotation(dirVector, transform.up);//Rotate character to look in the direction pointed by dirVector variable
            
        }
        else
            characterAnim.SetFloat("moveSpeedAnim", 0.0f);

        transform.position += dirVector * moveSpd * Time.deltaTime;//Update position of the character every frame when moving

    }
}
