using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    public CharacterController controller;
    


    [Header("Look")]
    //mouse sensitivity
    public float sensitivityX = 1000f;
    public float sensitivityY = 1200f;
    //current look direction
    public float currentX, currentY;
    //camera transform
    public Transform verticalLook;

    [Header("Movement")]
    //The player's movement speed
    public float walkSpeed = 3f;
    public float runSpeed = 4.5f;
    public float crouchSpeed = 2.25f;
    public float crawlSpeed = 1.5f;
    public float moveSpeed;
    //The direction the player is moving in
    public Vector3 moveDirection = Vector3.zero;
    //value of gravity
    public float gravity = -9.81f;
    //speed at which the player falls
    float verticalVelocity = 0f;

    [Header("Raycasts")]
    public float downRayDistance;

    public enum PlayerState { walk, run, crouch, crawl };
    [Header("Player State")]
    public PlayerState state = new PlayerState();
    public float crawlHoldTime = 0.6f;
    float startHoldTime;


    void Start()
    {
        //Lock down the cursor on start
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //reference our character controller
        controller = GetComponent<CharacterController>();

        //set state to walking at start
        state = PlayerState.walk;
    }

    void Update()
    {
        getInput();
        Look();
        Movement();
    }

    void getInput()
    {
        //Get mouse inputs, multiply by sensitivity values, multiply by deltaTime to remain independent of frame rate
        currentX += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        currentY += Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        //Resets moveDirection each tick
        moveDirection = Vector3.zero;

        //Take normalized input and multiplies by our rotation so we move relative to the direction we're facing
        moveDirection += transform.rotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        //Apply move speed and multiply by deltaTime to be independent of frame rate
        moveDirection *= moveSpeed * Time.deltaTime;

        ManageStates();
    }

    void ManageStates()
    {
        //pressing the crouch button resets the timer that determines if you're trying to crouch or crawl
        if (Input.GetButtonDown("Crouch"))
        {
            startHoldTime = Time.time;
        }

        if (state == PlayerState.walk)
        {
            moveSpeed = walkSpeed;

            if (Input.GetButton("Crouch")) { 
                //holding the crouch button for a certain time will switch player to crawl state
                if (Time.time - startHoldTime >= crawlHoldTime)
                {
                    state = PlayerState.crawl;
                } 
            }
            //releaseing crouch button before crawl state will set player to crouch state
            if (Input.GetButtonUp("Crouch") && Time.time - startHoldTime < crawlHoldTime)
            {
                state = PlayerState.crouch;
                startHoldTime = -crawlHoldTime;
            }

        }

        if (state == PlayerState.run)
        {
            moveSpeed = runSpeed;
        }

        if (state == PlayerState.crouch)
        {
            moveSpeed = crouchSpeed;

            //holding the crouch button for a certain time will switch player to crawl state
            if (Input.GetButton("Crouch"))
            {
                if (Time.time - startHoldTime >= crawlHoldTime)
                {
                    state = PlayerState.crawl;
                }
            }
            //releaseing crouch button before crawl state will set player to walk state
            if (Input.GetButtonUp("Crouch") && Time.time - startHoldTime < crawlHoldTime)
            {
                state = PlayerState.walk;
                startHoldTime = -crawlHoldTime;
            }
        }

        if (state == PlayerState.crawl)
        {
            moveSpeed = crawlSpeed;

            //holding the crouch button and tapping it will switch player to crouch state
            if (Input.GetButtonUp("Crouch") && Time.time - startHoldTime < crawlHoldTime)
            {
                    state = PlayerState.crouch;
            }
        }
    }

    void Look()
    {
        //Prevent player from looking farther than straight up or straight down
        if (currentY > 90f)
        {
            currentY = 90f;
        }
        else if (currentY < -60f)
        {
            currentY = -60f;
        }

        //rotate the camera vertically
        verticalLook.localRotation = Quaternion.Euler(-currentY, 0, 0);
        //rotate the character controler horizontally
        transform.rotation = Quaternion.Euler(0, currentX, 0);
    }

    void Movement()
    {
        Gravity();
        controller.Move(moveDirection);
    }

    void Gravity()
    {
        Ray downRay = new Ray(transform.position, Vector3.down);

        Debug.DrawRay(downRay.origin, new Vector3(0, -downRayDistance, 0), Color.red);

        RaycastHit hit;

        if (Physics.Raycast(downRay.origin, downRay.direction, out hit, downRayDistance))
        {
            //Debug.Log(hit.transform.name);
            verticalVelocity = 0f;
        }
        else
        {
            //Formula for gravity multiplies by time^2, so we multiply deltaTime twice
            verticalVelocity += gravity * Time.deltaTime;
            moveDirection.y = verticalVelocity * Time.deltaTime;
        }
    }
}
