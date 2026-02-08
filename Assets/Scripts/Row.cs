using UnityEngine;

public class Row : MonoBehaviour
{
    public Tile[] tiles {get; private set;}
    
    public string word
    {
        get{
            string word = "";
            foreach(Tile tile in tiles)
            {
                word += tile.letter;
            }
            return word;
        }
    }

    private void Awake()
    {
        tiles = GetComponentsInChildren<Tile>();
    }
}
