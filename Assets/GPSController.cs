﻿using UnityEngine;
using System.Collections;
using System;

public class GPSController : MonoBehaviour {

    private static GPSController instance;
    public static GPSController Instance {
        get
        {
            if(instance == null)
            {
                // Try looking for any existing not inicialized
                instance = FindObjectOfType<GPSController>();
                if(instance == null)
                {
                    var go = new GameObject("GPSController");
                    instance = go.AddComponent<GPSController>();
                }
                instance.SendMessage("Awake");
                instance.SendMessage("Start");
            }
            return instance;
        }
    }

    public Texture2D connectedSimbol;
    public Texture2D connectingSimbol;
    public Texture2D disconnectedSimbol;
    public float blinkingTime;
    public float iconWidth, iconHeight;

    private float time;

    public float update = .1f; // 5 meters
    public float accuracy = 1; // 10 meters

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void Start()
    {
        if(!IsStarted())
            StartCoroutine(StartLocation());
    }

    IEnumerator StartLocation()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        Input.location.Start(accuracy, update);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }
    }

    public bool IsStarted()
    {
        return Input.location.status == LocationServiceStatus.Initializing || Input.location.status == LocationServiceStatus.Running;
    }

    void Update()
    {
        time += Time.deltaTime;

        if (time > blinkingTime)
            time = time - Mathf.Floor(time/blinkingTime) * blinkingTime;
    }

    void OnGUI()
    {
        var paintSimbol = disconnectedSimbol;

        switch (Input.location.status) { 
            default:
            case LocationServiceStatus.Failed:
            case LocationServiceStatus.Stopped:
                paintSimbol = disconnectedSimbol;
                break;
            case LocationServiceStatus.Initializing:
                paintSimbol = connectingSimbol;
                break;
            case LocationServiceStatus.Running:
                paintSimbol = IsLocationValid() ? connectedSimbol : ((time > blinkingTime / 2f) ? connectedSimbol : connectingSimbol);
                break;
        }

        if(Event.current.type == EventType.Repaint)
            GUI.DrawTexture(new Rect(Screen.width - iconWidth - 5, 5, iconWidth, iconHeight), paintSimbol);
    }

    public bool IsLocationValid()
    {
        return Input.location.status == LocationServiceStatus.Running 
            && Input.location.lastData.timestamp > 0 
            && Mathf.Max(Input.location.lastData.horizontalAccuracy, Input.location.lastData.verticalAccuracy) < 50; // Max 50 metros
    }
}
