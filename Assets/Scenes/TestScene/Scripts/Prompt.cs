using UnityEngine;
using System.Collections.Generic;

public class Prompt : Subject
{
    private List<string> _decisionsAvailable = new List<string>();
    private string promptMessage = "UNINITIALIZED PROMPT";
    public int DecisionIndex = -1;
    private InputInterface inputInterface;
    private IPromptResponder _responder;

    public Prompt(string header, List<string> options, IPromptResponder responder)
    {
        promptMessage = header;
        _decisionsAvailable = options;
        this._responder = responder;
        if (responder == null)
        {
            Debug.LogWarning("No responder provided for prompt.");
        }
        inputInterface = GameObject.Find("InputInterface").GetComponent<InputInterface>();
        inputInterface.activePrompt = this;

        Display();
    }

    public bool ValidateDecisionIndex(int decisionIndex)
    {
        //Debug.Log($"Validating decision index: {decisionIndex}");
       //Debug.Log($"Decisions available count: {_decisionsAvailable.Count}");
        Debug.Log($"Is decision index valid: {decisionIndex >= 0 && decisionIndex < _decisionsAvailable.Count}");
        return decisionIndex >= 0 && decisionIndex < _decisionsAvailable.Count;
    }
    public void Display()
    {
        string outputText = $"{promptMessage}\n";
        for (int i = 0; i < _decisionsAvailable.Count; i++)
        {
            outputText += $"{i + 1}. {_decisionsAvailable[i]}\n";
        }
        TextOutputter.Instance.OutputText(outputText);
    }

    public void RecieveDecision(int decisionIndex)
    {
        if (ValidateDecisionIndex(decisionIndex))
        {
            this.DecisionIndex = decisionIndex;
            inputInterface.activePrompt = null;
            Debug.Log($"Decision made: {decisionIndex}");
            _responder.ProcessPromptResponse(decisionIndex);
        }
        else
        {
            TextOutputter.Instance.OutputText("Invalid decision index.");
        }
    }
}
