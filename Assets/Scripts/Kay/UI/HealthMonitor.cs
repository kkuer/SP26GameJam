using UnityEngine;
using TMPro;
using UnityEngine.UI;   

public class HealthMonitor : MonoBehaviour
{
    public Image healthImage;

    public Sprite healthy;
    public Sprite damaged;
    public Sprite critical;

    public Color healthyColor;
    public Color damagedColor;
    public Color criticalColor;

    public TMP_Text healthPercentDisplay;

    public HealthTracker playerHealth;

    private void Awake()
    {
        healthImage = GetComponent<Image>();
        healthPercentDisplay = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            if (FindAnyObjectByType<HomunManager>() == null)
            {
                return;
            }
            playerHealth = FindAnyObjectByType<HomunManager>().healthTracker;
        }

        float healthPercent = (playerHealth.currentHealth / playerHealth.maxHealth ) * 100;
        healthPercentDisplay.text = $"{healthPercent}%";

        if (healthPercent > 60)
        {
            if (healthImage.sprite != healthy)
            {
                healthImage.sprite = healthy;
            }
            if (healthImage.color != healthyColor)
            {
                healthImage.color = healthyColor;
            }
        }
        else if (healthPercent > 25)
        {
            if (healthImage.sprite != damaged)
            {
                healthImage.sprite = damaged;
            }
            if (healthImage.color != damagedColor)
            {
                healthImage.color = damagedColor;
            }
        }
        else
        {
            if (healthImage.sprite != critical)
            {
                healthImage.sprite = critical;
            }
            if (healthImage.color != criticalColor)
            {
                healthImage.color = criticalColor;
            }
        }
    }
}
