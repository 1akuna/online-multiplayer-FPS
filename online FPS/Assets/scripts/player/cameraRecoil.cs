using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRecoil : MonoBehaviour
{
    [SerializeField]private weapons weaponsScript;
    [Header("Recoil Settings: ")]
    [SerializeField]private float rotationSpeed = 6;
    [SerializeField]private float retrunSpeed = 25;

    [Header("hipFire: ")]
    [SerializeField] private Vector3 RecoilRotation = new Vector3(1f, 1f, 1f);

    [Header("Aiming: ")]
    [SerializeField]private Vector3 RecoilRotationAiming = new Vector3(0.5f, 0.5f, 0.5f);


    private bool aiming;
    private Vector3 currentRotation;
    private Vector3 Rot;

    private void FixedUpdate() 
    {
        if(!weaponsScript.thisIsLocalPlayer){return;}
        currentRotation = Vector3.Lerp(currentRotation, Vector3.zero, retrunSpeed * Time.deltaTime);
        Rot = Vector3.Slerp(Rot, currentRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(Rot);
    }

    private void Update() 
    {
        if(!weaponsScript.thisIsLocalPlayer){return;}
        if(Input.GetButton("Fire1") && weaponsScript.bulletCount > 0 && weaponsScript.isReloading == false)
        {
            Fire();
        }
        if(Input.GetButton("Fire2"))
        {
            aiming = true;
        }
        else
        {
            aiming = false;
        }
    }

    private void Fire()
    {
        if(aiming)
        {
            currentRotation += new Vector3(-RecoilRotationAiming.x, Random.Range(-RecoilRotationAiming.y, RecoilRotationAiming.y), Random.Range(-RecoilRotationAiming.z, RecoilRotationAiming.z));
        }
        else
        {
            currentRotation += new Vector3(-RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
        }
    }
}
