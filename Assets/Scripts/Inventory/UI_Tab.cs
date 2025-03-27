using UnityEngine;
using UnityEngine.UI;

public class UI_Tab : MonoBehaviour
{
    [SerializeField] UI_Handler handler;
    [SerializeField] BuildModeObject.ObjectType ObjectType;

    [SerializeField] Image image;
    [SerializeField] Sprite inactiveTab;
    [SerializeField] Sprite activeTab;

    public void OnClick()
    {
        handler.SwitchType(ObjectType);
    }

    private void FixedUpdate()
    {
        if (handler.selectedType == ObjectType && image.sprite == inactiveTab)
        { 
            image.sprite = activeTab;
        }
        else if (handler.selectedType != ObjectType && image.sprite == activeTab)
        {
            image.sprite = inactiveTab;
        }
    }
}
