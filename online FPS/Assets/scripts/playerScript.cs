using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class playerScript : NetworkBehaviour
{
    [Header("set Values: ")]
    [SerializeField]private float normalSpeed;
    [SerializeField]private float sprintSpeed;
    [SerializeField]private float mouseSens;
    [SerializeField]private float gravity;
    [SerializeField]private float jumpHeight;
    [SerializeField]private float fireRate;
    [SerializeField]private float bulletsInMag;
    [Header("Set variables: ")]
    [SerializeField]private Transform cameraTransform;
    [SerializeField]private Transform groundCheck;
    [SerializeField]private LayerMask groundMask;
    [SerializeField]private Transform weaponHolster;
    [SerializeField]private Transform weapon;
    [SerializeField]private Transform ADStransform;
    [SerializeField]private Transform hipFireTransform;

    private CharacterController controller;
    private Camera mainCam;
    private AudioSource audioSource;

    private float moveX;
    private float MoveZ;

    private float mouseX;
    private float mouseY;

    private float xRotation = 0f;

    private Vector3 velocity;

    private bool isGrounded;

    private float speed;

    private float nextFire = 0f;
    private float bulletCount = 0f;


    private void Start() 
    {
        if(isLocalPlayer)
        {
            transform.tag = "localPlayer";
        }
        else
        {
            transform.tag = "remotePlayer";
        }
        if(!isLocalPlayer){return;}
        audioSource = GameObject.Find("SFX").GetComponent<AudioSource>();
        controller = gameObject.GetComponent<CharacterController>();
        mainCam = Camera.main;
        mainCam.transform.position = cameraTransform.position;
        mainCam.transform.rotation = cameraTransform.rotation;
        mainCam.transform.parent = transform;
        weaponHolster.transform.parent = mainCam.transform;
        Cursor.lockState = CursorLockMode.Locked;
        speed = normalSpeed;
    }

    private void Update() 
    {
        if(!isLocalPlayer){return;}
        getInputs();
        movement();
        mouseLook();
        weaponADS();
        shoot();
    }

    private void shoot()
    {
        if(Input.GetButton("Fire1") && Time.time > nextFire && bulletCount <= bulletsInMag)
        {
            audioSource.Play();
            nextFire = Time.time + fireRate;
            if(Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out RaycastHit hit, Mathf.Infinity))
            {
                CmdHitSomthing();
            }
        }
    }

    private void weaponADS()
    {
        if(Input.GetKey(KeyCode.Mouse1))
        {
            weapon.transform.position = ADStransform.position;
        }
        else if(!Input.GetKey(KeyCode.Mouse1))
        {
            weapon.transform.position = hipFireTransform.position;
        }

    }

    private void mouseLook()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85, 85);

        mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
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
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
        //sprint
        if(Input.GetKey(KeyCode.LeftShift))
        {
            //isSprinting
            speed = sprintSpeed;
        }
        else
        {
            //notSprinting
            speed = normalSpeed;
        }
    }
    [Command]
    private void CmdHitSomthing()
    {
        Debug.Log("hit somthing!");
    }
}
