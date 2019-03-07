﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class TextUpdater : MonoBehaviour
{
    NetworkManagerHost manager;
    [SerializeField]
    GameObject text;
    [SerializeField]
    GameObject textGroup;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<NetworkManagerHost>();
        textGroup = GameObject.FindObjectOfType<VerticalLayoutGroup>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<NetworkConnection, int> entry in manager.connections)
        {
            var x = Instantiate(text, textGroup.transform);
            x.GetComponent<Text>().text = string.Format("Player {0} Connected", entry.Value);
        } 
    }
}
