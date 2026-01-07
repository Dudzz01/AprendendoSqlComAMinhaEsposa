using System;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class FurnitureInteractable : MonoBehaviour
{
    [Header("Identificação")]
    public string movableName;

    [Header("Regra da Fase (deste móvel)")]
    [Tooltip("Operação permitida nesta fase: CREATE | INSERT | UPDATE | DELETE | ALTER | DROP")]
    public string allowedOp = "CREATE";

    [Tooltip("Validador desta fase. Assinatura aceita: (sql, affected, db) | (sql, affected) | (affected)")]
    public Delegate validator;

    [TextArea] public string successMessage = "Parabéns, você concluiu o desafio!";
    [Range(0f, 5f)] public float closeDelaySeconds = 1.0f;
    public bool autoCloseOnSuccess = true;

    [Header("UI / Referências")]
    public SQLConsoleUI sqlUI;                      
    [TextArea] public string textoEnunciado;        
    public TMP_Text enunciadoText;                  

    [Header("Blocos SQL (opcional, para builder)")]
    public string[] tokens;                         

    [Header("Builder")]
    [SerializeField] private QueryBuilderUI builderUI;

    [Header("Progresso")]
    [Tooltip("Índice de save quantidadesDesafiosConcluidos")]
    public int challengeIndexInSave = -1;

    [Header("Ação opcional ao concluir com sucesso")]
    public bool useCustomSuccessAction = false;
    public UnityEvent onSuccess; // configure no Inspector se quiser ação customizada


    public void Interact()
    {
        if (sqlUI == null)
        {
            Debug.LogError($"[{name}] SQLConsoleUI não atribuído.");
            return;
        }

        
        if (enunciadoText != null && !string.IsNullOrWhiteSpace(textoEnunciado))
            enunciadoText.text = textoEnunciado;

       
        var session = new SQLConsoleUI.PhaseSession
        {
            AllowedOp = allowedOp,
            Validator = validator,
            SuccessMessage = string.IsNullOrWhiteSpace(successMessage)
                ? "Parabéns, você concluiu o desafio!"
                : successMessage,
            CloseDelaySeconds = Mathf.Max(0f, closeDelaySeconds),
            AutoCloseOnSuccess = autoCloseOnSuccess,
            AllowedTables = null,
            ChallengeIndex = challengeIndexInSave,
            UseCustomSuccess = useCustomSuccessAction,
            SuccessEvent = onSuccess
        };

        
        sqlUI.OpenPhase(session);

        if (builderUI != null)
            builderUI.Show(tokens); 
    }
}
