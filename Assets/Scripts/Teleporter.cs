using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision){
      SceneManager.LoadScene("Level2", LoadSceneMode.Single);    
    }

    void Update()
    {
        
    }
}
