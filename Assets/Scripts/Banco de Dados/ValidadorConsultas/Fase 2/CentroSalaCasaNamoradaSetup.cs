using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FurnitureInteractable))]
public class CentroSalaCasaNamoradaSetup : MonoBehaviour
{
   
    const int ExpectedMovelId = 18;
    static readonly HashSet<string> ExpectedItems = new HashSet<string> {
        "ChaveCentroSalaCasaNamorada"
    };

    void Awake()
    {
        var fi = GetComponent<FurnitureInteractable>();

        fi.validator = new Func<object, bool>(itemsObj =>
        {
            if (!(itemsObj is IEnumerable enumerable))
            {
                Debug.Log("Validator: não é IEnumerable → invalid");
                return false;
            }

            var lista = enumerable.Cast<object>().ToList();
            if (lista.Count != ExpectedItems.Count)
            {
                Debug.Log($"Validator: esperava {ExpectedItems.Count} itens, mas recebeu {lista.Count} → invalid");
                return false;
            }

            var found = new HashSet<string>();
            foreach (var item in lista)
            {
                var type = item.GetType();

      
                var propId = type.GetProperty("IdMovel");
                if (propId == null)
                {
                    Debug.Log("Validator: objeto sem propriedade IdMovel → invalid");
                    return false;
                }

            

                var propDica = item.GetType().GetProperty("Dica");
                if (propDica == null ||
                    string.IsNullOrEmpty(propDica.GetValue(item)?.ToString()))
                {
                    Debug.Log("Validator: objeto sem Dica");
                    return false;
                }
                int idMovel = Convert.ToInt32(propId.GetValue(item));
                if (idMovel != ExpectedMovelId)
                {
                    Debug.Log($"Validator: IdMovel={idMovel} (esperado {ExpectedMovelId}) → invalid");
                    return false;
                }

              
                var propNome = type.GetProperty("NomeItem");
                if (propNome == null)
                {
                    Debug.Log("Validator: objeto sem propriedade NomeItem → invalid");
                    return false;
                }
                var nomeItem = propNome.GetValue(item) as string;
                if (!ExpectedItems.Contains(nomeItem))
                {
                    Debug.Log($"Validator: NomeItem='{nomeItem}' não é esperado → invalid");
                    return false;
                }
                found.Add(nomeItem);
            }

            if (!found.SetEquals(ExpectedItems))
            {
                Debug.Log($"Validator: itens retornados {string.Join(",", found)} ≠ esperados {string.Join(",", ExpectedItems)} → invalid");
                return false;
            }

            Debug.Log("Validator: resposta correta → valid");
            if (GameController.s.quantidadesDesafiosConcluidos[19] == false)
            {
                GameController.s.quantidadesDesafiosConcluidos[19] = true;
                GameController.s.desafiosConcluidos++;
            }
            return true;
        });
    }
}
