using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnlargeBallScript : MonoBehaviour
{
    public Environment MyEnvironment; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Ball")
        {
            //Delete powerup from powerup list in environment 
            MyEnvironment.powerUpList.Remove(gameObject);
            Destroy(gameObject);
            MyEnvironment.powerUpBall = true;
        }
    }
}
