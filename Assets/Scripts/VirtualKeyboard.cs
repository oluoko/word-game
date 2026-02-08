using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualKeyboard : MonoBehaviour
{
    [Header("References")]
    public Board board;

    private Dictionary<char, VirtualKey> letterKeys = new Dictionary<char, VirtualKey>();
    private VirtualKey enterKey;
    private VirtualKey backspaceKey;

    private void Awake()
    {
        
        VirtualKey[] allKeys = GetComponentsInChildren<VirtualKey>();
        
        foreach (VirtualKey key in allKeys)
        {
            switch (key.keyType)
            {
                case VirtualKey.KeyType.Letter:
                    letterKeys[key.letter] = key;
                    break;
                case VirtualKey.KeyType.Enter:
                    enterKey = key;
                    break;
                case VirtualKey.KeyType.Backspace:
                    backspaceKey = key;
                    break;
            }
        }
    }

    public void OnKeyPressed(VirtualKey key)
    {
        if (board != null)
        {
            board.OnVirtualKeyPressed(key);
        }
    }

    public void UpdateKeyState(char letter, string stateType)
    {
        letter = char.ToUpper(letter);
    
        if (letterKeys.ContainsKey(letter))
        {
            VirtualKey key = letterKeys[letter];
            string currentStateType = key.GetCurrentStateType();
        
            if (stateType == "correct")
            {
                key.SetStateByType(stateType);
            }
            else if (stateType == "misplaced" && currentStateType != "correct")
            {
                key.SetStateByType(stateType);
            }
            else if (stateType == "incorrect" && 
                 currentStateType != "correct" && 
                 currentStateType != "misplaced")
            {
                key.SetStateByType(stateType);
            }
        }
    }

    public void ResetKeyboard()
    {
        foreach (var keyPair in letterKeys)
        {
            keyPair.Value.SetStateByType("default");
            keyPair.Value.SetInteractable(true);
        }
    }

    public void SetKeyInteractable(char letter, bool interactable)
    {
        letter = char.ToUpper(letter);
        
        if (letterKeys.ContainsKey(letter))
        {
            letterKeys[letter].SetInteractable(interactable);
        }
    }

    public bool IsKeyInState(char letter, string stateType)
    {
        letter = char.ToUpper(letter);
    
        if (letterKeys.ContainsKey(letter))
        {
            return letterKeys[letter].GetCurrentStateType() == stateType;
        }
        return false;
    }

    public string GetKeyboardState()
    {
        string state = "";
        
        foreach (var keyPair in letterKeys)
        {
            VirtualKey key = keyPair.Value;
            string keyState = key.GetCurrentStateType();
            
            // Save ALL states, including default, so we capture pre-marked incorrect keys
            if (!string.IsNullOrEmpty(keyState))
            {
                state += key.letter + ":" + keyState + ",";
            }
        }
        
        Debug.Log($"GetKeyboardState: {state.TrimEnd(',')}");
        return state.TrimEnd(',');
    }

    public void RestoreKeyboardState(string stateString)
    {
        if (string.IsNullOrEmpty(stateString))
        {
            Debug.Log("RestoreKeyboardState: Empty state string");
            return;
        }
        
        Debug.Log($"RestoreKeyboardState: Processing '{stateString}'");
        
        string[] keyStates = stateString.Split(',');
        
        foreach (string keyState in keyStates)
        {
            if (string.IsNullOrEmpty(keyState)) continue;
            
            string[] parts = keyState.Split(':');
            if (parts.Length == 2)
            {
                char letter = parts[0][0];
                string state = parts[1];
                
                Debug.Log($"RestoreKeyboardState: Setting {letter} to {state}");
                
                // Directly set the state, bypassing the priority logic in UpdateKeyState
                letter = char.ToUpper(letter);
                if (letterKeys.ContainsKey(letter))
                {
                    letterKeys[letter].SetStateByType(state);
                }
            }
        }
    }
}