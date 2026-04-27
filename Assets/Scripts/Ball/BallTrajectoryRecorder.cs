using System.Collections.Generic;
using UnityEngine;

public class BallTrajectoryRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float recordInterval = 0.04f;
    [SerializeField] private float minDistanceBetweenPoints = 0.1f;

    private readonly List<Vector2> trajectoryPoints = new();
    private bool isRecording;
    private float timer;

    public void StartRecording()
    {
        isRecording = true;
        timer = 0f;
        trajectoryPoints.Clear();
        trajectoryPoints.Add(transform.position);
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public void ClearTrajectory()
    {
        trajectoryPoints.Clear();
    }

    private void Update()
    {
        if (!isRecording) return;

        timer += Time.deltaTime;

        if (timer >= recordInterval)
        {
            timer = 0f;
            TryRecordPoint();
        }
    }

    private void TryRecordPoint()
    {
        Vector2 currentPosition = transform.position;

        if (trajectoryPoints.Count == 0)
        {
            trajectoryPoints.Add(currentPosition);
            return;
        }

        float distance = Vector2.Distance(
            currentPosition,
            trajectoryPoints[^1]
        );

        if (distance >= minDistanceBetweenPoints)
        {
            trajectoryPoints.Add(currentPosition);
        }
    }

    public List<Vector2> GetTrajectoryPoints()
    {
        return trajectoryPoints;
    }
}