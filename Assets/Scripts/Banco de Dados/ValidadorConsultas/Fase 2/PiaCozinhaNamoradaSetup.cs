using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PiaCozinhaNamoradaSetup : MonoBehaviour
{

    const int ExpectedMovelId = 13;

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
            if (lista.Count == 0)
            {
                Debug.Log("Validator: sem registros → invalid");
                return false;
            }


            foreach (var item in lista)
            {

                var prop = item.GetType().GetProperty("IdMovel");
                if (prop == null)
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


                var value = prop.GetValue(item);
                int idMovel;
                try
                {
                    idMovel = Convert.ToInt32(value);
                }
                catch
                {
                    Debug.Log($"Validator: não conseguiu converter {value} para int → invalid");
                    return false;
                }


                if (idMovel != ExpectedMovelId)
                {
                    Debug.Log($"Validator: encontrou IdMovel={idMovel} (esperado {ExpectedMovelId}) → invalid");
                    return false;
                }
            }


            Debug.Log($"Validator: todos os {lista.Count} itens vêm de IdMovel={ExpectedMovelId} → valid");
            
            return true;
        });
    }
}
