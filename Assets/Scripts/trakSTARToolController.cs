using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trakSTARToolController : MonoBehaviour
{
    private Vector3 objectPosition;
    private Vector3 objectRotation;


    public float[] toolPose = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    public int toolNumber = 0;
    //Convert default position values from in to m
    private float poseScale = 0.0254f;

    void Update()
    {
        transformGameObject();
    }

    // variable to keep track of if the sensor is in the positive z or negative z
    // trakSTAR is giving positive values for any negative z with magnitude greater that 1
    private bool isNegative = false;

    private void transformGameObject()
    {
        setObjectPose();
        //Map Raw stream to Unity coordinate system and convert in to m

        if (toolPose[1] < 0)
        {
            // Check for negative numbers that occur when the sensor crosses from positive to negative
            while (toolPose[1] < 0)
            {
                setObjectPose();
            }
            isNegative = !isNegative;
        }

        // Set object positions
        if (isNegative == false)
        {
            objectPosition = new Vector3(-toolPose[2], -toolPose[3], -toolPose[1]);
        }
        if (isNegative)
        {
            objectPosition = new Vector3(toolPose[2], toolPose[3], toolPose[1]);
        }

        objectPosition *= poseScale;

        gameObject.transform.position = objectPosition + trakSTARController.instance.trakSTARBasePosition;


        objectRotation = new Vector3(toolPose[5], toolPose[4], toolPose[6]);
        gameObject.transform.eulerAngles = objectRotation + trakSTARController.instance.trakSTARBaseRotation;
    }

    private void setObjectPose()
    {
        if (toolNumber == 0)
        {
            toolPose = TrainerController.instance.poseZero;
        }
        else if (toolNumber == 1)
        {
            toolPose = TrainerController.instance.poseOne;
        }
    }
}
