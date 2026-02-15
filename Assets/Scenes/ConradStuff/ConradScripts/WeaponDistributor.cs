using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class WeaponDistributor : MonoBehaviour
{
    //Spawner rotator
    float rotateAmount;
    //Node emitter point count
    public int nodeCount;
    //Node firerate
    public float nodeFireRate;
    //Node damage
    public float nodeDamage;
    //Node to generate/script on node
    public GameObject weaponNode;
    public NormalEmitter emitterScript;
    //Generated node (for parenting purposes)
    GameObject spawnedWeapon;
    //Player gameobject (for parenting purposes)
    public GameObject playerGO;
    //Node spawner point
    public GameObject spawnPoint;
    //Center weapon point, used mainly for passing in for the spinning swords emitters
    public ConradDumbSpin swordCenter;
    //Enum for selecting weapons from the list of weapons
    public enum WeaponType
    {
        Standard,
        Piercer,
        Burster,
        Exploder,
        Sword,
        Aura,
        Thorns
    }
    public WeaponType currentWeapon;
    //Bool to intercept non-multishot weapons
    public bool isDuplicable = true;
    //List of all the potential weapons to spawn
    public List<GameObject> weaponList = new List<GameObject>();

    //Takes stock of how many nodes of the given weapon are asked to spawn, then divides that off a 360 degree total rotation
    //while creating a node in turn, so there's a properly rotated bunch of emitters spread along the center player ball.
    void Start()
    {
        //Selects the weapon generated 
        if (currentWeapon == WeaponType.Standard)
        {
            weaponNode = weaponList[0];
        }
        else if (currentWeapon == WeaponType.Piercer)
        {
            weaponNode = weaponList[1];
        }
        else if (currentWeapon == WeaponType.Burster)
        {
            weaponNode = weaponList[2];
        }
        else if (currentWeapon == WeaponType.Exploder)
        {
            weaponNode = weaponList[3];
        }
        else if (currentWeapon == WeaponType.Sword)
        {
            weaponNode = weaponList[4];
        }
        else if (currentWeapon == WeaponType.Aura)
        {
            weaponNode = weaponList[5];
            isDuplicable = false;
        }
        else if (currentWeapon == WeaponType.Thorns)
        {
            weaponNode = weaponList[6];
            isDuplicable = false;
        }

        //Rotates the emitter relative to the amount of nodes
        rotateAmount = 360/nodeCount;
        //Process for standard weapons, excluding aura and thorns
        if((nodeCount > 0) && (isDuplicable))
        {
            for (int i = 0; i < nodeCount; i++)
            {
                this.transform.localEulerAngles = new Vector3(0,0,this.transform.localEulerAngles.z + rotateAmount);
                spawnedWeapon = Instantiate(weaponNode, spawnPoint.transform.position, this.transform.rotation);
                spawnedWeapon.transform.SetParent(playerGO.transform);
                emitterScript = spawnedWeapon.GetComponent<NormalEmitter>();
                emitterScript.fireRate = nodeFireRate;
                if (currentWeapon == WeaponType.Sword)
                {
                    emitterScript.fireRate = nodeFireRate * 200;
                    emitterScript.swordCenter = swordCenter;
                }

            }
        }
        else if (!isDuplicable)
        {
            spawnedWeapon = Instantiate(weaponNode, playerGO.transform.position, this.transform.rotation);
            spawnedWeapon.transform.SetParent(playerGO.transform);
        }
    }
}
