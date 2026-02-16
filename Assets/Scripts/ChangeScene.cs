using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int sceneIndex;

    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float animationSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    public void ChangeAndPopulateInventory()
    {
        if (BuildManager.Instance != null)
        {
            foreach (Node n in BuildManager.Instance.allActiveNodes)
            {
                if (n.Type == NodeType.Center) continue;
                switch (n.NodeColor)
                {
                    case NodeColor.Red: InventoryManager.Instance.redNodesOwned++; break;
                    case NodeColor.Blue: InventoryManager.Instance.blueNodesOwned++; break;
                    case NodeColor.Green: InventoryManager.Instance.greenNodesOwned++; break;
                    case NodeColor.Purple: InventoryManager.Instance.purpleNodesOwned++; break;
                    case NodeColor.Yellow: InventoryManager.Instance.yellowNodesOwned++; break;
                    case NodeColor.Orange: InventoryManager.Instance.orangeNodesOwned++; break;
                }
            }
        }

        SceneManager.LoadScene(sceneIndex);
    }

    public void GoBackToNodes()
    {
        if (BuildInfo.Instance != null)
        {
            Destroy(BuildInfo.Instance.gameObject);
            InventoryManager.Instance.currentLevel++;
        }
        SceneManager.LoadScene(sceneIndex);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(3);
    }

    public void ActuallyStartGame()
    {
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}