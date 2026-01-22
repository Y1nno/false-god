using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TextOutputter : MonoBehaviour
{
    
    public Transform scrollbar;
    public TextMeshProUGUI textPrefab;
    public Transform contentPanel;

    public Queue<TMP_Text> outputTexts = new Queue<TMP_Text>();

    public static TextOutputter Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private TextOutputter(){}

    public void OutputText(string text)
    {
        TMP_Text newText = Instantiate(textPrefab, contentPanel);
        newText.text = text;
        Canvas.ForceUpdateCanvases();
        scrollbar.GetComponent<Scrollbar>().value = 0;
        outputTexts.Enqueue(newText);
        if (outputTexts.Count > 50)
        {
            TMP_Text oldText = outputTexts.Dequeue();
            Destroy(oldText.gameObject);
        }
    }
}
