using UnityEngine;

public class ExplosionAuraBlast : MonoBehaviour
{
    public GameObject bulletSplash;
    public EnemyTag enemyTag;
    public float auraGrowSpeed;
    public float auraGrowMaxSize;

    public void Start()
    {
        this.transform.localScale = Vector2.zero;
    }

    public void Update()
    {
        if (this.transform.localScale.x < auraGrowMaxSize)
        {
            this.transform.localScale = new Vector2(this.transform.localScale.x + (auraGrowSpeed * Time.deltaTime), this.transform.localScale.y + (auraGrowSpeed * Time.deltaTime));
        }
        else if (this.transform.localScale.x > auraGrowMaxSize)
        {
            this.transform.localScale = new Vector2(auraGrowMaxSize, auraGrowMaxSize);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null)
        {
            Instantiate(bulletSplash, col.transform.position, Quaternion.identity);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null)
        {
            Instantiate(bulletSplash, col.transform.position, Quaternion.identity);
        }
    }
}
