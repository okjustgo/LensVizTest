// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;

/// <summary>
/// KeywordManager allows you to specify keywords and methods in the Unity
/// Inspector, instead of registering them explicitly in code.
/// This also includes a setting to either automatically start the
/// keyword recognizer or allow your code to start it.
///
/// IMPORTANT: Please make sure to add the microphone capability in your app, in Unity under
/// Edit -> Project Settings -> Player -> Settings for Windows Store -> Publishing Settings -> Capabilities
/// or in your Visual Studio Package.appxmanifest capabilities.
/// </summary>
public class VoiceManager : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer = null;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    private GameObject messagePrefab, message;

    void Start()
    {
        messagePrefab = Resources.Load(@"Message", typeof(GameObject)) as GameObject;
        message = Instantiate(messagePrefab);
        message.SetActive(false);
        message.transform.parent = GameObject.Find("Options").transform;
        
        keywords.Add("Show options", () =>
         {
             this.showOptions();
         });

        keywords.Add("Load cars", () =>
        {
            this.createGraph("mtcars.hgd");
        });

        keywords.Add("Load volcano", () =>
        {
            this.createGraph("volcano.hgd");
        });

        keywords.Add("Load diamonds", () =>
        {
            this.createGraph("diamonds.hgd");
        });

        keywords.Add("Load iris", () =>
        {
            this.createGraph("iris.hgd");
        });

        keywords.Add("Load UK", () =>
        {
            this.createGraph("ukswap.hgd");
        });

        keywords.Add("Rotate", () =>
        {
            this.rotateGraph();
        });

        keywords.Add("Remove", () =>
        {
            this.removeGraph();
        });

        keywords.Add("Reset world", () =>
        {
            this.resetWorld();
        });

        keywords.Add("Read QR", () =>
        {
            this.startQR();
        });

        keywords.Add("Cancel QR", () =>
        {
            this.stopQR();
            var beepDown = GameObject.Find("BeepDown");
            beepDown.GetComponent<AudioSource>().Play();
        });

        keywords.Add("Hide options", () =>
        {
            this.hideOptions();
        });

        //this.startQR();
        //Used for quick testing
        //this.startQR();

        this.createGraph("Fuel_Economy.hgd");
        //this.createGraph("Price_of_Diamonds_by_Quality.hgd");
        
        //this.showOptions();

        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    private void removeGraph()
    {
        //this.hideOptions();
        RaycastHit hitInfo;
        if (Physics.Raycast(
                Camera.main.transform.position,
                Camera.main.transform.forward,
                out hitInfo,
                20.0f,
                Physics.DefaultRaycastLayers))
        {
            hitInfo.transform.SendMessage("Destroy");
        }
    }

    private void resetWorld()
    {
        var graphs = GameObject.FindGameObjectsWithTag("GraphTag");
        foreach (var graph in graphs)
        {
            graph.SendMessage("Destroy");
        }
    }

    private void rotateGraph()
    {
        //this.hideOptions();
        RaycastHit hitInfo;
        if (Physics.Raycast(
                Camera.main.transform.position,
                Camera.main.transform.forward,
                out hitInfo,
                20.0f,
                Physics.DefaultRaycastLayers))
        {
            hitInfo.transform.SendMessage("Rotate");
        }
    }

    private void createGraph(string dataset)
    {
        //this.hideOptions();
        Graph.createGraph(dataset);        
    }

    private void startQR()
    {
        //this.hideOptions();
        GameObject.Find("Managers").GetComponent<QRCodeManager>().SendMessage("StartReading");
    }

    private void stopQR()
    {
        //this.hideOptions();
        GameObject.Find("Managers").GetComponent<QRCodeManager>().SendMessage("StopReading");
    }

    private void showOptions()
    {
        var text = "Hi! You can say: \n\n";
        var options = keywords.Keys.ToArray();
        foreach (var option in options)
        {
            /*if (!option.Equals(options.First()))
            {*/
                text += option;
                if (!option.Equals(options.Last()))
                {
                    text += '\n';
                }
            /*}*/

        }
        var messageText = message.transform.GetComponent<Text>();
        messageText.text = text;
        //message.transform.position = new Vector3(0, 0, 1f);
        messageText.enabled = true;

        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        message.transform.position = headPosition + (gazeDirection * 3);

        Quaternion toQuat = Camera.main.transform.localRotation;
        toQuat.x = 0;
        toQuat.z = 0;
        message.transform.rotation = toQuat;
        message.SetActive(true);
    }

    private void hideOptions()
    {
        var messageText = message.transform.GetComponent<Text>();
        messageText.text = "";
        messageText.enabled = false;
        message.SetActive(false);
    }

    void Update()
    {
        /*if (message.transform.GetComponent<Text>().enabled)
        {
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;
            message.transform.position = headPosition + (gazeDirection * 3);

            Quaternion toQuat = Camera.main.transform.localRotation;
            toQuat.x = 0;
            toQuat.z = 0;
            message.transform.rotation = toQuat;
        }*/
    }

    void OnDestroy()
    {

    }

}