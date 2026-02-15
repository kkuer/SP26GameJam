using UnityEngine;

public class SwordPrefab : MonoBehaviour
{
    public GameObject bulletSplash;
    public EnemyTag enemyTag;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnCollisionEnter2D(Collision2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null)
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        }

    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null)
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        }
    }
}
