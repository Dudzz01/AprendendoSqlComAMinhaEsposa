using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase2D2_Index_UX_Strict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "CREATE";
        furniture.successMessage = "Índice criado corretamente!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateCreateUniqueIndexUx);
    }

    private bool ValidateCreateUniqueIndexUx(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var rx = new Regex(
            @"^create\s+unique\s+index\s+ux_cx_cn\s+on\s+caixas\s*\(\s*casa_id\s*,\s*nome\s*\)$",
            RegexOptions.IgnoreCase
        );
        if (!rx.IsMatch(cleaned)) return false;

        
        var uniques = db.GetUniqueIndexes("caixas");
        var idx = uniques.FirstOrDefault(i => i.name.Equals("ux_cx_cn", StringComparison.OrdinalIgnoreCase));
        if (idx == null) return false;                 
        if (idx.columns.Count != 2) return false;      
        if (!idx.columns[0].Equals("casa_id", StringComparison.OrdinalIgnoreCase)) return false;
        if (!idx.columns[1].Equals("nome", StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }

    private static string StripComments(string s)
    {
        
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return s;
    }
}
