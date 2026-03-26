using UnityEngine;
using UnityEngine.InputSystem;

public class SaveSystemDebug : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame)
        {
            SaveManager sm = RunManager.Instance.GetService<SaveManager>();
            if (sm != null)
            {
                sm.NukeAllData();
                Debug.Log("SaveSystemDebug: F6 pressed. All save data nuked.");
            }
        }
    }
}
