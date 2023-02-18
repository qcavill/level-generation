using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Player : object
{
    static public int testVal = 37;
    static public GameObject player;
    static private bool playerFound;
    static private GameObject[] players;
 
    static public GameObject returnPlayer()
    {
      // if(playerFound == false){
        players = GameObject.FindGameObjectsWithTag("player");
        player = players[0];
        playerFound = true;
      
    
      return player;
    }
}
