﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampController : MonoBehaviour
{
    [SerializeField] private GameObject[] camps;

    // Start is called before the first frame update
    void Start()
    {
        // Progreso del juego
        camps[0].SetActive(Config.data.dungeonsStarted >= 0 ? true : false);
        camps[1].SetActive(Config.data.dungeonsStarted >= 1 ? true : false);
        camps[2].SetActive(Config.data.dungeonsStarted >= 4 ? true : false);
        camps[3].SetActive(Config.data.dungeonsStarted >= 6 ? true : false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
