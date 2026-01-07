using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(FurnitureInteractable))]
public class EscravinhaJoinerSetup : MonoBehaviour
{
    const int ExpectedMovelId = 39;
    static readonly string[] RequiredCols = { "NomeItem", "NomeMovel", "NomeComodo", "Dica" };
    static readonly string[] Tables = { "itens", "moveis", "comodos" };

    void Awake()
    {
        var fi = GetComponent<FurnitureInteractable>();
        fi.validator = new Func<string, object, bool>((sql, itemsObj) =>
        {

            var norm = Regex.Replace(sql, "\\s+", " ").Trim();

            norm = Regex.Replace(norm, "\\s*\\.\\s*", ".");

            var m = Regex.Match(norm,
                @"^SELECT\s+(?<cols>.*?)\s+FROM\s+(?<tbl>\w+)",
                RegexOptions.IgnoreCase);
            if (!m.Success) return DebugFail("não encontrou SELECT … FROM");
            string fromTbl = m.Groups["tbl"].Value.ToLower();
            if (!Tables.Contains(fromTbl))
                return DebugFail($"FROM usa tabela inesperada '{fromTbl}'");


            string iAlias = "Itens", mAlias = "Moveis", cAlias = "Comodos";
            var aliasPattern = @"\b(?:FROM|JOIN)\s+(Itens|Moveis|Comodos)(?:\s+AS)?\s+(\w+)\b";
            foreach (Match am in Regex.Matches(norm, aliasPattern, RegexOptions.IgnoreCase))
            {
                var tbl = am.Groups[1].Value.ToLower();
                var alias = am.Groups[2].Value;
                if (tbl == "itens") iAlias = alias;
                if (tbl == "moveis") mAlias = alias;
                if (tbl == "comodos") cAlias = alias;
            }

          
            var cols = m.Groups["cols"].Value
                        .Split(',').Select(c => c.Trim()).ToList();
            if (cols.Count != RequiredCols.Length)
                return DebugFail($"esperava {RequiredCols.Length} colunas, achou {cols.Count}");
            foreach (var req in RequiredCols)
            {
                string prefix;
                string tableName;
                if (req == "Dica")
                {
                    prefix = iAlias;
                    tableName = "Itens";
                }
                else
                {
                    prefix = req == "NomeItem" ? iAlias
                              : req == "NomeMovel" ? mAlias
                              : cAlias;
                    tableName = req == "NomeItem" ? "Itens"
                              : req == "NomeMovel" ? "Moveis"
                              : "Comodos";
                }
                var pat = $@"\b(?:{Regex.Escape(prefix)}|{tableName})\.{req}\b";
                if (!cols.Any(c => Regex.IsMatch(c, pat, RegexOptions.IgnoreCase)))
                    return DebugFail($"coluna {req} com alias inválido");
            }

            
            foreach (var other in Tables.Except(new[] { fromTbl }))
            {
                var patJoin = $@"\bINNER\s+JOIN\s+{other}\b";
                if (!Regex.IsMatch(norm, patJoin, RegexOptions.IgnoreCase))
                    return DebugFail($"falta INNER JOIN {other}");
            }

            
            var patMovCom = $@"\b(?:{Regex.Escape(mAlias)}\.IdComodo\s*=\s*{Regex.Escape(cAlias)}\.IdComodo|" +
                             $@"{Regex.Escape(cAlias)}\.IdComodo\s*=\s*{Regex.Escape(mAlias)}\.IdComodo)\b";
            if (!Regex.IsMatch(norm, patMovCom, RegexOptions.IgnoreCase))
                return DebugFail("faltando ON IdComodo entre Moveis e Comodos");


            var patItemMov = $@"\b(?:{Regex.Escape(iAlias)}\.IdMovel\s*=\s*{Regex.Escape(mAlias)}\.IdMovel|" +
                  $@"{Regex.Escape(mAlias)}\.IdMovel\s*=\s*{Regex.Escape(iAlias)}\.IdMovel)\b";
            if (!Regex.IsMatch(norm, patItemMov, RegexOptions.IgnoreCase))
                return DebugFail("faltando ON IdMovel entre Itens e Moveis");

          
            var wherePat = $@"\b(?:{Regex.Escape(iAlias)}|{Regex.Escape(mAlias)})\.IdMovel\s*=\s*{ExpectedMovelId}\b";
            if (!Regex.IsMatch(norm, wherePat, RegexOptions.IgnoreCase))
                return DebugFail("IdMovel deve ser qualificado por Moveis ou Itens");

      
            if (!(itemsObj is IEnumerable en))
                return DebugFail("resultado não é IEnumerable");
            var list = en.Cast<object>().ToList();
            if (list.Count == 0)
                return DebugFail("nenhum registro retornado");

       
            string[] expectedItems = { "LivroJoiner", "CanetaJoiner" };
            var actualItems = list
                .Select(o => o.GetType().GetProperty("NomeItem")?.GetValue(o)?.ToString())
                .Where(s => !string.IsNullOrEmpty(s))
                .OrderBy(s => s)
                .ToArray();
            if (!expectedItems.OrderBy(s => s).SequenceEqual(actualItems))
                return DebugFail("itens retornados não correspondem ao esperado");

            foreach (var obj in list)
            {
                var t = obj.GetType();
                foreach (var req in RequiredCols)
                {
                    var p = t.GetProperty(req);
                    if (p == null || p.GetValue(obj) == null)
                        return DebugFail($"objeto sem {req}");
                }
            }

            Debug.Log("Validator: resposta correta → valid");
            return true;
        });
    }

    bool DebugFail(string reason)
    {
        Debug.Log($"Validator: {reason} → invalid");
        return false;
    }
}


