using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHitScript : MonoBehaviour
{
    private bool startCount = false;
    private bool hasMoved = false;
    private float counter = 5f;
    private Rigidbody body;
    private Vector3 initialPostion;
    private Environment environment;
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        initialPostion = this.transform.position;
        environment = GetComponentInParent<Environment>();
    }

    // Update is called once per frame
    void Update()
    {
        if (initialPostion != this.transform.position && !hasMoved)
        {
            hasMoved = true;
            body.useGravity = true;
            environment.ballHasBeenTakenNonTraining = true;
        }
        //Set ball to hazard when the countdown started
        if (startCount == true)
        {
            transform.gameObject.tag = "Hazard";
            counter -= Time.deltaTime;
            //Destroy ball if countdown ended
            if (counter <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    //Play sound if ball hits
    void OnCollisionEnter(Collision collision)
    {
        gameObject.GetComponent<AudioSource>().Play();
        startCount = true;
    }
}
