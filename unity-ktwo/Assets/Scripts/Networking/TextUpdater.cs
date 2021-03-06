﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class TextUpdater: MonoBehaviour
{
    [SerializeField]
    GameObject text;
    [SerializeField]
    GameObject textGroup;

    // Start is called before the first frame update
    void Start()
    {
        textGroup = GameObject.FindObjectOfType<VerticalLayoutGroup>().gameObject;
    }

    // Update is called once per frame
   public void UpdateText()
    {
        foreach (Transform child in textGroup.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (KeyValuePair<NetworkConnection, PlayerConnectionObject> entry in KtwoServer.instance.connections)
        {
            var x = Instantiate(text, textGroup.transform);
            x.GetComponent<Text>().text = string.Format("Player {0} Connected", entry.Value.connectionNumber);
        }
    }
}
