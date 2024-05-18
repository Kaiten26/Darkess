using UnityEngine;

public class PauseMenuActivator : MonoBehaviour
{
    public GameObject pauseMenuUI; // Référence au Canvas du menu de pause

    void Start()
    {
        // Assurez-vous que le menu de pause est désactivé au démarrage
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuUI != null)
            {
                if (pauseMenuUI.activeSelf)
                {
                    HidePauseMenu();
                }
                else
                {
                    ShowPauseMenu();
                }
            }
        }
    }

    public void ShowPauseMenu()
    {
        pauseMenuUI.SetActive(true);
    }

    public void HidePauseMenu()
    {
        pauseMenuUI.SetActive(false);
    }
}
