﻿using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Dodger : Agent
{
    //Movement speed and Rotation
    public float MovingSpeed = 5.0f;
    public float RotationSpeed = 5.0f;
    public float Force = 10.0f;

    //Properties
    private Rigidbody body;
    private bool canJump;
    public bool isHit;
    private bool timePast;
    private bool isOnField;

    public override void Initialize()
    {
        base.Initialize();
        body = GetComponent<Rigidbody>();
        body.centerOfMass = Vector3.zero;
        body.inertiaTensorRotation = Quaternion.identity; //Disable rotation in every possible way! https://answers.unity.com/questions/1484746/rigibody-constraints-do-not-work-still-moves-a-lit.html
        StartCoroutine(DelayMethode());
        canJump = true;
        isOnField = true;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //Forward backward
        if (vectorAction[0] != 0)
        {
            Vector3 translation = transform.forward * MovingSpeed * (vectorAction[0] * 2 - 3) * Time.deltaTime;
            transform.Translate(translation, Space.World);
        }

        //Move sideward
        if (vectorAction[1] != 0)
        {
            Vector3 translation = transform.right * MovingSpeed * (vectorAction[1] * 2 - 3) * Time.deltaTime;
            transform.Translate(translation, Space.World);
        }

        //Rotation
        if (vectorAction[2] != 0)
        {
            float rotation = RotationSpeed * (vectorAction[2] * 2 - 3) * Time.deltaTime;
            transform.Rotate(0, rotation, 0);
        }

        //Jump
        if (vectorAction[3] != 0)
        {
            Jump();
        }
    }
    public override void Heuristic(float[] actionsOut)
    {
        /*
        if(transform.name != environment.selectedDodger)
        {
            return;
        }
        */

        //Defined actions
        actionsOut[0] = 0f;
        actionsOut[1] = 0f;
        actionsOut[2] = 0f;
        actionsOut[3] = 0f;

        if (Input.GetKey(KeyCode.Z)) // Moving fwd
        {
            actionsOut[0] = 2f;
        }
        else if (Input.GetKey(KeyCode.S)) // Move Backwards
        {
            actionsOut[0] = 1f;
        }
        else if (Input.GetKey(KeyCode.Q)) // Move left
        {
            actionsOut[1] = 1f;
        }
        else if (Input.GetKey(KeyCode.D)) // Move Right
        {
            actionsOut[1] = 2f;
        }
        else if (Input.GetKey(KeyCode.A)) // Rotate left
        {
            actionsOut[2] = 1f;
        }
        else if (Input.GetKey(KeyCode.E)) // Rotate right
        {
            actionsOut[2] = 2f;
        }
        else if (Input.GetKey(KeyCode.Space)) // Jump
        {
            actionsOut[3] = 1f;
        }
    }

    private void Jump()
    {
        //Make character jump if jump is ready
        if (canJump)
        {
            canJump = false;
            body.AddForce(Vector3.up * Force, ForceMode.Impulse);
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "PlayingGround")
        {
            isOnField = true;
            canJump = true;
        }
        if(collision.collider.tag == "Ground")
        {
            isOnField = false;
            canJump = true;
        }
        if(collision.gameObject.tag == "Ball")
        {
            AddReward(-0.5f);
            isHit = true;
        }
        if(collision.gameObject.tag == "Dodger")
        {
            AddReward(-0.5f);
        }
    }

    //Detect if a second passed
    IEnumerator DelayMethode()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            timePast = true;
        }
    }

    private void FixedUpdate()
    {
        //Add reward if dodger is on playing field
        if (timePast)
        {
            if (isOnField)
            {
                AddReward(0.01f);
            } else
            {
                AddReward(-0.5f);
            }
            timePast = false;
        }
        //Destroys dodger if he falls off from environment
        if(transform.position.y < -1)
        {
            Destroy(this.gameObject);
        }
    }
}
