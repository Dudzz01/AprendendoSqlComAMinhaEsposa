using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FurnitureInteractable))]
public class ComputadorCasaNamoradaSetup : MonoBehaviour
{
    
    const int ExpectedMovelId = 26;
    
    static readonly HashSet<int> ExpectedItemIds = new HashSet<int> { 51, 52 };

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

            
            if (lista.Count != ExpectedItemIds.Count)
            {
                Debug.Log($"Validator: esperava {ExpectedItemIds.Count} itens, mas recebeu {lista.Count} → invalid");
                return false;
            }

            var foundIds = new HashSet<int>();
            foreach (var item in lista)
            {
                var type = item.GetType();

               
                var propMovel = type.GetProperty("IdMovel");
                if (propMovel == null)
                {
                    Debug.Log("Validator: objeto sem propriedade IdMovel → invalid");
                    return false;
                }

                var propNome = item.GetType().GetProperty("NomeItem");
                if (propNome == null ||
                    string.IsNullOrEmpty(propNome.GetValue(item)?.ToString()))
                {
                    Debug.Log("Validator: objeto sem NomeItem");
                    return false;
                }



                var propDica = item.GetType().GetProperty("Dica");
                if (propDica == null ||
                    string.IsNullOrEmpty(propDica.GetValue(item)?.ToString()))
                {
                    Debug.Log("Validator: objeto sem Dica");
                    return false;
                }


                int idMovel = Convert.ToInt32(propMovel.GetValue(item));
                if (idMovel != ExpectedMovelId)
                {
                    Debug.Log($"Validator: encontrou IdMovel={idMovel} (esperado {ExpectedMovelId}) → invalid");
                    return false;
                }

               
                var propItem = type.GetProperty("IdItem");
                if (propItem == null)
                {
                    Debug.Log("Validator: objeto sem propriedade IdItem → invalid");
                    return false;
                }
                foundIds.Add(Convert.ToInt32(propItem.GetValue(item)));
            }

            if (!foundIds.SetEquals(ExpectedItemIds))
            {
                Debug.Log($"Validator: IDs retornados [{string.Join(",", foundIds)}] ≠ esperados [{string.Join(",", ExpectedItemIds)}] → invalid");
                return false;
            }

            Debug.Log("Validator: resposta correta → valid");
            if (GameController.s.quantidadesDesafiosConcluidos[17] == false)
            {
                GameController.s.quantidadesDesafiosConcluidos[17] = true;
                GameController.s.desafiosConcluidos++;
            }
            return true;
        });
    }
}
