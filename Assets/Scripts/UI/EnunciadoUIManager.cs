using UnityEngine;
using TMPro;

public class EnunciadoUIManager : MonoBehaviour
{
    public static EnunciadoUIManager I;
    public TextMeshProUGUI textoEnunciadoTMP;

    private void Awake() => I = this;

    public void Show(string texto)
    {
        textoEnunciadoTMP.text = texto;
    }
}
