using System;
using UnityEngine;

public class trackGeneration : MonoBehaviour
{
    public GameObject[] trackPieces;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject trackPiece in trackPieces)
        {
            GameObject.Instantiate(trackPiece);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
