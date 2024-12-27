using System.Collections;
using UnityEngine;
using TMPro;
public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public float subtitleDuration = 3f;

    private void Start()
    {
        subtitleText.text = "";
    }

    public void DisplaySubtitle(string text, float duration = -1f)
    {
        if (duration < 0f) duration = subtitleDuration;
        StartCoroutine(ShowSubtitleCoroutine(text, duration));
    }

    private IEnumerator ShowSubtitleCoroutine(string text, float duration)
    {
        subtitleText.text = text;
        yield return new WaitForSeconds(duration);
        subtitleText.text = "";
    }
}
