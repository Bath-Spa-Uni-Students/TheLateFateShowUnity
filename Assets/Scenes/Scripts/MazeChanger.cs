using NavMeshPlus.Components;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeChanger : MonoBehaviour
{
    [Header("Room Segment Gorups")]
    [SerializeField] private GameObject[] s1;
    [SerializeField] private GameObject[] s2;
    [SerializeField] private GameObject[] s3;
    [SerializeField] private GameObject[] s4;
    [SerializeField] private GameObject[] s5;
    [SerializeField] private GameObject[] s6;
    [SerializeField] private GameObject[] s7;
    [SerializeField] private GameObject[] s8;
    private int roomCounter = 0;

    [Header("Settings")]
    [SerializeField] private float switchInterval = 300f; // Set this value to somehting higher, ive set lower for testing
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("Transition")]
    [SerializeField] private float transitionFadeTime = 0.5f; // for future fade effect

    [SerializeField] private Transform anchor1, anchor2, anchor3, anchor4 ,anchor5, anchor6, anchor7, anchor8;

    private Coroutine switchCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateAllSegments();
        //StartCoroutine(SegmentReset());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SelectSegment(GameObject[] segment)
    {

        foreach (GameObject seg in segment)
        {
            seg.SetActive(false);
        }
        segment[Random.Range(0, segment.Length)].SetActive(true);

        //add anchor logic
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

        //rebake nav mesh here after rooms switch might casue slight lag but should be hideable with transition effect
    }

    IEnumerator SegmentReset()
    {
        yield return new WaitForSeconds(10f);
        GenerateAllSegments();
        StartCoroutine(SegmentReset());

  
    }
    private void CheckPlayerCollision(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GenerateAllSegments();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            roomCounter++;
            Debug.Log("Player entered a new room. Room counter: " + roomCounter);
            if (roomCounter == 3)
            {
                GenerateAllSegments();
                roomCounter = 0;
            }
        }
    }

    //debug here
    public void ForceSwitch()
    {

    }
}
