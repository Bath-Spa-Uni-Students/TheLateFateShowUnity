using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeChanger : MonoBehaviour
{
    [SerializeField] private GameObject[] s1;
    [SerializeField] private GameObject[] s2;
    [SerializeField] private GameObject[] s3;
    [SerializeField] private GameObject[] s4;
    [SerializeField] private GameObject[] s5;
    [SerializeField] private GameObject[] s6;
    [SerializeField] private GameObject[] s7;
    [SerializeField] private GameObject[] s8;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateAllSegments();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SelectSegment(GameObject[] segment)
    {
        foreach(GameObject seg in segment)
        {
            seg.SetActive(false);
        }
        segment[Random.Range(0, segment.Length)].SetActive(true);
    }

    private void GenerateAllSegments()
    {
        SelectSegment(s1);
        SelectSegment(s2);
        SelectSegment(s3);
        SelectSegment(s4);
        SelectSegment(s5);
        SelectSegment(s6);
        SelectSegment(s7);
        SelectSegment(s8);
    }
}
