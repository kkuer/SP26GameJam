using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq.Expressions;
using static UnityEngine.ParticleSystem;
using Unity.VisualScripting;

public class WeaponDistributor : MonoBehaviour
{
    //Spawner rotator
    float rotateAmount;

    //Node emitter point count
    public int multishotCount;

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

    [Header("Attack Speed")]
    public float projectileAttackSpeed = 1;
    public float swordAttackSpeed = 200;
    public float auraAttackSpeed = 10;

    [Header("Modifier Values")]
    public int sipohonCount;
    public int dotCount;
    public int sizeCount;
    public int ricochetCount;

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
    public void BuildEmitters()
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
        rotateAmount = 360 / (1 + multishotCount);

        //Process for standard weapons, excluding aura and thorns
        if (isDuplicable)
        {

            Debug.Log("Start For loop for each emitter");

            for (int i = 0; i < multishotCount + 1; i++)
            {
                this.transform.localEulerAngles = new Vector3(0, 0, this.transform.localEulerAngles.z + rotateAmount);
                spawnedWeapon = Instantiate(weaponNode, spawnPoint.transform.position, this.transform.rotation);
                spawnedWeapon.transform.SetParent(playerGO.transform);
                emitterScript = spawnedWeapon.GetComponent<NormalEmitter>();


                switch (currentWeapon)
                {
                    case WeaponType.Piercer:

                        int pierceIndex = -1;

                        for(int j = 0; j <BuildInfo.Instance.activeAbilities.Count; j++)
                        {
                            if (BuildInfo.Instance.activeAbilities[j].abilityType.Equals(EffectType.PiercingShot))
                            {
                                pierceIndex = j; 
                            }
                        }

                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageToDeal *=
                            BuildInfo.Instance.activeAbilities[pierceIndex].damageMultiplier;

                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageSource = EffectType.PiercingShot;

                        emitterScript.fireRate = (nodeFireRate + 1) * projectileAttackSpeed;
                        break;

                    case WeaponType.Burster:
                        int burstIndex = -1;

                        for (int j = 0; j < BuildInfo.Instance.activeAbilities.Count; j++)
                        {
                            if (BuildInfo.Instance.activeAbilities[j].abilityType.Equals(EffectType.BurstShot))
                            {
                                burstIndex = j;
                            }
                        }
                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageToDeal *=
                            BuildInfo.Instance.activeAbilities[burstIndex].damageMultiplier;

                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageSource = EffectType.BurstShot;

                        emitterScript.fireRate = (nodeFireRate + 1) * projectileAttackSpeed;
                        break;

                    case WeaponType.Exploder:
                        int explodeIndex = -1;

                        for (int j = 0; j < BuildInfo.Instance.activeAbilities.Count; j++)
                        {
                            if (BuildInfo.Instance.activeAbilities[j].abilityType.Equals(EffectType.ExplosiveShot))
                            {
                                explodeIndex = j;
                            }
                        }

                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageToDeal *=
                            BuildInfo.Instance.activeAbilities[explodeIndex].damageMultiplier;

                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageSource = EffectType.ExplosiveShot;

                        emitterScript.fireRate = (nodeFireRate + 1) * projectileAttackSpeed;
                        break;

                    case WeaponType.Sword:
                        int swordIndex = -1;

                        for (int j = 0; j < BuildInfo.Instance.activeAbilities.Count; j++)
                        {
                            if (BuildInfo.Instance.activeAbilities[j].abilityType.Equals(EffectType.MeleeSwipe))
                            {
                                swordIndex = j;
                            }
                        }
                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageToDeal *=
                            BuildInfo.Instance.activeAbilities[swordIndex].damageMultiplier;

                        emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageSource = EffectType.MeleeSwipe;

                        emitterScript.fireRate = (nodeFireRate + 1) * swordAttackSpeed;
                        emitterScript.swordCenter = swordCenter;
                        break;
                }
            }
        }
        else if (!isDuplicable)
        {
            spawnedWeapon = Instantiate(weaponNode, playerGO.transform.position, this.transform.rotation);
            spawnedWeapon.transform.SetParent(playerGO.transform);
            emitterScript = spawnedWeapon.GetComponent<NormalEmitter>();

            switch (currentWeapon)
            {
                case WeaponType.Aura:
                    int auraIndex = -1;

                    for (int j = 0; j < BuildInfo.Instance.activeAbilities.Count; j++)
                    {
                        if (BuildInfo.Instance.activeAbilities[j].abilityType.Equals(EffectType.AuraBurst))
                        {
                            auraIndex = j;
                        }
                    }
                    emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageToDeal *=
                            BuildInfo.Instance.activeAbilities[auraIndex].damageMultiplier;

                    emitterScript.fireRate = (nodeFireRate + 1) * auraAttackSpeed;
                    emitterScript.bulletPrefab.GetComponent<DamageExecutor>().damageSource = EffectType.AuraBurst;
                    break;

                case WeaponType.Thorns:
                    int thornsIndex = -1;

                    for (int j = 0; j < BuildInfo.Instance.activeAbilities.Count; j++)
                    {
                        if (BuildInfo.Instance.activeAbilities[j].abilityType.Equals(EffectType.Thorns))
                        {
                            thornsIndex = j;
                        }
                    }
                    
                    spawnedWeapon.GetComponent<DamageExecutor>().damageToDeal *=
                            BuildInfo.Instance.activeAbilities[thornsIndex].damageMultiplier;
                    spawnedWeapon.GetComponent<DamageExecutor>().damageSource = EffectType.Thorns;
                    break;
            }
            
        }
    }
}
