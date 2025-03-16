using UnityEngine;

public class UI_Tab : MonoBehaviour
{
    [SerializeField] UI_Handler handler;
    [SerializeField] BuildModeObject.ObjectType ObjectType;

    public void OnClick()
    {
        handler.SwitchType(ObjectType);
    }
}
