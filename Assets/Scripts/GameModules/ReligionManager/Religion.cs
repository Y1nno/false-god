using UnityEngine;

public class Religion
{
    public string ReligionID { get; protected set; }

    public virtual void OnJoinReligion()
    {
        Debug.Log("Joined religion: " + ReligionID);
        TextOutputter.Instance.OutputText("You have joined the religion: " + ReligionID);
    }

    public virtual void OnLeaveReligion()
    {
        Debug.Log("Left religion: " + ReligionID);
        TextOutputter.Instance.OutputText("You have left the religion: " + ReligionID);
    }
}
