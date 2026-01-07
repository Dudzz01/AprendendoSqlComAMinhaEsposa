using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FurnitureInteractable))]
public class LivroFaseOneSetup : MonoBehaviour
{
    //Armario Fase 1
    void Awake()
    {
        var fi = GetComponent<FurnitureInteractable>();

        
        fi.validator = new Func<object, bool>(itemsObj =>
        {
            int count = 0;
            if (itemsObj is IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                    count++;
            }
            bool valid = count > 0;
            Debug.Log($"Validator: encontrados {count} registros → valid = {valid}");
            return valid;
        });
    }
}
