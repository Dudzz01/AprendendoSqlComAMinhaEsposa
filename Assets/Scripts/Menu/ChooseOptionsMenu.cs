using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseOptionsMenu : MonoBehaviour
{
    // Start is called before the first frame update

    private void Start()
    {
        //audioSourceMenu = GetComponent<AudioSource>();
    }

    public void SelectPlay()
    {
        GameController.s.arrayFasesDesbloqueadas[0] = true;

        if (GetComponent<SaveGame>().LoadGameOfScene() == null)
        {
           
            Debug.Log($"Fase {1} é igual a {GameController.s.arrayFasesDesbloqueadas[0]}");
            GetComponent<SaveGame>().SaveGameOfScene(GameController.s);
        }
        else
        {
            
            GameController.s = GetComponent<SaveGame>().LoadGameOfScene();
        }

        
        SceneManager.LoadScene("SelecionarFaseTeste");
    }

    public void Creditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    public void BackMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Exit()
    {
        Application.Quit();
    }

}
