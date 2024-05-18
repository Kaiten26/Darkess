using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuButton : MonoBehaviour
{
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] Animator animator;
    [SerializeField] AnimatorFunctions animatorFunctions;
    [SerializeField] int thisIndex;

    void Update()
    {
        if (pauseMenuController.index == thisIndex)
        {
            animator.SetBool("selected", true);
            if (Input.GetAxis("Submit") == 1)
            {
                animator.SetBool("pressed", true);
                if (thisIndex == 0) // Resume Game
                {
                    ResumeGame();
                }
                else if (thisIndex == 1) // Load Menu
                {
                    LoadMenu();
                }
                else if (thisIndex == 2) // Quit Game
                {
                    QuitGame();
                }
            }
            else if (animator.GetBool("pressed"))
            {
                animator.SetBool("pressed", false);
                animatorFunctions.disableOnce = true;
            }
        }
        else
        {
            animator.SetBool("selected", false);
        }
    }

    void ResumeGame()
    {
        pauseMenuController.HidePauseMenu();
    }

    void LoadMenu()
    {
        Time.timeScale = 1f; // Assurez-vous que le temps est remis à sa valeur normale
        SceneManager.LoadScene("Menu");
    }

    void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
