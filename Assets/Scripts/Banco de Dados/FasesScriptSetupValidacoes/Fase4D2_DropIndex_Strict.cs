using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase4D2_DropIndex_Strict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "DROP";
        furniture.successMessage = "Índice removido com sucesso!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateDropUxCxCn);
    }

    private bool ValidateDropUxCxCn(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // 1) STRICT do comando
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        var rx = new Regex(@"^drop\s+index\s+if\s+exists\s+ux_cx_cn$", RegexOptions.IgnoreCase);
        if (!rx.IsMatch(cleaned)) return false;

        // 2) PRAGMA/sqlite_master - garantir que o índice não exista mais
        // (checamos por nome direto no master porque GetUniqueIndexes pega só únicos por tabela)
        var rows = db.ExecuteQuery<_NameRow>(
            "SELECT name FROM sqlite_master WHERE type='index' AND lower(name)='ux_cx_cn';"
        );
        if (rows.Any()) return false; // ainda existe? inválido

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
