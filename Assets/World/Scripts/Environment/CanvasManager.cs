using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CanvasManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Text messageText;
    public string message;
    public float fadeDuration = 1.0f;
    public float letterPause = 0.05f;
    public float displayDuration = 5.0f;
    public float initialDelay = 1.0f; // Delay before starting to display the text
    public BossController bossController; // Référence au BossController

    private void Start()
    {
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && bossController != null && bossController.IsDead)
        {
            StartCoroutine(ShowMessage());
        }
    }

    private IEnumerator ShowMessage()
    {
        canvasGroup.gameObject.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0, 1, fadeDuration));

        // Wait for initial delay
        yield return new WaitForSeconds(initialDelay);

        // Display text letter by letter
        yield return StartCoroutine(DisplayText(messageText, message, letterPause));

        // Wait for a few seconds
        yield return new WaitForSeconds(displayDuration);

        // Load the Menu scene
        SceneManager.LoadScene("Menu");

        // Optionally wait for the scene to load before fading out
        yield return null;

        // Fade out (in the new scene)
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1, 0, fadeDuration));

        canvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    private IEnumerator DisplayText(Text textComponent, string text, float letterPause)
    {
        textComponent.text = "";
        foreach (char letter in text.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(letterPause);
        }
    }
}
