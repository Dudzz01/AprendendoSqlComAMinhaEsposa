using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogoScript : MonoBehaviour
{
    [Header("Diálogo")]
    [SerializeField] private string[] dialogoNpc;
    [SerializeField] private int dialogoIndex;
    [SerializeField] private GameObject dialogoPanel;
    [SerializeField] private TMP_Text dialogoTexto;
    [SerializeField] private TMP_Text nomeDialogoTexto;
    [SerializeField] private Image imageNpc;
    [SerializeField] private Sprite spriteNpc;
    [SerializeField] private bool readyToSpeak;
    [SerializeField] private bool startDialogo;
    [SerializeField] private bool goMenuAfterDialogo;

    private GameObject playerObj;

    void Start()
    {
        dialogoPanel.SetActive(false);

        playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj.GetComponent<Player>().enabled = false;
        playerObj.GetComponent<Animator>().enabled = false;


        StartDialogo();
    }

    void Update()
    {
       
        if (Input.GetButtonDown("Fire1") && readyToSpeak && !startDialogo)
        {
            StartDialogo();
        }
      
        else if (startDialogo && dialogoTexto.text == dialogoNpc[dialogoIndex])
        {
            dialogoIndex++;
            if (dialogoIndex < dialogoNpc.Length)
            {
                StartCoroutine(DelayAndShowNext());
            }
            else
            {
              
                dialogoPanel.SetActive(false);
                startDialogo = false;
                dialogoIndex = 0;
                playerObj.GetComponent<Player>().enabled = true;
                playerObj.GetComponent<Animator>().enabled = true;
                if (goMenuAfterDialogo)
                {
                    
                    SceneManager.LoadScene("Menu");
                }
            }
        }
    }

    private void StartDialogo()
    {
  
        if (playerObj == null)
            playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj.GetComponent<Player>().enabled = false;

    
        dialogoPanel.SetActive(true);
        startDialogo = true;
        dialogoIndex = 0;
     
        imageNpc.sprite = spriteNpc;
        StartCoroutine(ShowDialogo());
    }

    private IEnumerator DelayAndShowNext()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(ShowDialogo());
    }

    private IEnumerator ShowDialogo()
    {
        dialogoTexto.text = "";
        foreach (char letra in dialogoNpc[dialogoIndex])
        {
            dialogoTexto.text += letra;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            readyToSpeak = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            readyToSpeak = false;
    }
}
