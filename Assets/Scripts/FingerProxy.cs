using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

// Script to apply finger proxy algorithm
public class FingerProxy : MonoBehaviour
{
    // Set to the proxy object from unity
    public Transform proxyToPlace;

    // Masks for detecting what can and cannot be hit by raycast
    public LayerMask mask, fingerMask;

    // Variables to keep track of distance, force, torque, and angles
    public Vector3 distance, force, normalForce;
    public Vector3 torque;
    public float torqueMag;
    float oldAngle, angleChange; // Previous angle value (absolute), delta angle

    // enteredMesh keeps track of if the finger just entered this frame
    // isFingerInMesh keeps track of if the finger is in the mesh
    bool enteredMesh;
    public bool isFingerInMesh;

    // Constants for calculating torque  -- from Papers in Talise's GDrive --  "Char. of Tissue Stiffness", "Mechanical Properties and Young's Mod"
    float fingerRadius = 0.002f;
    float skinFriction = 0.46f;
    float skinThickness = 0.02f;
    float skinTwistRadius = 0.01f;
    // Skin Stiffness of 266.3 N/m
    float skinStiffness = 266.3f;
    // 4.2 * 10^5 N/m^2
    float skinElasticity = 4.2f * (float)Math.Pow(10, 5);

    // Adjustment accounts for origin at center of object, not at tip, must be set based on size of finger
    Vector3 adjustment = new Vector3(0, 0, 0.01525f);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Forward facing ray
        Ray ray = new Ray(transform.position, -transform.forward);

        // Backward facing ray
        Vector3 rayStartPosition = new Vector3(0, 0, 0);
        // Draw the ray from a starting point far away
        // Raycast only detects hits from the outside of the mesh so the ray must start from the opposite side
        rayStartPosition += (transform.position + transform.forward);
        Ray ray2 = new Ray(rayStartPosition, -transform.forward);
        

        // Count the number of hits to determine if inside or outside the skin
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 100, mask);
        if (hits.Length == 0)
        {
            isFingerInMesh = true;
        }
        else if (Vector3.Distance(hits[0].point, transform.position) <= 0.01525) // 0.01525 =  distance from orgin of finger object to tip to finger object
        {
            isFingerInMesh = true;
        }
        else
        {
            isFingerInMesh = false;
        }

        // Detect a hit from the backwards ray // ray from orgina of finger and outwards toward the skin
        RaycastHit hit;
        if (Physics.Raycast(ray2, out hit, 100, mask))
        {
            if (isFingerInMesh)
            {
                // UnityEngine.Debug.DrawLine(ray2.origin, transform.position, Color.red);
                
                // Finding the angle for Torque Rendering:
                // Set the initial angle for when the finger entered the skin and current angle
                if (enteredMesh == false)
                {
                    enteredMesh = true;
                    oldAngle = transform.eulerAngles.z;
                    angleChange = 0;
                }
                else
                { 
                    float angleDelta = transform.eulerAngles.z - oldAngle;
                    if (angleDelta > 100)  // Set a lower threshold due to being within the Update function (1 frame)
                    {
                        angleDelta -= 360;
                        
                    }
                    else if (angleDelta < -100)
                    {
                        angleDelta += 360;
                    }
                    oldAngle = transform.eulerAngles.z;
                    angleChange += angleDelta;
                    // print(angleChange);
                }

                // The rest of finger proxy
                // Finger is inside the skin so move proxy to hit point
                proxyToPlace.position = hit.point;

                // Calculate force using skin stiffness
                distance = (transform.position + adjustment) - proxyToPlace.position;
                force = skinStiffness * distance;


                // Calculate torque
                // Torque from normal force only (see paper)
                normalForce = (Vector3.Dot(force, hit.normal)) * hit.normal;
                torque = (2 * skinFriction * fingerRadius / 3) * normalForce;

                // Account for any rotation
                // Given a 100 MPa for skin 100 MPa * pi * r^4 / 2 = k
                float skinTorsionalConstant = skinElasticity * 2 * (float)Math.PI * 0.4f * skinThickness * fingerRadius * skinTwistRadius;
                float rotationalTorque =  skinTorsionalConstant * angleChange * (float)Math.PI / 180;

                torqueMag = rotationalTorque + torque.z;
            }
            else
            {
                // Finger is outside the skin
                proxyToPlace.position = transform.position;
                distance = (transform.position) - proxyToPlace.position;
                force = skinStiffness * distance;
                torque *= 0;
                torqueMag = 0;
                enteredMesh = false;
            }
        }
        else
        {
            // Finger is outside the skin
            proxyToPlace.position = transform.position;
            distance = transform.position - proxyToPlace.position;
            force = skinStiffness * distance;
            torque *= 0;
            torqueMag = 0;
            enteredMesh = false;
        }
        //print(distance);
    }
}
