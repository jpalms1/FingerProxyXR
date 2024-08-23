using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

// Basic raycasting, detects what the tip of the needle is hitting as it is inserted

public class RayCast : MonoBehaviour
{
    public GameObject[] hapticTargets;
    public bool isNeedleInSkin;
    public bool isNeedleInMuscle;
    public bool isNeedleInDisc;
    public bool isNeedleInStem;
    public bool isNeedleInBone;

    // Raycast will only detect hits with this mask
    public LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        // Contains the names of each game object the needle can detect
        hapticTargets = new GameObject[] {
            GameObject.Find("skin"),
            GameObject.Find("Muscle"),
            GameObject.Find("Male_Skeletal_Intervertabral_Discs_Geo"),
            GameObject.Find("Nervous_Brain_Stem_Geo"),
            GameObject.Find("Skeleton") };
    }

    // Update is called once per frame
    void Update()
    {
        // Draw the ray out of the front of the needle
        Ray ray = new Ray(transform.position, transform.up);
        RaycastHit hitInfo;

        // Detect any hits, must have the correct mask
        if (Physics.Raycast(ray, out hitInfo, 10, mask))
        {
            // Detect hits within the body of the needle, adjust length based on size of needle
            if (hitInfo.distance < 0.05)
            {
                print(hitInfo.collider.gameObject.name);
                print(hitInfo.distance);
                if (hitInfo.collider.gameObject.name == hapticTargets[0].name)
                {
                    //Combine skin and muscle feedback but use skin mesh collider
                    isNeedleInSkin = true;
                    isNeedleInMuscle = true;
                    isNeedleInDisc = false;
                    isNeedleInStem = false;
                    isNeedleInBone = false;
                    print("Skin/Muscle  ENTER");
                }
                if (hitInfo.collider.gameObject.name == hapticTargets[2].name)
                {
                    isNeedleInDisc = true;
                    isNeedleInSkin = false;
                    isNeedleInMuscle = false;
                    isNeedleInStem = false;
                    isNeedleInBone = false;
                    print("Disc  ENTER");
                }
                if (hitInfo.collider.gameObject.name == hapticTargets[3].name)
                {
                    isNeedleInStem = true;
                    isNeedleInSkin = false;
                    isNeedleInMuscle = false;
                    isNeedleInDisc = false;
                    isNeedleInBone = false;
                    //drip CSF
                    //GetComponent<ParticleSystem>().Play();
                    print("Stem  ENTER");
                    //no force
                }
                if (hitInfo.collider.gameObject.name == hapticTargets[4].name)
                {
                    isNeedleInBone = true;
                    isNeedleInSkin = false;
                    isNeedleInMuscle = false;
                    isNeedleInDisc = false;
                    isNeedleInStem = false;
                    print("Bone  ENTER");
                }

            }

        }
        else
        {
            // UnityEngine.Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, UnityEngine.Color.green);
        }

    }
}
