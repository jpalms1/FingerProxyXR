using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Diagnostics;

using Debug = UnityEngine.Debug;
using static System.Net.Mime.MediaTypeNames;
using System;

public class SerialComms : MonoBehaviour
{
    GameObject index, thumb;

    //Set the port and the baud rate to 9600
    public string portName = "COM9";
    //public int baudRate = 115200;
    public int baudRate = 9600;
    SerialPort stream;

    private float lastTime = 0.0f;
    private float currentTime = 0.0f;

    public static string[] arduinoDataVals;
    public static float[] unityDataVals;

    public int expectedUnityEntries;

    private List<string[]> arduinoDataList;
    private List<float[]> unityDataList;

    private string oldMessage = "0.00 0.00 0.00 0.00 0.00\n";


    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Start Serial Comms");
        //initialize lists
        arduinoDataList = new List<string[]>();
        unityDataList = new List<float[]>();

        //Start the hand behavior/game logic as disabled until serial comms is up
        index = GameObject.Find("trakSTAR/Index");
        thumb = GameObject.Find("trakSTAR/Thumb");

        //Define and open serial port       
        stream = new SerialPort(portName, baudRate);
        stream.Open();
        // stream.DiscardInBuffer();
        // stream.DiscardOutBuffer();

        Debug.Log("<color=green>Serial Communication Established</color>");

        //Serial Port Read and Write Timeouts
        stream.ReadTimeout = 5;
        stream.WriteTimeout = 10;

        //Enable Game Logic
        //GetComponent<HapticController>().enabled = true;
        Debug.Log("<color=blue>Haptic Controller Enabled</color>");

        writeSerial("0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00");
        stream.Close();
        // readSerial();
    }
    
    // Update is called once per frame
    void Update()
    {
        //Debug.Log("SerialComms.cs");
        if (stream.IsOpen || !stream.IsOpen)  // 
        {
            currentTime = Time.time;

            if (currentTime - lastTime > 0.01f)
            {
                // Get force from finger proxy
                Vector3 index_force = index.GetComponent<FingerProxy>().force;
                Vector3 thumb_force = thumb.GetComponent<FingerProxy>().force;
                float index_torque = index.GetComponent<FingerProxy>().torqueMag;
                float thumb_torque = thumb.GetComponent<FingerProxy>().torqueMag;
                index_force.y *= -1;  // Changed direction to consider  Unity's LHC 
                float index_magF = Mathf.Sqrt(index_force.sqrMagnitude);
                float index_shear = Mathf.Sqrt(index_force.x * index_force.x + index_force.y * index_force.y);
                thumb_force.x *= -1; // Changed direction to consider  Unity's LHC 
                thumb_force.y *= -1; // Changed direction to consider  Unity's LHC 
                float thumb_magF = Mathf.Sqrt(thumb_force.sqrMagnitude);
                float thumb_shear = Mathf.Sqrt(thumb_force.x * thumb_force.x + thumb_force.y * thumb_force.y);
                
                // Message for Regular hoxels
                //string message = index_force.x.ToString("0.00") + " " + index_force.y.ToString("0.00") + " " + index_force.z.ToString("0.00") + " " + index_magF.ToString("0.00") + " " + index_shear.ToString("0.00") + " " + index_torque.ToString("0.000") + "\n";
                // message = message + " " + thumb_force.x.ToString("0.00") + " " + thumb_force.y.ToString("0.00") + " " + thumb_force.z.ToString("0.00") + " " + thumb_magF.ToString("0.00") + " " + thumb_shear.ToString("0.00") + '\n';
                string message =  index_magF.ToString("0.00") + "A" + thumb_magF.ToString("0.00") + "B" + "\n";

                // Message for finger prints
                string message2 = index_force.x.ToString("0.00") + " " + index_force.y.ToString("0.00") + " " + index_force.z.ToString("0.00") + " " + index_torque.ToString("0.000");
                message2 = message2 + " " + thumb_force.x.ToString("0.00") + " " + thumb_force.y.ToString("0.00") + " " + thumb_force.z.ToString("0.00") + " " + thumb_torque.ToString("0.000") + '\n';

                // print(message2);
                
                
                
                // Check to see if the old message is the same
                // If so dont sent the new message to avoid semaphore issue
                if (oldMessage != message)
                {
                    // Open stream
                    stream.Open();
                    stream.DiscardInBuffer();
                    stream.DiscardOutBuffer();
                    //Write to Arudino via serial

                    writeSerial(message);
                    lastTime = currentTime;

                    // Close stream to avoid semaphore error --- check later on Jasmin's PCs
                    stream.Close();
                    oldMessage = message;
                }
                
                //Read the serial data that came from arduino
                // readSerial();

                //Debug.Log("Back from Arduino");
                // lastTime = currentTime;
                
            }
        }
    }
    
    public void writeSerial(string message)
    {
        if (stream.IsOpen)
        {
            //read stuff
            try
            {
                stream.Write(message);
                Debug.Log("MESSAGE: " + message);
            }
            catch (IOException e)
            {
                //time out exception
                Debug.Log("Failed MESSAGE: " + message);
                print(e);
                print(Time.time);
            }    
        }
       
    }

    public void readSerial()
    {
        if (stream.IsOpen)
        {
            try
            {
                //read stuff
                string arduinoMessage = stream.ReadLine();
                 Debug.Log("arduinoMessage: " + arduinoMessage);
                arduinoDataVals = arduinoMessage.Split(',');
            }
            catch (System.TimeoutException)
            {
                //time out exception
                //Do Nothing
            }
        }
    }

    private void OnApplicationQuit()
    {
        //Close Serial Stream
        Debug.Log("<color=blue>GOODBYE</color>");
        stream.Close();

        /*Shut down the application*/
        //UnityEditor.EditorApplication.isPlaying = false;

        //Ignored in editor, used in build
        UnityEngine.Application.Quit();
    }
}
