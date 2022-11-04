using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Creates a visual change which allows the a string to cycle between a number of dots
/// </summary>
public class LoadingText : MonoBehaviour
{
    [SerializeField] float interval;
    [SerializeField] int maxDotCount;

    public string text;

    public int currentDotCount;
    TextMeshProUGUI textComponent => GetComponent<TextMeshProUGUI>();
    void Start()
    {
        StartCoroutine(UpdateText());
    }
    // Update is called once per frame
    void Update()
    {
        textComponent.text = text + new string('.', currentDotCount);
    }

    IEnumerator UpdateText()
    {
        yield return new WaitForSeconds(interval);
        currentDotCount = currentDotCount + 1 > maxDotCount ? 0 : currentDotCount + 1;
        StartCoroutine(UpdateText());
    }
}
