using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ShowData : MonoBehaviour
{
    public TextMeshPro forceText, torqueText;

    GameObject index, thumb;

    // Start is called before the first frame update
    void Start()
    {
        //Start the hand behavior/game logic as disabled until serial comms is up
        index = GameObject.Find("trakSTAR/Index");
        thumb = GameObject.Find("trakSTAR/Thumb");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 index_force = index.GetComponent<FingerProxy>().force;
        Vector3 thumb_force = thumb.GetComponent<FingerProxy>().force;
        float index_torque = index.GetComponent<FingerProxy>().torqueMag;
        float thumb_torque = thumb.GetComponent<FingerProxy>().torqueMag;

        forceText.text = index_force + "\n" + thumb_force;
        torqueText.text = index_torque + "\n" + thumb_torque;
        //print(forceText.text);

    }
}
