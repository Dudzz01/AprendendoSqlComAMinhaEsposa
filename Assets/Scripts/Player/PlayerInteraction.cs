using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuração de Interação")]
    [SerializeField] float interactRadius = 2f;
    [SerializeField] KeyCode interactKey = KeyCode.LeftControl;

    [Header("Referências de UI")]
    [SerializeField] GameObject promptUI;            // painel que contém o texto
    [SerializeField] TextMeshProUGUI promptText;     // texto do prompt
    [SerializeField] Button closeConsoleButton;      // botão opcional para fechar o console

    [Header("Modo Blocos (opcional)")]
    [SerializeField] private QueryBuilderUI queryBuilderUI; // se usar builder, só setamos os tokens

    private FurnitureInteractable nearestFI;
    private bool consoleOpen = false;

    void Start()
    {
        if (closeConsoleButton != null)
            closeConsoleButton.onClick.AddListener(CloseConsole);

        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        // Descobre o móvel mais próximo dentro do raio
        var all = FindObjectsOfType<FurnitureInteractable>();
        var inRange = all
            .Select(fi => new { fi, dist = Vector3.Distance(transform.position, fi.transform.position) })
            .Where(x => x.dist <= interactRadius)
            .OrderBy(x => x.dist)
            .FirstOrDefault();

        if (inRange != null)
        {
            nearestFI = inRange.fi;

            // Mostra o prompt quando o console não está aberto
            if (!consoleOpen && promptUI != null && promptText != null)
            {
                promptUI.SetActive(true);
                promptText.text = $"Pressione CTRL para acessar {nearestFI.movableName}";
            }

            // Interagir
            if (Input.GetKeyDown(interactKey))
            {
                if (promptUI != null) promptUI.SetActive(false);
                consoleOpen = true;

                // Se usar builder de blocos, só atualiza o pool de tokens (sem abrir fluxo legado)
                if (queryBuilderUI != null && nearestFI.tokens != null)
                    queryBuilderUI.availableTokens = nearestFI.tokens;

                // Novo fluxo: deixa o próprio móvel abrir o console com sua sessão (AllowedOp/Validator/mensagem/etc.)
                nearestFI.Interact();

                // Se você ainda usa um gerenciador de enunciado externo, mantenha esta linha:
                // (ignora se não existir)
                try { EnunciadoUIManager.I?.Show(nearestFI.textoEnunciado); } catch { /* opcional */ }
            }
        }
        else
        {
            nearestFI = null;
            if (promptUI != null) promptUI.SetActive(false);
        }

        // ESC fecha o console
        if (Input.GetKeyDown(KeyCode.Escape) && consoleOpen)
            CloseConsole();
    }

    // Chamado quando o console é fechado "por fora" (ex.: sucesso customizado)
    public void NotifyConsoleClosedExternally()
    {
        consoleOpen = false;

        // Se ainda estou perto do mesmo móvel, volta a mostrar o prompt
        if (nearestFI != null &&
            Vector3.Distance(transform.position, nearestFI.transform.position) <= interactRadius)
        {
            if (promptUI != null && promptText != null)
            {
                promptUI.SetActive(true);
                promptText.text = $"Pressione CTRL para acessar {nearestFI.movableName}";
            }
        }
        else
        {
            if (promptUI != null) promptUI.SetActive(false);
        }
    }


    private void CloseConsole()
    {
        // Fecha o builder (se estiver aberto)
        if (queryBuilderUI != null && queryBuilderUI.gameObject.activeSelf)
            queryBuilderUI.OnClose();

        // Fecha o console SQL da fase atual (via móvel mais próximo, se disponível)
        if (nearestFI != null && nearestFI.sqlUI != null)
            nearestFI.sqlUI.Close();

        consoleOpen = false;

        // Reexibe o prompt se ainda estiver dentro do raio
        if (nearestFI != null &&
            Vector3.Distance(transform.position, nearestFI.transform.position) <= interactRadius)
        {
            if (promptUI != null && promptText != null)
            {
                promptUI.SetActive(true);
                promptText.text = $"Pressione CTRL para acessar {nearestFI.movableName}";
            }
        }
        else
        {
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
