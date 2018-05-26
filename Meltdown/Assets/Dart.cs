﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics; 

public class Dart : MonoBehaviour {

    // Use this for initialization
    Transform TF; 
    Rigidbody RB;

    VRTK_BaseGrabAttach BGA;

    VRTK_InteractableObject IO;

    BoxCollider BC; 

    bool Hold; 
    bool Thrown;

    float FlightSpeed = 2.0f; 
	void Awake ()
    {
        TF = GetComponent<Transform>();
        RB = GetComponent<Rigidbody>();
        //RB.centerOfMass = new Vector3(0.0f, 0.0f, 0.1f);
        BGA = GetComponent<VRTK_BaseGrabAttach>();

        BGA.throwMultiplier = FlightSpeed;

        IO = GetComponent<VRTK.VRTK_InteractableObject>();
        IO.InteractableObjectGrabbed += new InteractableObjectEventHandler(ObjectGrabbed);

        IO.InteractableObjectUngrabbed += new InteractableObjectEventHandler(ObjectUngrabbed);
        BC = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Thrown)
        {
            //print("do we get here");
            TF.LookAt(TF.position + RB.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Thrown)
        {
            Thrown = false;

            if (collision.gameObject.tag == "dartboard")
            {
                print("Hit dartboard");
                RB.useGravity = false;
                RB.isKinematic = true;
                RB.velocity = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        
    }

    private void ObjectGrabbed(object sender, InteractableObjectEventArgs e)

    {
        Hold = true; 
        Debug.Log("Im Grabbed");

        RB.useGravity = true;
        RB.isKinematic = false;

        BC.isTrigger = true;

    }

    private void ObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        Thrown = true;
        Hold = false;
        RB.useGravity = true;
        RB.isKinematic = false;

        BC.isTrigger = false;
    }
}

