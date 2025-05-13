using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI; // Needed for Image

public class ButtonVFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameManager.GameState activeState;
    //if mouse position is hovering over this UI image button, change text size of tmp to 34
    public void OnPointerEnter(PointerEventData eventData)
    {
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>();
        tmp.fontSize = 34;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>();
        tmp.fontSize = 30;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //getcomponent in child
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>();
        tmp.color = new Color(0.9f, 0.9f, 0.9f, 1f);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //getcomponent in child
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>();
        tmp.color = Color.white;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnStateActive;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnStateActive;
    }

    //if corresponding state is active, change TMP color to light yellow (#FFFF99), else white
    public void OnStateActive(GameManager.GameState state)
    {
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>();
        if (state == activeState)
        {
            tmp.color = new Color(1f, 1f, 0.6f); // #FFFF99
        }
        else
        {
            tmp.color = Color.white; // Or your default color
        }
    }
}
