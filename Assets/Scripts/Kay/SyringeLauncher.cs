using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SyringeLauncher : MonoBehaviour
{
    Vector2 mousePos;
    Camera cam;

    public GameObject HomunculousPrefab;

    //public float defaultEulerRot;

    [SerializeField]private float detectLength = 2f;
    [SerializeField] private float launchDistance = 2f;

    private void Start()
    {
        cam = Camera.main;
    }

    public bool CanLaunch { get; private set; } = true;

    private void Update()
    {
        // detect mouse position
        // pivot around transform pivot point 
        PivotWithMouse();
        // raycast (if hit the petri border, dont allow to rotate further) (MAKE SURE PIVOT IS NOT IN THE BOUNDS OF THE BORDER!!!)

        // launch with lmb
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (CanLaunch)
            {
                Launch();
            }
        }
    }

    private void PivotWithMouse()
    {
        // get directional vector from mouse position to pivot
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 dir = new Vector2(transform.position.x, transform.position.y) - mousePos;
        Vector2 dirNorm = dir.normalized;

        Debug.DrawRay(transform.position, dirNorm * detectLength, Color.red);
        DetectBorders(dirNorm);
    }
    private void DetectBorders(Vector2 dirNorm)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirNorm, detectLength);
        if (hit.collider != null)
        {
            if (hit.collider.GetComponent<PetriTag>())
            {
                //CanLaunch = false;
            }
            else
            {
                //CanLaunch = true;
                transform.right = dirNorm;
            }
        }
        else transform.right = dirNorm;
    }

    public void Launch()
    {
        Vector2 spawnPos = this.transform.position + transform.right.normalized * launchDistance; // spawn at syringe tip
        Debug.DrawRay(this.transform.position, this.transform.position * launchDistance, Color.green, 1f);

        HomunMove homun = Instantiate(HomunculousPrefab, spawnPos, Quaternion.identity).GetComponent<HomunMove>();
        homun.startDir = transform.right;

        CanLaunch = false;
        Debug.Log("Launch!");

        gameObject.SetActive(false);
    }
}
