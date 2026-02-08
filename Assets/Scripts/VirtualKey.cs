using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirtualKey : MonoBehaviour
{
    [System.Serializable]
    public class KeyState
    {
        public Color backgroundColor;
        public Color textColor = Color.white;
    }

    public enum KeyType
    {
        Letter,
        Enter,
        Backspace
    }

    [Header("Key Properties")]
    public KeyType keyType = KeyType.Letter;
    public char letter;

    [Header("Key States")]
    public KeyState defaultState;
    public KeyState correctState;
    public KeyState misplacedState;
    public KeyState incorrectState;
    public KeyState disabledState;

    private string currentStateType = "default";
    private KeyState currentState;
    private Button button;
    private Image background;
    private TextMeshProUGUI text;

    private void Awake()
    {
        button = GetComponent<Button>();
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        switch (keyType)
        {
            case KeyType.Letter:
                text.text = char.ToUpper(letter).ToString();
                break;
        }

        SetStateByType("default");
    }

    private void OnValidate()
    {
        if (keyType == KeyType.Letter && letter != '\0')
        {
            letter = char.ToUpper(letter);
        }
    }

    public void SetState(KeyState state)
    {   
        currentState = state;
        background.color = state.backgroundColor;
        if (text)
        {
            text.color = state.textColor;
        }
    }
    
    public void SetStateByType(string stateType)
    {
        currentStateType = stateType;
        
        switch (stateType)
        {
            case "correct":
                SetState(correctState);
                break;
            case "misplaced":
                SetState(misplacedState);
                break;
            case "incorrect":
                SetState(incorrectState);
                break;
            default:
                SetState(defaultState);
                break;
        }
    }
    
    public string GetCurrentStateType()
    {
        return currentStateType;
    }
    
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
        
        if (!interactable && disabledState != null)
        {
            SetState(disabledState);
        }
    }

    public void OnKeyPressed()
    {
        VirtualKeyboard keyboard = GetComponentInParent<VirtualKeyboard>();
        if (keyboard != null)
        {
            AudioManager.Instance?.PlayButtonClick();
            keyboard.OnKeyPressed(this);
        }
        else
        {
            Debug.LogWarning("VirtualKey could not find VirtualKeyboard parent!");
        }
    }
}