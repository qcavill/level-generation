using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelGeneration;


public class CorridorMeta : MonoBehaviour
{
    public bool horizontal = false;
    public bool centerPiece = false;
    public ArrayList pairedCorridors = new ArrayList();
    public Room room1;
    public Room room2;  
}
