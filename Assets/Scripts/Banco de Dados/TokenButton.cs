using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(Button))]
public class TokenButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI label;

    private Action<string> onClick;
    private string tokenText;

    void Awake()
    {
        
            label = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init(string token, Action<string> onClick)
    {
        tokenText = token;
        this.onClick = onClick;

        if (label != null)
            label.text = token;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => this.onClick?.Invoke(tokenText));
    }
}
