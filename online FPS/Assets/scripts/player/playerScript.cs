using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

public class playerScript : NetworkBehaviour
{
    [Header("set Values: ")]
    [SerializeField]private float normalSpeed;
    [SerializeField]private float sprintSpeed;
    [SerializeField]private float mouseSens;
    [SerializeField]private float Startgravity;
    [SerializeField]private float jumpHeight;
    [SerializeField]private float wallRunRayDistance;
    [SerializeField]private float wallRunGravity;
    [SerializeField]private float wallRunSpeed;
    [SerializeField]private float wallRunRotationSmoothness;
    [Header("Set variables: ")]
    [SerializeField]private Transform cameraTransform;
    [SerializeField]private Transform groundCheck;
    [SerializeField]private LayerMask groundMask;
    [SerializeField]private Transform weapon;
    [SerializeField]private GameObject canvas;
    [SerializeField]private GameObject pauseMenuPanel;
    [SerializeField]private Transform wallRunTransform;
    [SerializeField]private Transform cameraRecoil;

    private CharacterController controller;
    private Camera mainCam;
    private AudioSource audioSource;
    private NetworkTransformChild networkTransformChild;
    private playerDeath playerDeathScript;
    private Animator anim;

    private float moveX;
    private float MoveZ;

    private float mouseX;
    private float mouseY;

    private float xRotation = 0f;

    private Vector3 velocity;
    private Vector3 weaponStartPos;
    private bool isWallRunning;
    private float gravity;

    private bool isGrounded;

    private float speed;
    private bool pauseMenuActive;

    private void Start() 
    {
        string ID = "Player " + NetworkServer.connections.Count;
        transform.name = ID;

        if(isLocalPlayer)
        {
            transform.tag = "localPlayer";
            weapon.gameObject.layer = LayerMask.NameToLayer("localWeapon");
            for(int i = 0; i < weapon.childCount; i++)
            {
                weapon.GetChild(i).gameObject.layer = LayerMask.NameToLayer("localWeapon");
            }
        }
        else
        {
            transform.tag = "remotePlayer";
            weapon.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        if(!isLocalPlayer){return;}
        audioSource = GameObject.Find("SFX").GetComponent<AudioSource>();
        controller = gameObject.GetComponent<CharacterController>();
        playerDeathScript = gameObject.GetComponent<playerDeath>();
        mainCam = Camera.main;
        mainCam.transform.position = cameraTransform.position;
        mainCam.transform.rotation = cameraTransform.rotation;
        mainCam.transform.parent = cameraTransform;
        Cursor.lockState = CursorLockMode.Locked;
        speed = normalSpeed;
        gravity = Startgravity;
        canvas.SetActive(true);
        anim = gameObject.GetComponent<Animator>();
        weaponStartPos = weapon.position;
    }

    private void Update() 
    {
        if(!isLocalPlayer){return;}
        if(playerDeathScript.isDead == true){return;}

        if(pauseMenuActive == true)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.Locked;
                pauseMenuPanel.SetActive(false);
                pauseMenuActive = false;
            }
            else
            {
                return;
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            pauseMenuPanel.SetActive(true);
            pauseMenuActive = true;
        }
        getInputs();
        movement();
        mouseLook();
        wallrun();
    }

    private void wallrun()
    {
        //right wallrun
        if(Physics.Raycast(wallRunTransform.position, wallRunTransform.right, out RaycastHit hit_R, wallRunRayDistance))
        {
            if(hit_R.transform.tag == "wallRideable")
            {
                isWallRunning = true;
                gravity = wallRunGravity;
                speed = wallRunSpeed;
                Quaternion originalRotation = cameraTransform.localRotation;
                Quaternion setPosition = new Quaternion(cameraTransform.localRotation.x, cameraTransform.localRotation.y, 20, cameraTransform.localRotation.w);
                cameraTransform.localRotation = Quaternion.Lerp(originalRotation, setPosition, wallRunRotationSmoothness);
            }
        }
        //left wallrun
        else if(Physics.Raycast(wallRunTransform.position, -wallRunTransform.right, out RaycastHit hit_L, wallRunRayDistance))
        {
            if(hit_L.transform.tag == "wallRideable")
            {

                isWallRunning = true;
                gravity = wallRunGravity;
                speed = wallRunSpeed;
                Quaternion originalRotation = cameraTransform.localRotation;
                Quaternion setPosition = new Quaternion(cameraTransform.localRotation.x, cameraTransform.localRotation.y, -20, cameraTransform.localRotation.w);
                cameraTransform.localRotation = Quaternion.Lerp(originalRotation, setPosition, wallRunRotationSmoothness);
            }
        }
        else
        {
            isWallRunning = false;
            gravity = Startgravity;
            Quaternion originalRotation = cameraTransform.localRotation;
            Quaternion setPosition = new Quaternion(cameraTransform.localRotation.x, cameraTransform.localRotation.y, 0, cameraTransform.localRotation.w);
            cameraTransform.localRotation = Quaternion.Lerp(originalRotation, setPosition, wallRunRotationSmoothness);
        }
    }

    private void mouseLook()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85, 85);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void getInputs()
    {
        moveX = Input.GetAxis("Horizontal");
        MoveZ = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X") * mouseSens;
        mouseY = Input.GetAxis("Mouse Y") * mouseSens;
    }

    private void movement()
    {
        Vector3 move = transform.right * moveX + transform.forward * MoveZ;
        controller.Move(move * speed * Time.deltaTime);
        //gravity
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        //jump
        if(Input.GetButtonDown("Jump") && isGrounded && isWallRunning == false)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
        //sprint
        if(Input.GetKey(KeyCode.LeftShift) && isWallRunning == false)
        {
            //isSprinting
            speed = sprintSpeed;
        }
        else if(!Input.GetKey(KeyCode.LeftShift) && isWallRunning == false)
        {
            //notSprinting
            speed = normalSpeed;
        }
    }
    public void pauseMenu_Resume()
    {
        if(!isLocalPlayer){return;}
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenuPanel.SetActive(false);
        pauseMenuActive = false;
    }
    public void pauseMenu_MainMenu()
    {
        NetworkManager.singleton.StopHost();
    }
    public void pauseMenu_Exit()
    {
        if(isClientOnly)
        {
            NetworkManager.singleton.StopClient();
        }
        else if(isServer)
        {
            NetworkManager.singleton.StopHost();
        }
        Application.Quit();
    }
}
