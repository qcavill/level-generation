using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorCollisions : MonoBehaviour
{
  public Rigidbody rb;
  bool repairRoom = false;
  bool corridorAlive = true;
  public GameObject wallPrefab;
  int collisions = 0;
  int corridor_collisions = 0;
  
  

  void Start()
  {
  
  
  }

 
  void Update()
  {
    
   
  }
  void OnCollisionEnter(Collision other) {
    CorridorMeta cm = gameObject.GetComponent<CorridorMeta>();
    GameObject replacementWall;
  
    if (cm.centerPiece == true && other.gameObject.tag == "room") {
      collisions += 1;
    }
    if(cm.centerPiece == true) {
      if(collisions > 0 && corridorAlive == true){
        foreach(GameObject corridor in cm.pairedCorridors) {
          Destroy(corridor);
          repairRoom = true;
          if(cm.horizontal == true) {
            replacementWall = Instantiate(wallPrefab, new Vector3(
            cm.room1.rightExt,
            1,
            (cm.room1.bottomExt + cm.room1.topExt)/2), Quaternion.identity);
            replacementWall.transform.localScale = new Vector3(0.5f,4f,2f);   

            replacementWall = Instantiate(wallPrefab, new Vector3(
            cm.room2.leftExt,
            1,
            (cm.room2.bottomExt + cm.room2.topExt)/2), Quaternion.identity);
            replacementWall.transform.localScale = new Vector3(0.5f,4f,2f);  
            cm.room1.rightcor = null;
            cm.room2.leftcor = null;
            
          }else{
            replacementWall = Instantiate(wallPrefab, new Vector3(
              (cm.room1.rightExt + cm.room1.leftExt)/2,
              1,
              cm.room1.topExt
              ), Quaternion.identity);
            replacementWall.transform.localScale = new Vector3(2f,4f,0.5f);      
            replacementWall = Instantiate(wallPrefab, new Vector3(
              (cm.room2.rightExt + cm.room2.leftExt)/2,
              1,
              cm.room2.bottomExt
              ), Quaternion.identity);
            cm.room1.topcor = null;
            cm.room2.bottomcor = null;
            replacementWall.transform.localScale = new Vector3(2f,4f,0.5f);    
          }
          corridorAlive = false;
        }
      }
    }
  }
}
