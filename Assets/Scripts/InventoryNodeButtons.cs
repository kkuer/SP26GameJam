using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryNodeButtons : MonoBehaviour
{
    public Color ownedColor;
    public Color unownedColor;

    public Color iconUnownedColor;

    public Image icon;

    private Image sprite;

    public int amount;
    public TMP_Text amountLabel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sprite = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        amountLabel.text = amount.ToString();

        if (amount > 0)
        {
            sprite.color = ownedColor;
            icon.color = Color.white;
        }
        else
        {
            sprite.color = unownedColor;
            icon.color = iconUnownedColor;
        }
    }
}
