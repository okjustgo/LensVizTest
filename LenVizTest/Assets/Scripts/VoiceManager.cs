// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;

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

        keywords.Add("Create bar graph", () =>
        {
            this.createGraph("barplot");
        });

        keywords.Add("Create scatter plot", () =>
        {
            this.createGraph("scatterplot");
        });

        keywords.Add("Create surface chart", () =>
        {
            this.createGraph("surface");
        });

        keywords.Add("Create radar tube", () =>
        {
            this.createGraph("radartube");
        });

        keywords.Add("Remove", () =>
        {
            this.removeGraph();
        });

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

    private void createGraph(string graphType)
    {
        var graphPrefab = Resources.Load(@"Graph", typeof(GameObject)) as GameObject;
        var graph = Instantiate(graphPrefab);
        graph.SendMessage("OnSelect");
        graph.GetComponent<Graph>().whatToRender = graphType;
        graph.transform.parent = gameObject.transform;
    }

    void Update()
    {

    }

    void OnDestroy()
    {

    }

}