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

    void Start()
    {

        keywords.Add("Hey holograph", () =>
        {
            this.showOptions();
        });

        keywords.Add("Create bar graph", () =>
        {
            this.createGraph("diamonds.hgd");
        });

        keywords.Add("Create scatter plot", () =>
        {
            this.createGraph("iris.hgd");
        });

        keywords.Add("Create surface chart", () =>
        {
            this.createGraph("volcano.hgd");
        });

        keywords.Add("Create radar tube", () =>
        {
            this.createGraph("mtcars.hgd");
        });

        keywords.Add("Remove", () =>
        {
            this.removeGraph();
        });

        keywords.Add("Start QR", () =>
        {
            this.startQR();
        });

        keywords.Add("Stop QR", () =>
        {
            this.stopQR();
        });

        this.showOptions();

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

    private void createGraph(string dataset)
    {
        var graphPrefab = Resources.Load(@"Graph", typeof(GameObject)) as GameObject;
        var graph = Instantiate(graphPrefab);
        graph.SendMessage("OnSelect");
        graph.GetComponent<Graph>().datasetToRender = dataset;
        graph.transform.parent = gameObject.transform;
    }

    private void startQR()
    {

    }

    private void stopQR()
    {

    }

    private void showOptions()
    {
        var text = "Hello! You can say: \n\n";
        var options = keywords.Keys.ToArray();
        foreach (var option in options)
        {
            if (!option.Equals(options.First())) {
                text += option;
                if (!option.Equals(options.Last()))
                {
                    text += '\n';
                }
            }
            
        }
        var tooltipPrefab = Resources.Load(@"Tooltip", typeof(GameObject)) as GameObject;
        var tooltip = Instantiate(tooltipPrefab);
        tooltip.transform.parent = GameObject.Find("Canvas").transform;
        var tooltipText = tooltip.transform.GetComponent<Text>();
        tooltipText.text = text;
        tooltip.transform.position = new Vector3(0, 0, 0.5f);
        tooltipText.enabled = true;
    }
    void Update()
    {

    }

    void OnDestroy()
    {

    }

}