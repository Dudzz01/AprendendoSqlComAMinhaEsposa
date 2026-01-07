using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase2D1_CreateCaixasStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "CREATE";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        
        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateCreateCaixasExactAnyOrder);
    }

    private bool ValidateCreateCaixasExactAnyOrder(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var m = Regex.Match(
            cleaned,
            @"^\s*create\s+table\s+caixas\s*\((.*)\)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        if (!m.Success) return false;

        string body = m.Groups[1].Value;

        
        var defs = body
            .Split(',')
            .Select(s => NormalizeSpaces(s))
            .Where(s => s.Length > 0)
            .ToList();

        
        if (defs.Count != 7) return false;

        
        var fkItems = defs.Where(d => Regex.IsMatch(d, @"^foreign\s+key\b", RegexOptions.IgnoreCase)).ToList();
        if (fkItems.Count != 1) return false;

        var fk = fkItems[0];
        var fkOk = Regex.IsMatch(
            fk,
            @"^foreign\s+key\s*\(\s*casa_id\s*\)\s+references\s+casas\s*\(\s*idcasa\s*\)\s+on\s+delete\s+cascade$",
            RegexOptions.IgnoreCase
        );
        if (!fkOk) return false;

        
        defs.Remove(fk);

        
        var expected = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = new Regex(@"^id\s+integer\s+primary\s+key$", RegexOptions.IgnoreCase),
            ["nome"] = new Regex(@"^nome\s+text\s+not\s+null$", RegexOptions.IgnoreCase),
            ["local"] = new Regex(@"^local\s+text$", RegexOptions.IgnoreCase),
            ["capacidade"] = new Regex(@"^capacidade\s+integer\s+default\s+(?:10|'10')$", RegexOptions.IgnoreCase),
            ["ativa"] = new Regex(@"^ativa\s+integer\s+default\s+(?:1|'1')$", RegexOptions.IgnoreCase),
            ["casa_id"] = new Regex(@"^casa_id\s+integer\s+not\s+null$", RegexOptions.IgnoreCase),
        };

        var remaining = new HashSet<string>(expected.Keys, StringComparer.OrdinalIgnoreCase);

        foreach (var def in defs)
        {
            
            var mm = Regex.Match(def, @"^([a-z_][a-z0-9_]*)\b", RegexOptions.IgnoreCase);
            if (!mm.Success) return false;
            var col = mm.Groups[1].Value;

            
            if (!expected.ContainsKey(col)) return false;

            
            if (!expected[col].IsMatch(def)) return false;

            remaining.Remove(col);
        }

        
        return remaining.Count == 0;
    }

    
    private static string StripComments(string s)
    {
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);      
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);  
        return s;
    }

    private static string NormalizeSpaces(string s)
    {
        s = Regex.Replace(s, @"\s+", " ");
        return s.Trim();
    }
}
