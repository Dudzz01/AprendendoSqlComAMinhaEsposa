using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(FurnitureInteractable))]
public class MesaVizinhoSalaSetup : MonoBehaviour
{
    const int ExpectedComodoId = 11;
    const int ExpectedMovelId = 41;

    static readonly string[] RequiredCols = { "NomeMovel", "NomeItem", "Dica", "NomeComodo" };
    static readonly string[] Tables = { "moveis", "itens", "comodos" };

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
            if (!m.Success)
                return DebugFail("não encontrou SELECT … FROM");

            var fromTbl = m.Groups["tbl"].Value.ToLower();
            if (fromTbl != "moveis")
                return DebugFail($"FROM deve ser 'Moveis', achou '{fromTbl}'");

   
            string mAlias = "Moveis", iAlias = "Itens", cAlias = "Comodos";
            var aliasPattern = @"\b(?:FROM|JOIN)\s+(Moveis|Itens|Comodos)(?:\s+AS)?\s+(\w+)\b";
            foreach (Match am in Regex.Matches(norm, aliasPattern, RegexOptions.IgnoreCase))
            {
                var tbl = am.Groups[1].Value.ToLower();
                var alias = am.Groups[2].Value;
                if (tbl == "moveis") mAlias = alias;
                if (tbl == "itens") iAlias = alias;
                if (tbl == "comodos") cAlias = alias;
            }

     
            var cols = m.Groups["cols"].Value
                        .Split(',').Select(c2 => c2.Trim()).ToList();
            if (cols.Count != RequiredCols.Length)
                return DebugFail($"esperava {RequiredCols.Length} colunas, achou {cols.Count}");
            foreach (var req in RequiredCols)
            {
                var pat = $@"\b(?:{Regex.Escape(mAlias)}|Moveis)\.{req}\b|" +
                          $@"\b(?:{Regex.Escape(iAlias)}|Itens)\.{req}\b|" +
                          $@"\b(?:{Regex.Escape(cAlias)}|Comodos)\.{req}\b";
                if (!cols.Any(c2 => Regex.IsMatch(c2, pat, RegexOptions.IgnoreCase)))
                    return DebugFail($"falta coluna {req}");
            }

         
            if (!Regex.IsMatch(norm, @"\bLEFT\s+JOIN\s+Itens\b", RegexOptions.IgnoreCase))
                return DebugFail("falta LEFT JOIN Itens");
            if (!Regex.IsMatch(norm, @"\bINNER\s+JOIN\s+Comodos\b", RegexOptions.IgnoreCase))
                return DebugFail("falta INNER JOIN Comodos");

    
            if (Regex.IsMatch(norm, @"\bINNER\s+JOIN\s+Itens\b", RegexOptions.IgnoreCase))
                return DebugFail("INNER JOIN Itens não permitido — use apenas LEFT JOIN Itens");

       
            if (Regex.IsMatch(norm, @"\bLEFT\s+JOIN\s+(Moveis|Comodos)\b", RegexOptions.IgnoreCase))
                return DebugFail("LEFT JOIN só permitido em Itens → invalid");

    
            if (Regex.IsMatch(norm, @"\bRIGHT\s+JOIN\b", RegexOptions.IgnoreCase))
                return DebugFail("RIGHT JOIN não permitido nesta fase → invalid");

    
            if (Regex.IsMatch(norm, @"\bINNER\s+JOIN\s+Moveis\b", RegexOptions.IgnoreCase))
                return DebugFail("INNER JOIN Moveis não é permitido aqui → invalid");

       
            var patItemMov = $@"\b(?:{Regex.Escape(iAlias)}\.IdMovel\s*=\s*{Regex.Escape(mAlias)}\.IdMovel|" +
                              $@"{Regex.Escape(mAlias)}\.IdMovel\s*=\s*{Regex.Escape(iAlias)}\.IdMovel)\b";
            if (!Regex.IsMatch(norm, patItemMov, RegexOptions.IgnoreCase))
                return DebugFail("faltando ON entre Itens e Moveis");

            var patMovCom = $@"\b(?:{Regex.Escape(mAlias)}\.IdComodo\s*=\s*{Regex.Escape(cAlias)}\.IdComodo|" +
                             $@"{Regex.Escape(cAlias)}\.IdComodo\s*=\s*{Regex.Escape(mAlias)}\.IdComodo)\b";
            if (!Regex.IsMatch(norm, patMovCom, RegexOptions.IgnoreCase))
                return DebugFail("faltando ON entre Moveis e Comodos");

         
            var whereCom = $@"\b{Regex.Escape(mAlias)}\.IdComodo\s*=\s*{ExpectedComodoId}\b";
            if (!Regex.IsMatch(norm, whereCom, RegexOptions.IgnoreCase))
                return DebugFail("falta WHERE IdComodo = 11");
            var whereMov = $@"\b{Regex.Escape(mAlias)}\.IdMovel\s*=\s*{ExpectedMovelId}\b";
            if (!Regex.IsMatch(norm, whereMov, RegexOptions.IgnoreCase))
                return DebugFail("falta WHERE IdMovel = 41");

       
            if (!(itemsObj is IEnumerable en))
                return DebugFail("resultado não é IEnumerable");
            var list = en.Cast<object>().ToList();
            if (list.Count != 1)
                return DebugFail($"esperava 1 registro, mas recebeu {list.Count}");

        
            var obj = list[0];
            var t = obj.GetType();
            foreach (var req in new[] { "NomeMovel", "NomeComodo" })
            {
                var p = t.GetProperty(req);
                if (p == null || p.GetValue(obj) == null)
                    return DebugFail($"objeto sem {req}");
            }
         
            var nomeItemVal = t.GetProperty("NomeItem").GetValue(obj) as string;
            if (nomeItemVal != null && nomeItemVal != "ChaveMesaVizinho")
                return DebugFail($"NomeItem='{nomeItemVal}' (esperado 'ChaveMesaVizinho' ou null)");
            var dicaVal = t.GetProperty("Dica").GetValue(obj) as string;
            if (dicaVal != null && dicaVal != "Sem Dica")
                return DebugFail($"Dica='{dicaVal}' (esperado 'Sem Dica' ou null)");

            Debug.Log("Validator: resposta correta → valid");

            if (GameController.s.quantidadesDesafiosConcluidos[27] == false)
            {
                GameController.s.quantidadesDesafiosConcluidos[27] = true;
                GameController.s.desafiosConcluidos++;
            }

            return true;
        });
    }

    bool DebugFail(string reason)
    {
        Debug.Log($"Validator: {reason} → invalid");
        return false;
    }
}
