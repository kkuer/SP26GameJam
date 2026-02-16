using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PickupLogic : MonoBehaviour
{
    public Color r, y, b, g, p, o;
    public enum MyType { Red, Blue, Green, Yellow, Purple, Orange}
    public MyType type;

    private void OnEnable()
    {
        var sr = GetComponent<SpriteRenderer>();
        int randomIndex = Random.Range(0, 6);

        switch (randomIndex)
        {
            case 0:
                type = MyType.Red;
                sr.color = r;
                break;
            case 1:
                type = MyType.Blue;
                sr.color = b;
                break;
            case 2:
                type = MyType.Green;
                sr.color = g;
                break;
            case 3:
                type = MyType.Yellow;
                sr.color = y;
                break;
            case 4:
                type = MyType.Purple;
                sr.color = p;
                break;
            case 5:
                type = MyType.Orange;
                sr.color = o;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.GetComponent<HomunManager>())
        {
            if(HomunManager.instance.canPickupMoreStuff)
            {
                //Collect Pickup
                switch (type)
                {
                    case MyType.Red: InventoryManager.Instance.redNodesOwned++; break;
                    case MyType.Blue: InventoryManager.Instance.blueNodesOwned++; break;
                    case MyType.Green: InventoryManager.Instance.greenNodesOwned++; break;
                    case MyType.Purple: InventoryManager.Instance.purpleNodesOwned++; break;
                    case MyType.Yellow: InventoryManager.Instance.yellowNodesOwned++; break;
                    case MyType.Orange: InventoryManager.Instance.orangeNodesOwned++; break;
                }
                HomunManager.instance.CountInventory();
            }
            
        }
        Destroy(gameObject);
    }
}
