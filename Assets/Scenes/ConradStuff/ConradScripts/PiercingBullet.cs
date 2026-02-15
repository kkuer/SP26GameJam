using UnityEngine;
using System.Collections;

public class PiercingBullet : MonoBehaviour
{
    //Bullet speed
    public float bullSpeed;
    //Bullet particles
    public GameObject bulletSplash;
    //Bullet lifetime
    public float bulletLife;
    //Enemy tag script, checks for when determining piercing
    EnemyTag enemyTag;
    //Bullet jitter
    //public float bulletDeviation;

    public int ricochetTimes = 5;



    void Start()
    {
        //ricochetTimes = HomunManager.instance.GetRicochetTimes(EffectType.PiercingShot);
        ricochetTimes = 5;
        StartCoroutine(SelfDes());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * bullSpeed);
        //transform.position = new Vector3(transform.position.x + (Random.Range(-bulletDeviation, bulletDeviation)), transform.position.y + (Random.Range(-bulletDeviation, bulletDeviation)), transform.position.z + (Random.Range(-bulletDeviation, bulletDeviation)));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null )
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        }
        else 
        {
            //CHECK FOR RICOCHET HERE
            if(ricochetTimes > 0)
            {
                //if ricochet, then need to bounce from the wall here
                ricochetTimes--;


                //THIS PART ISNT WORKING IDK WHY 
                Vector2 inDirection = this.GetComponent<Rigidbody2D>().linearVelocity;
                Vector2 inNormal = col.contacts[0].normal;
                Vector2 newDirection = Vector2.Reflect(inDirection, inNormal);
                this.GetComponent<Rigidbody2D>().linearVelocity = newDirection;
                return;
            }
            else
            {
                //If no ricochet, then vfx and destroy
                //Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
                Destroy(gameObject);
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null)
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
            //Destroy(gameObject);
        }
    }

    IEnumerator SelfDes()
    {
        yield return new WaitForSeconds(bulletLife);
        Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
