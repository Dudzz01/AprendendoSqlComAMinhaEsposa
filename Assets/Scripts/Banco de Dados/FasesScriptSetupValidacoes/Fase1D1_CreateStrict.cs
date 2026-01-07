using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase1D1_CreateStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "CREATE";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = false;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateCreateExactAnyOrder);
    }

    
    private bool ValidateCreateExactAnyOrder(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        
        string cleaned = StripComments(sql);
        cleaned = cleaned.Trim();
        var semicolonTrim = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        var lower = semicolonTrim.ToLowerInvariant();

        
        if (Regex.IsMatch(lower, @"\b(autoincrement|foreign\s+key|constraint|unique|check|primary\s+key\s*\()", RegexOptions.IgnoreCase))
            return false;

        
        var m = Regex.Match(lower, @"^\s*create\s+table\s+brinquedos\s*\((.*)\)\s*$",
                            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!m.Success) return false;

        string body = m.Groups[1].Value;

        
        var defs = body.Split(',')
                       .Select(s => NormalizeSpaces(s))
                       .Select(s => s.Trim())
                       .Where(s => s.Length > 0)
                       .ToList();

        
        if (defs.Count != 8) return false;

        
        var expected = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase)
        {
            
            ["id"] = new Regex(@"^id\s+integer\s+primary\s+key$", RegexOptions.IgnoreCase),
            ["nome"] = new Regex(@"^nome\s+text\s+not\s+null$", RegexOptions.IgnoreCase),
            ["categoria"] = new Regex(@"^categoria\s+text\s+not\s+null$", RegexOptions.IgnoreCase),
            ["cor"] = new Regex(@"^cor\s+text$", RegexOptions.IgnoreCase),
            ["material"] = new Regex(@"^material\s+text$", RegexOptions.IgnoreCase),
            ["idade_recomendada"] = new Regex(@"^idade_recomendada\s+integer$", RegexOptions.IgnoreCase),
            ["data_cadastro"] = new Regex(@"^data_cadastro\s+text\s+default\s+current_date$", RegexOptions.IgnoreCase),
            ["ativo"] = new Regex(@"^ativo\s+integer\s+default\s+1$", RegexOptions.IgnoreCase),
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

        bool isValidated = remaining.Count == 0;

   


        return isValidated;
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
