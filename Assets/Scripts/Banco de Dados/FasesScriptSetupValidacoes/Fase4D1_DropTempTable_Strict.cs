using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase4D1_DropTempTable_Strict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "DROP";
        furniture.successMessage = "Tabela removida com sucesso!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateDropCaixasVazias);
    }

    private bool ValidateDropCaixasVazias(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // 1) STRICT do comando
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        var rx = new Regex(@"^drop\s+table\s+if\s+exists\s+caixasvazias$", RegexOptions.IgnoreCase);
        if (!rx.IsMatch(cleaned)) return false;

        // 2) PRAGMA/sqlite_master - garantir que NÃO exista mais nenhuma tabela com esse nome
        // (funciona tanto p/ temp quanto p/ main na mesma conexão)
        var info = db.GetTableInfo("CAIXASVAZIAS");
        if (info != null && info.Count > 0) return false; // ainda existe? inválido

        // extra: checar no sqlite_master também
        var rows = db.ExecuteQuery<_NameRow>(
            "SELECT name FROM sqlite_master WHERE type IN ('table','view') AND lower(name)='caixasvazias';"
        );
        if (rows.Any()) return false;

        return true;
    }

    private static string StripComments(string s)
    {
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return s;
    }

    private class _NameRow { public string name { get; set; } }
}
