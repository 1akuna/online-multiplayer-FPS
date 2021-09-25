using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

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
    [SerializeField]private float reloadTime;
    [Header("Set variables: ")]
    [SerializeField]private Transform cameraTransform;
    [SerializeField]private Transform groundCheck;
    [SerializeField]private LayerMask groundMask;
    [SerializeField]private Transform weaponHolster;
    [SerializeField]private Transform weapon;
    [SerializeField]private Transform ADStransform;
    [SerializeField]private Transform hipFireTransform;
    [SerializeField]private Transform gunBarrel;
    [SerializeField]private GameObject bulletTrail;
    [SerializeField]private TMP_Text bulletCountText;

    private CharacterController controller;
    private Camera mainCam;
    private AudioSource audioSource;
    private NetworkTransformChild networkTransformChild;

    private float moveX;
    private float MoveZ;

    private float mouseX;
    private float mouseY;

    private float xRotation = 0f;

    private Vector3 velocity;

    private bool isGrounded;
    private bool isReloading = false;

    private float speed;

    private float nextFire = 0f;
    private float bulletCount = 30f;


    private void Start() 
    {
        string ID = "Player " + GetComponent<NetworkIdentity>().netId;
        transform.name = ID;

        if(isLocalPlayer)
        {
            transform.tag = "localPlayer";
            weapon.gameObject.layer = LayerMask.NameToLayer("localWeapon");
        }
        else
        {
            transform.tag = "remotePlayer";
            weapon.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        if(!isLocalPlayer){return;}
        audioSource = GameObject.Find("SFX").GetComponent<AudioSource>();
        controller = gameObject.GetComponent<CharacterController>();
        mainCam = Camera.main;
        mainCam.transform.position = cameraTransform.position;
        mainCam.transform.rotation = cameraTransform.rotation;
        mainCam.transform.parent = cameraTransform;
        Cursor.lockState = CursorLockMode.Locked;
        speed = normalSpeed;
    }

    private void Update() 
    {
        if(!isLocalPlayer){return;}
        bulletCountText.text = bulletCount.ToString();
        getInputs();
        movement();
        mouseLook();
        weaponADS();
        if(Input.GetKey(KeyCode.R))
        {
            StartCoroutine(reload());
        }
        if(Input.GetButton("Fire1") && Time.time > nextFire && bulletCount > 0 && isReloading == false)
        {
            audioSource.Play();
            nextFire = Time.time + fireRate;
            bulletCount--;
            CmdShoot(mainCam.transform.position, mainCam.transform.forward, gunBarrel.position, gunBarrel.rotation);
        }
    }

    private IEnumerator reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        bulletCount = 30;
        isReloading = false;
    }

    [Command]
    private void CmdShoot(Vector3 _mainCamPos, Vector3 _mainCamDirection, Vector3 _gunBarrelPos, Quaternion _gunBarrelRot)
    {
        if(Physics.Raycast(_mainCamPos, _mainCamDirection, out RaycastHit hit, Mathf.Infinity))
        {
            GameObject bulletTrailInstance = Instantiate(bulletTrail, _gunBarrelPos, _gunBarrelRot);
            NetworkServer.Spawn(bulletTrailInstance);
            spawnBulletTrail(bulletTrailInstance, _gunBarrelPos, hit.point);
            Destroy(bulletTrailInstance, 1f);
            if(hit.transform.tag == "remotePlayer" || hit.transform.tag == "localPlayer")
            {
                Debug.Log(hit.transform.name + " has been hit");
                ClientChangeColor(hit.transform.gameObject);
            }
            if(hit.transform.tag == "target")
            {
                ClientChangeColor(hit.transform.gameObject);
            }
        }
    }
    [ClientRpc]
    private void spawnBulletTrail(GameObject bulletTrailInstance, Vector3 _gunBarrelPos, Vector3 hitPoint)
    {
        LineRenderer lineR = bulletTrailInstance.GetComponent<LineRenderer>();
        lineR.SetPosition(0, _gunBarrelPos);
        lineR.SetPosition(1, hitPoint);
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
    [ClientRpc]
    private void ClientChangeColor(GameObject target)
    {
        if(target.tag == "target")
        {
            target.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            MeshRenderer targetMeshRenderer = target.transform.Find("playerModel").GetComponent<MeshRenderer>();
            Color playerStartColor = targetMeshRenderer.material.color;
            targetMeshRenderer.material.color = Color.red;
        }
    }
}
