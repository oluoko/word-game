using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public class State
    {
        public Color fillColor;
        public Color textColor = Color.white;

    }
    
    public State tileState {get; private set;}
    public char letter {get; private set; }
    
    private TextMeshProUGUI text;
    private Image fill;

    

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        fill = GetComponent<Image>();
    }

    public void SetLetter(char letter)
    {
        this.letter = letter;
        text.text = letter.ToString();
    }

    public void SetState(State state)
    {
        this.tileState = state;
        fill.color = state.fillColor;
        text.color = state.textColor;
    }
    
}
