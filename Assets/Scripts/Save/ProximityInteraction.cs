using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProximityInteraction : MonoBehaviour
{
    [Header("Proximidade")]
    
    public Transform playerTransform;
    
    public float proximityDistance = 0.5f;

    
    
    public KeyCode interactKey = KeyCode.LeftControl;

    [Header("UI")]
    
    public GameObject promptPanel;
    
    public GameObject infoPanel;
    
    public TextMeshProUGUI infoText;
   
    public Button closeButton;

  
    public string message = "Aqui vai o texto que você quiser.";

   
    private Player playerScript;
    private Rigidbody2D rb2d;

    bool inRange = false;

    void Start()
    {
       
        if (promptPanel != null) promptPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);

       
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseInfoPanel);
        }

       
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        
        if (playerTransform != null)
        {
            playerScript = playerTransform.GetComponent<Player>();
            rb2d = playerTransform.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

      
        Vector2 delta = (Vector2)playerTransform.position - (Vector2)transform.position;
        bool nowInRange = delta.magnitude <= proximityDistance;

        
        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (promptPanel != null && (infoPanel == null || !infoPanel.activeSelf))
                promptPanel.SetActive(inRange);
        }

        if (inRange && Input.GetKeyDown(interactKey))
        {
     
            if (promptPanel != null) promptPanel.SetActive(false);
            if (infoPanel != null) infoPanel.SetActive(true);
            if (infoText != null) infoText.text = message;

       
            if (playerScript != null) playerScript.enabled = false;
            if (rb2d != null) rb2d.velocity = Vector2.zero;
        }
    }

   
    void CloseInfoPanel()
    {
        if (infoPanel != null) infoPanel.SetActive(false);

 
        if (playerScript != null) playerScript.enabled = true;

        if (promptPanel != null && inRange)
            promptPanel.SetActive(true);
    }

  
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
    }
}
