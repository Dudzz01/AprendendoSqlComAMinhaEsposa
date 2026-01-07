using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using UnityEngine.Events;

public class SQLConsoleUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text feedbackText;
    private bool _isExecuting;
    [Header("Resultado (painel separado)")]
    [SerializeField] private GameObject resultsPanel;     // arraste o GameObject "Results"
    [SerializeField] private TMP_Text resultsText;      // arraste o TMP dentro do Results
    [SerializeField] private bool closeConsoleOnSuccess = true; // fecha o console quando mostrar resultado

    // -------------------------
    // Sessão por fase/móvel
    // -------------------------
    public struct PhaseSession
    {
        public string AllowedOp;            // "CREATE" | "INSERT" | "UPDATE" | "DELETE" | "ALTER" | "DROP"
        public Delegate Validator;          // (sql, affected, db) | (sql, affected) | (affected)
        public string SuccessMessage;       // ex.: "Parabéns, você concluiu o desafio!"
        public float CloseDelaySeconds;     // ex.: 1.0f
        public bool AutoCloseOnSuccess;     // ex.: true
        public string[] AllowedTables;     // legado (não usado aqui)
        public int ChallengeIndex;
        public bool UseCustomSuccess;       // se true, não mostra mensagem/Results; invoca evento
        public UnityEvent SuccessEvent;     // evento a invocar no sucesso
    }

    private class _Row { public string name { get; set; } }

    private PhaseSession _session;
    private DatabaseManager db;
    private GameObject playerObj;
    private GameObject inputArea;

    void Start()
    {
        db = FindObjectOfType<DatabaseManager>();
        if (panel) panel.SetActive(false);
    }

    // Mantido por compat (QueryBuilderUI/Furniture antigos ainda podem chamar)
    public void Open(string[] tables, Delegate validator)
    {
        OpenPhase(new PhaseSession
        {
            AllowedOp = null,
            Validator = validator,
            SuccessMessage = "Parabéns, você concluiu o desafio!",
            CloseDelaySeconds = 1.0f,
            AutoCloseOnSuccess = false,
            AllowedTables = tables,
            ChallengeIndex = -1

        });
    }

    // NOVO: abertura com sessão (use este no novo jogo)
    public void OpenPhase(PhaseSession session)
    {
        _session = session;

        if (feedbackText) feedbackText.text = "";
        if (inputField) { inputField.text = ""; inputField.ActivateInputField(); }

        if (resultsPanel)
        {
            resultsPanel.SetActive(false);
          
        }  
           

        if (panel)
        {
            panel.SetActive(true);
            var img = panel.GetComponent<Image>();
            if (img) img.enabled = true;

            var t = panel.transform.Find("InputArea");
            if (t) { inputArea = t.gameObject; inputArea.SetActive(true); }
        }

        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) playerObj.GetComponent<Player>().enabled = false;
    }

    public void Close()
    {
        if (inputArea) inputArea.SetActive(false);
        if (panel)
        {
            var img = panel.GetComponent<Image>();
            if (img) img.enabled = false;
            panel.SetActive(false);
        }
        if (playerObj) playerObj.GetComponent<Player>().enabled = true;

        var qb = FindObjectOfType<QueryBuilderUI>();
        if (qb) qb.Hide();

        

    }

    private IEnumerator CloseAfterDelay(float secs)
    {
        yield return new WaitForSeconds(secs);
        Close();
    }

    public void OnExecute()
    {
        string sql = (inputField != null ? inputField.text : "").Trim();
        if (string.IsNullOrWhiteSpace(sql))
        {
            if (feedbackText) feedbackText.text = "Digite um comando SQL.";
            return;
        }

        // Normaliza e detecta operação
        string norm = Regex.Replace(sql, @"\s+", " ").Trim();
        norm = Regex.Replace(norm, @"\s*\.\s*", ".");
        var mOp = Regex.Match(norm, @"^\s*(\w+)", RegexOptions.IgnoreCase);
        string op = mOp.Success ? mOp.Groups[1].Value.ToUpperInvariant() : "";

        // Checa operação da fase (se definida)
        if (!string.IsNullOrEmpty(_session.AllowedOp) &&
            !string.Equals(op, _session.AllowedOp, StringComparison.OrdinalIgnoreCase))
        {
            if (feedbackText) feedbackText.text = $"Esta fase aceita apenas {_session.AllowedOp}.";
            return;
        }

        // Sem SELECT no novo jogo
        if (op == "SELECT")
        {
            if (feedbackText) feedbackText.text = "Esta fase não usa SELECT.";
            return;
        }

        // DDL/DML suportadas
        if (op == "CREATE" || op == "INSERT" || op == "UPDATE" || op == "DELETE" || op == "ALTER" || op == "DROP")
        {
            // --- Transação ao redor do comando ---
            try
            {
                var tables = db.ExecuteQuery<_Row>(
                    "SELECT name FROM sqlite_master WHERE type='table'"
                );
                foreach (var t in tables)
                    Debug.Log("[DB] Tabela existente: " + t.name);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Falha ao listar tabelas: " + e.Message);
            }

            if (_isExecuting) return;
            _isExecuting = true;
            try
            {
                if (db == null) throw new InvalidOperationException("DatabaseManager não encontrado.");

                db.BeginTransaction(); // <--- começa a transação

                // Executa o comando
                int affected = db.ExecuteNonQuery(sql);

                // Validação (pode inspecionar o estado transacional)
                bool ok = true;
                if (_session.Validator != null)
                {
                    int p = _session.Validator.Method.GetParameters().Length;
                    ok = p == 3 ? (bool)_session.Validator.DynamicInvoke(sql, affected, db)
                         : p == 2 ? (bool)_session.Validator.DynamicInvoke(sql, affected)
                                  : (bool)_session.Validator.DynamicInvoke(affected);
                }

                if (ok)
                {
                    db.Commit(); // <--- persiste mudanças

                    if (_session.ChallengeIndex >= 0)
                    {
                        try { GameController.MarkChallengeComplete(_session.ChallengeIndex); }
                        catch (Exception e) { Debug.LogWarning($"Falha ao salvar progresso: {e.Message}"); }
                    }

                    // Se houver ação customizada, dispare-a e NÃO mostre o painel de resultado
                    if (_session.UseCustomSuccess && _session.SuccessEvent != null)
                    {
                        if (closeConsoleOnSuccess) Close(); // fecha o console (Query Builder/Console)

                        var pi = FindObjectOfType<PlayerInteraction>();
                        if (pi != null)
                        {
                            pi.NotifyConsoleClosedExternally();
                        }

                        try { _session.SuccessEvent.Invoke(); }
                        catch (Exception evEx) { Debug.LogWarning($"Falha ao executar ação customizada: {evEx.Message}"); }

                        
                    }
                    else
                    {
                        // fluxo padrão (Results/feedback)
                        string msg = !string.IsNullOrEmpty(_session.SuccessMessage)
                            ? _session.SuccessMessage
                            : "Parabéns, você concluiu o desafio!";

                        if (resultsPanel)
                        {
                            if (resultsText) resultsText.text = msg;
                            resultsPanel.SetActive(true);

                            if (closeConsoleOnSuccess) Close(); // fecha o console; Results fica aberto
                        }
                        else
                        {
                            if (feedbackText) feedbackText.text = msg;
                            if (_session.AutoCloseOnSuccess)
                                StartCoroutine(CloseAfterDelay(_session.CloseDelaySeconds > 0 ? _session.CloseDelaySeconds : 1f));
                        }
                    }
                }
                else
                {
                    db.Rollback(); // <--- desfaz tudo se inválido
                    if (feedbackText)
                        feedbackText.text = "Resposta incorreta, revise o código e corrija.";
                }
            }

            catch (TargetInvocationException tie)
            {
                try { db?.Rollback(); } catch { }
                if (feedbackText) feedbackText.text = "Erro: " + tie.InnerException.Message;
                Debug.LogError(tie.InnerException);
            }
            catch (Exception ex)
            {
                try { db?.Rollback(); } catch { }
                if (feedbackText) feedbackText.text = "Erro: " + ex.Message;
                Debug.LogError(ex);
            }
            finally
            {
                _isExecuting = false;
            }
            return;
        }

        
        if (feedbackText) feedbackText.text = "Resposta incorreta, revise o código e corrija.";
    }

    public void ExecuteRaw(string sql)
    {
        if (inputField) inputField.text = sql;
        OnExecute();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (resultsPanel && resultsPanel.activeSelf)
            {
                resultsPanel.SetActive(false);
                return; // não fecha o console aqui
            }

            Close();

        }
            
    }
}
