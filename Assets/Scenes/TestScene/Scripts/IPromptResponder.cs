using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public interface IPromptResponder
{
    public void ProcessPromptResponse(int decisionIndex);
}
