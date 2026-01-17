using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase1D2_InsertDespensaArmarioCozinhaStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    private const int ID_ARMARIO_COZINHA = 102;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "INSERT";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateInsertDespensaArmario);
    }

    private bool ValidateInsertDespensaArmario(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

      
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var m = Regex.Match(
            cleaned,
            @"^\s*insert\s+into\s+itens\s*\(\s*idmovel\s*,\s*nomeitem\s*,\s*tipo\s*,\s*quantidade\s*,\s*unidade\s*,\s*status\s*\)\s*values\s*(.+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        if (!m.Success) return false;

        string valuesPart = m.Groups[1].Value.Trim();

        
        var tupleMatches = Regex.Matches(valuesPart, @"\(\s*[^()]*\)");
        if (tupleMatches.Count != 2) return false;

        string remainder = Regex.Replace(valuesPart, @"\(\s*[^()]*\)", "");
        remainder = Regex.Replace(remainder, @"[\s,]+", "");
        if (remainder.Length != 0) return false;

        
        var got = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match tm in tupleMatches)
        {
            string tuple = tm.Value.Trim();
            if (!tuple.StartsWith("(") || !tuple.EndsWith(")")) return false;

            string inner = tuple.Substring(1, tuple.Length - 2).Trim();

            var parts = inner.Split(',')
                             .Select(p => NormalizeSpaces(p))
                             .ToList();

            if (parts.Count != 6) return false;

            
            if (!IsIntToken(parts[0], ID_ARMARIO_COZINHA)) return false;

            string nome = ExtractStringToken(parts[1]);
            string tipo = ExtractStringToken(parts[2]);
            if (nome == null || tipo == null) return false;

            
            if (!TryParseIntToken(parts[3], out int qtd)) return false;

            string unidade = ExtractStringToken(parts[4]);
            string status = ExtractStringToken(parts[5]);
            if (unidade == null || status == null) return false;

            string key = $"{ID_ARMARIO_COZINHA}|{nome.ToLowerInvariant()}|{tipo.ToUpperInvariant()}|{qtd}|{unidade.ToLowerInvariant()}|{status.ToUpperInvariant()}";
            got.Add(key);
        }

      
        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            $"{ID_ARMARIO_COZINHA}|guardanapo|ESTOQUE|200|un|OK",
            $"{ID_ARMARIO_COZINHA}|copo descartavel|ESTOQUE|100|un|OK"
        };

        return expected.SetEquals(got);
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

    
    private static bool IsIntToken(string token, int expected)
    {
        if (!TryParseIntToken(token, out int v)) return false;
        return v == expected;
    }

    private static bool TryParseIntToken(string token, out int value)
    {
        value = 0;
        token = NormalizeSpaces(token);

        if ((token.StartsWith("'") && token.EndsWith("'")) || (token.StartsWith("\"") && token.EndsWith("\"")))
        {
            token = token.Substring(1, token.Length - 2).Trim();
        }

        return int.TryParse(token, out value);
    }

    
    private static string ExtractStringToken(string token)
    {
        token = NormalizeSpaces(token);

        if (token.Length < 2) return null;

        if (token.StartsWith("'") && token.EndsWith("'"))
            return token.Substring(1, token.Length - 2);

        if (token.StartsWith("\"") && token.EndsWith("\""))
            return token.Substring(1, token.Length - 2);

        return null;
    }
}
