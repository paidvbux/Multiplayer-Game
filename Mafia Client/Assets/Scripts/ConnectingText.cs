using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectingText : MonoBehaviour
{
    [SerializeField] float interval;
    [SerializeField] int maxDotCount;
    public int currentDotCount;
    TextMeshProUGUI textComponent => GetComponent<TextMeshProUGUI>();
    void Start()
    {
        StartCoroutine(UpdateText());
    }
    // Update is called once per frame
    void Update()
    {
        textComponent.text = "Connecting" + new string('.', currentDotCount);
    }

    IEnumerator UpdateText()
    {
        yield return new WaitForSeconds(interval);
        currentDotCount = currentDotCount + 1 > maxDotCount ? 0 : currentDotCount + 1;
        StartCoroutine(UpdateText());
    }
}
