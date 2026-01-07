using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase5D2_AlterCaixas_AddCorCaixa_Strict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "ALTER";
        furniture.successMessage = "Coluna 'cor_caixa' adicionada corretamente!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateAlterAddCorCaixa);
    }

    private bool ValidateAlterAddCorCaixa(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // 1) STRICT do comando
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        var rx = new Regex(@"^alter\s+table\s+caixas\s+add\s+column\s+cor_caixa\s+text$",
                           RegexOptions.IgnoreCase);
        if (!rx.IsMatch(cleaned)) return false;

        // 2) PRAGMA — garantir que a coluna exista do jeito certo
        var info = db.GetTableInfo("caixas");
        if (info == null || info.Count == 0) return false;

        var col = info.FirstOrDefault(c => c.name.Equals("cor_caixa", StringComparison.OrdinalIgnoreCase));
        if (col == null) return false;
        if (!string.Equals(col.type?.Trim(), "TEXT", StringComparison.OrdinalIgnoreCase)) return false;
        if (col.notnull != 0) return false;      // opcional
        if (!string.IsNullOrWhiteSpace(col.dflt_value)) return false; // sem default

        return true;
    }

    private static string StripComments(string s)
    {
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return s;
    }
}
