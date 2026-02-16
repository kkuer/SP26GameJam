using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HomunVisuals : MonoBehaviour
{
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (InventoryManager.Instance.currentLevel < 3)
        {
            animator.Play("Level1");
        }
        else if (InventoryManager.Instance.currentLevel < 6)
        {
            animator.Play("Level2");
        }
        else
        {
            //animator.Play("Level3");
        }
    }
}
