using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MafiaDialogue : MonoBehaviour
{
    public SubtitleManager subtitleManager;
    public AudioSource dialogueAudio;
    public AudioClip[] dialogueClips;

    private string[] subtitles = {
        "¿Sabes por qué estás aquí?",
        "Mataste a mi hija. Ese quirófano era su última oportunidad.",
        "Podría matarte ahora mismo. Fácil. Rápido.",
        "Pero eres más útil vivo que muerto.",
        "Trabajarás para mí. Sin preguntas, sin fallos.",
        "Trabajarás en una tienda que controlo. Parece un negocio legítimo, pero es solo una fachada.",
        "Los clientes entran... y algunos no vuelven a salir. Tú sabrás cuáles. Tú sabrás qué hacer.",
        "Y no olvides: partes del cuerpo. las necesito para mi otro negocio.",
        "Una sola falla... y terminarás como mi hija."
    };
    public Animator mafiaAnimator;
    public string[] animationTriggers;
    private void Start()
    {
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < dialogueClips.Length; i++)
        {
            if (i < animationTriggers.Length && !string.IsNullOrEmpty(animationTriggers[i]))
            {
                mafiaAnimator.SetTrigger(animationTriggers[i]);
            }

            subtitleManager.DisplaySubtitle(subtitles[i], dialogueClips[i].length);

            dialogueAudio.clip = dialogueClips[i];
            dialogueAudio.Play();

            yield return new WaitForSeconds(dialogueClips[i].length + 0.5f);
        }
        SceneManager.LoadScene("tutorial");
    }
}
