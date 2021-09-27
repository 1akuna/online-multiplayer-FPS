using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class weapons : NetworkBehaviour
{
    [SerializeField]private GameObject bulletTrail;
    [SerializeField]private GameObject bulletHolePrefab;
    [SerializeField]private Transform weaponHolster;
    [SerializeField]private Transform ADStransform;
    [SerializeField]private Transform hipFireTransform;
    [SerializeField]private TMP_Text bulletCountText;
    [SerializeField]private Animator anim;

    [Header("Gun Cusomization: ")]
    [SerializeField]private Transform weapon;
    [SerializeField]private float fireRate;
    [SerializeField]private float bulletsInMag;
    [SerializeField]private float reloadTime;
    [SerializeField]private Transform gunBarrel;
    [SerializeField]private float raycastThickness;

    private AudioSource audioSource;
    private Camera mainCam;
    private playerDeath playerDeathScript;

    public bool isReloading;
    private float nextFire = 0f;
    public float bulletCount = 30f;
    public bool thisIsLocalPlayer;

    private void Start() 
    {
        if(!isLocalPlayer)
        {
            thisIsLocalPlayer = false;
            return;
        }
        thisIsLocalPlayer = true;
        audioSource = GameObject.Find("SFX").GetComponent<AudioSource>();
        playerDeathScript = gameObject.GetComponent<playerDeath>();
        mainCam = Camera.main;
    }

    private void Update() 
    {
        if(!isLocalPlayer){return;}
        if(playerDeathScript.isDead == true){return;}
        bulletCountText.text = bulletCount.ToString();
        weaponADS();
        if(Input.GetKeyDown(KeyCode.R) && bulletCount != 30)
        {
            StartCoroutine(reload());
        }
        if(Input.GetKey(KeyCode.Mouse0) && Time.time > nextFire && bulletCount > 0 && isReloading == false)
        {
            audioSource.Play();
            nextFire = Time.time + fireRate;
            bulletCount--;
            CmdShoot(mainCam.transform.position, mainCam.transform.forward, gunBarrel.position, gunBarrel.rotation, raycastThickness, transform.name);
        }
    }

    private IEnumerator reload()
    {
        anim.SetBool("isReloading", true);
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        anim.SetBool("isReloading", false);
        bulletCount = 30;
        isReloading = false;
    }

    [Command]
    private void CmdShoot(Vector3 _mainCamPos, Vector3 _mainCamDirection, Vector3 _gunBarrelPos, Quaternion _gunBarrelRot, float _raycastThickness, string shooterName)
    {
        if(Physics.SphereCast(_mainCamPos, _raycastThickness, _mainCamDirection, out RaycastHit hit, Mathf.Infinity))
        {
            if(hit.transform.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                //bulletHole
                GameObject bulletHoleInstance = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
                NetworkServer.Spawn(bulletHoleInstance);
                bulletHoleInstance.transform.LookAt(hit.point + hit.normal);
                Destroy(bulletHoleInstance, 3f);
            }
            
            //bulletTrail
            GameObject bulletTrailInstance = Instantiate(bulletTrail, _gunBarrelPos, _gunBarrelRot);
            NetworkServer.Spawn(bulletTrailInstance);
            spawnBulletTrail(bulletTrailInstance, _gunBarrelPos, hit.point);
            Destroy(bulletTrailInstance, 1f);

            //check if player was hit
            if(hit.transform.tag == "remotePlayer" || hit.transform.tag == "localPlayer")
            {
                if(hit.transform.name == shooterName){return;}
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
            //weapon.transform.position = ADStransform.position;
            anim.SetBool("isAiming", true);
        }
        else if(!Input.GetKey(KeyCode.Mouse1))
        {
            //weapon.transform.position = hipFireTransform.position;
            anim.SetBool("isAiming", false);
        }

    }
    [ClientRpc]
    private void ClientChangeColor(GameObject target)
    {
        if(target.tag == "target")
        {
            target.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            StartCoroutine(waitToResetColor(target));
        }
        else
        {
            target.GetComponent<playerDeath>().Die(target.transform);
            MeshRenderer targetMeshRenderer = target.transform.Find("playerModel").GetComponent<MeshRenderer>();
            Color playerStartColor = targetMeshRenderer.material.color;
            targetMeshRenderer.material.color = Color.red;
        }
    }

    private IEnumerator waitToResetColor(GameObject target)
    {
        yield return new WaitForSeconds(2f);
        target.GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
