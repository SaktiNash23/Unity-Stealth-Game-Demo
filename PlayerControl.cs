using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    float moveX;
    float moveY;
    float moveSpd;

    public Transform camDirection;//Reference to GameObject directionReference's Transform component
    public Animator characterAnim;

    Vector3 vectorX;
    Vector3 vectorY;

    void Awake()
    {
        moveSpd = 5.0f;
        characterAnim = GetComponent<Animator>();
        characterAnim.SetBool("crouchAnim", false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical");

        vectorX = camDirection.right * moveX * moveSpd * Time.deltaTime;
        vectorY = camDirection.forward * moveY * moveSpd * Time.deltaTime;

        Vector3 dirVector = (vectorX + vectorY);
        dirVector = dirVector.normalized;
        
        
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("Crouching");
            characterAnim.SetBool("crouchAnim", true);
            moveSpd = 2.0f;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            Debug.Log("Standing");
            characterAnim.SetBool("crouchAnim", false);
            moveSpd = 5.0f;
        }


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
            
            transform.rotation = Quaternion.LookRotation(dirVector, transform.up);
            //Debug.Log("move X : " + moveX);
            //Debug.Log("move Y : " + moveY);
        }
        else
            characterAnim.SetFloat("moveSpeedAnim", 0.0f);

        transform.position += dirVector * moveSpd * Time.deltaTime;


    }
}
