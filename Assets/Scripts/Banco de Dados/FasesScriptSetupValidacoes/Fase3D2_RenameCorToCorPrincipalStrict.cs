// Fase 3 — Desafio 2
// Gabarito (texto exato, flexível em espaços/maiusc.):
// ALTER TABLE brinquedos RENAME COLUMN cor TO cor_principal;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase3D2_RenameCorToCorPrincipalStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        
        if (!furniture) furniture = GetComponent<FurnitureInteractable>();
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        
        furniture.allowedOp = "ALTER";
        furniture.successMessage = "Coluna cor_secundaria adicionada com sucesso!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        
        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateAddCorSecundaria);
    }

    private bool ValidateAddCorSecundaria(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;
        if (db == null) return false;

        
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline); 
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var rx = new Regex(
            @"^alter\s+table\s+brinquedos\s+add\s+column\s+cor_secundaria\s+text$",
            RegexOptions.IgnoreCase
        );

        if (!rx.IsMatch(cleaned))
            return false;

        
        var cols = db.GetTableInfo("brinquedos");
        if (cols == null || cols.Count == 0) return false;

        var col = cols.FirstOrDefault(c =>
            c.name.Equals("cor_secundaria", StringComparison.OrdinalIgnoreCase));

        if (col == null) return false;

        
        if (!string.Equals(col.type, "TEXT", StringComparison.OrdinalIgnoreCase))
            return false;

        
        if (col.notnull != 0)
            return false;

       
        if (!string.IsNullOrEmpty(col.dflt_value))
            return false;

        return true;
    }

    private static string StripComments(string s)
    {
        
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return s;
    }
}
