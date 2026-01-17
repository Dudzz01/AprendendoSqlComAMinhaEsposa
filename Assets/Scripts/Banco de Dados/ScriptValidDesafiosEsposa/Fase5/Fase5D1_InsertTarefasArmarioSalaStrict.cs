using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase5D1_InsertTarefasArmarioSalaStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    private const int ID_ARMARIO_SALA = 204;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "INSERT";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateInsertTarefasArmarioSala);
    }

    private bool ValidateInsertTarefasArmarioSala(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var m = Regex.Match(
            cleaned,
            @"^\s*insert\s+into\s+itens\s*\(\s*idmovel\s*,\s*nomeitem\s*,\s*tipo\s*,\s*status\s*,\s*prioridade\s*\)\s*values\s*(.+)\s*$",
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
                             .Select(NormalizeSpaces)
                             .Where(p => p.Length > 0)
                             .ToList();

            if (parts.Count != 5) return false;

            
            if (!IsIntToken(parts[0], ID_ARMARIO_SALA)) return false;

            string nome = ExtractStringToken(parts[1]);
            string tipo = ExtractStringToken(parts[2]);
            string status = ExtractStringToken(parts[3]);
            if (nome == null || tipo == null || status == null) return false;

            if (!TryParseIntToken(parts[4], out int prioridade)) return false;

            string key = $"{ID_ARMARIO_SALA}|{nome.ToLowerInvariant()}|{tipo.ToUpperInvariant()}|{status.ToUpperInvariant()}|{prioridade}";
            got.Add(key);
        }

        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            $"{ID_ARMARIO_SALA}|organizar o armario|TAREFA|PENDENTE|1",
            $"{ID_ARMARIO_SALA}|separar doacoes|TAREFA|PENDENTE|2"
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
