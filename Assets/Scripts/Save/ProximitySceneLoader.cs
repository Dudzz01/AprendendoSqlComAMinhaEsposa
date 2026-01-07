using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProximitySceneLoader : MonoBehaviour
{
    [Header("Configuração da Cena")]
    public string sceneName;

    [Header("Proximidade")]
    public Transform playerTransform;
    public float proximityDistance = 1.5f;

    [Header("Interação")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI (opcional)")]
    public GameObject promptUI;
    public GameObject promptUIErro;

    [Header("BotãoFecharErro")]
    public Button botaoFecharErro;

    [Header("TextoErro")]
    public TMPro.TextMeshProUGUI textoErro;

    [Header("QuantidadeDesafiosConcluidosRequisito")]

    public int quantidadeDesafiosConcluidosRequisito;

 

    bool inRange = false;

    void Start()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else Debug.LogError("ProximitySceneLoader: não encontrou tag 'Player'");
        }

        if (promptUI == null)
            Debug.LogWarning("ProximitySceneLoader: promptUI não atribuído");
        else
            promptUI.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null) return;

      
        Vector2 pPos = new Vector2(playerTransform.position.x,
                                   playerTransform.position.y);
        Vector2 tPos = new Vector2(transform.position.x,
                                   transform.position.y);

       
        float dist = Vector2.Distance(pPos, tPos);
        

        if(botaoFecharErro.enabled)
        {
            botaoFecharErro.onClick.AddListener(() =>
            {
                promptUIErro.SetActive(false);
                playerTransform.gameObject.GetComponent<Player>().enabled = true;
                promptUI.SetActive(true);
            });
        }

        bool nowInRange = dist <= proximityDistance;
        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (promptUI != null)
                promptUI.SetActive(inRange);
        }

        

            if (inRange && Input.GetKeyDown(interactKey) && GameController.s.desafiosConcluidos >= quantidadeDesafiosConcluidosRequisito)
            {
                SceneManager.LoadScene(sceneName);
            }
            else if(inRange && Input.GetKeyDown(interactKey) && GameController.s.desafiosConcluidos < quantidadeDesafiosConcluidosRequisito)
            {

                promptUI.SetActive(false);
                promptUIErro.SetActive(true);
                textoErro.text = "Não está permitido avançar para a próxima fase.\n\nChaves Encontradas: " + GameController.s.desafiosConcluidos + "/" + quantidadeDesafiosConcluidosRequisito;
                playerTransform.gameObject.GetComponent<Player>().enabled = false;
                playerTransform.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }

        
        

    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
    }
}
