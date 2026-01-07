using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(FurnitureInteractable))]
public class MesaAbandonadaCozinhaSetup : MonoBehaviour
{
    
    const int ExpectedMovelId = 57;  
    const int ExpectedQtdItens = 5;   
    const int ExpectedQtdChaves = 0;   

    void Awake()
    {
        var fi = GetComponent<FurnitureInteractable>();
        fi.validator = new Func<string, object, bool>((sql, itemsObj) =>
        {
            
            var norm = Regex.Replace(sql, @"\s+", " ").Trim();
            norm = Regex.Replace(norm, @"\s*\.\s*", ".");

            
            var m = Regex.Match(norm,
                @"^SELECT\s+(?<cols>.*?)\s+FROM\s+(?<tbl>\w+)",
                RegexOptions.IgnoreCase);
            if (!m.Success)
                return DebugFail("não encontrou SELECT … FROM");
            if (!Regex.IsMatch(m.Groups["tbl"].Value, @"^itens$", RegexOptions.IgnoreCase))
                return DebugFail($"FROM deve ser 'Itens', achou '{m.Groups["tbl"].Value}'");

            
            if (Regex.IsMatch(norm, @"\b(?:INNER|LEFT|RIGHT|FULL)\s+JOIN\b", RegexOptions.IgnoreCase))
                return DebugFail("nenhum JOIN é permitido neste exercício");

            
            var cols = m.Groups["cols"].Value
                        .Split(',')
                        .Select(c => c.Trim())
                        .ToList();
            if (cols.Count != 3)
                return DebugFail($"esperava 3 colunas, achou {cols.Count}");

            var patterns = new[]
            {
                new Regex(@"^(?:Itens\.)?IdMovel$", RegexOptions.IgnoreCase),
                new Regex(@"^COUNT\(\s*\*\s*\)\s+AS\s+QtdItens$", RegexOptions.IgnoreCase),
                new Regex(@"^SUM\(\s*NomeItem\s+LIKE\s*'Chave%'\s*\)\s+AS\s+QtdChaves$", RegexOptions.IgnoreCase),
            };
            foreach (var pat in patterns)
                if (!cols.Any(col => pat.IsMatch(col)))
                    return DebugFail($"falta coluna correspondente a '{pat}'");

            var whereMatches = Regex.Matches(norm, @"\bWHERE\b", RegexOptions.IgnoreCase);
            if (whereMatches.Count != 1)
                return DebugFail("deve haver exatamente uma cláusula WHERE");
            var wherePat = new Regex(
                $@"\bWHERE\s+(?:\w+\.)?IdMovel\s*=\s*{ExpectedMovelId}\b",
                RegexOptions.IgnoreCase);
            if (!wherePat.IsMatch(norm))
                return DebugFail($"falta WHERE IdMovel = {ExpectedMovelId}");
         
            if (Regex.IsMatch(norm, @"\bAND\b|\bOR\b", RegexOptions.IgnoreCase))
                return DebugFail("AND/OR não permitido na cláusula WHERE");

           
            var groupByMatches = Regex.Matches(norm, @"\bGROUP\s+BY\b", RegexOptions.IgnoreCase);
            if (groupByMatches.Count != 1)
                return DebugFail("deve haver exatamente um GROUP BY");
            var groupByPat = new Regex(
                @"\bGROUP\s+BY\s+(?:\w+\.)?IdMovel\b",
                RegexOptions.IgnoreCase);
            if (!groupByPat.IsMatch(norm))
                return DebugFail("falta GROUP BY IdMovel");

            
            if (!(itemsObj is IEnumerable en))
                return DebugFail("resultado não é IEnumerable");
            var list = en.Cast<object>().ToList();
            if (list.Count != 1)
                return DebugFail($"esperava 1 registro, mas recebeu {list.Count}");

          
            var obj = list[0];
            var t = obj.GetType();


            // IdMovel
            var propId = t.GetProperty("IdMovel");
            if (propId == null) return DebugFail("propriedade 'IdMovel' não encontrada");
            var idVal = (int)propId.GetValue(obj);
            Debug.Log("Valor idVal" + idVal);
            if (idVal != ExpectedMovelId)
                return DebugFail($"IdMovel={idVal} (esperado {ExpectedMovelId})");

            // QtdItens
            var propQtd = t.GetProperty("QtdItens");
            if (propQtd == null) return DebugFail("propriedade 'QtdItens' não encontrada");
            var qtd = Convert.ToInt32(propQtd.GetValue(obj));
            Debug.Log("Valor qtd" + qtd);
            if (qtd != ExpectedQtdItens)
                return DebugFail($"QtdItens={qtd} (esperado {ExpectedQtdItens})");

            // QtdChaves
            var propChv = t.GetProperty("QtdChaves");
            if (propChv == null) return DebugFail("propriedade 'QtdChaves' não encontrada");
            var chv = Convert.ToInt32(propChv.GetValue(obj));
            Debug.Log("Valor chv" + chv);
            if (chv != ExpectedQtdChaves)
                return DebugFail($"QtdChaves={chv} (esperado {ExpectedQtdChaves})");

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
