using System.Collections;
using UnityEngine;

public enum BallState
{
    AttachedToPlayer,
    Thrown,
    Returning
}

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallTrajectoryRecorder trajectoryRecorder;
    [SerializeField] private Transform holdPoint;

    [Header("Recall Settings")]
    [SerializeField] private float recallSpeed = 18f;
    [SerializeField] private float attachDistance = 0.35f;

    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private BallState currentState = BallState.AttachedToPlayer;
    private Transform playerTarget;
    private Transform attachedTarget;
    private Coroutine recallCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<Collider2D>();

        if (trajectoryRecorder == null)
        {
            trajectoryRecorder = GetComponent<BallTrajectoryRecorder>();
        }

        if (currentState == BallState.AttachedToPlayer)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            SetColliderState(false);
        }
    }

    public bool CanBeThrown()
    {
        return currentState == BallState.AttachedToPlayer;
    }

    public void SetHoldPoint(Transform target)
    {
        holdPoint = target;

        if (currentState == BallState.AttachedToPlayer)
        {
            SnapToAttachedTarget();
        }
    }

    public void Throw(Vector2 startPosition, Vector2 direction, float force)
    {
        if (recallCoroutine != null)
        {
            StopCoroutine(recallCoroutine);
            recallCoroutine = null;
        }

        transform.position = startPosition;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        attachedTarget = null;
        SetColliderState(true);

        currentState = BallState.Thrown;

        trajectoryRecorder.ClearTrajectory();
        trajectoryRecorder.StartRecording();

        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    public void RecallToPlayer(Transform player)
    {
        if (currentState != BallState.Thrown) return;

        playerTarget = player;
        attachedTarget = holdPoint != null ? holdPoint : player;
        currentState = BallState.Returning;

        trajectoryRecorder.StopRecording();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        SetColliderState(false);

        recallCoroutine = StartCoroutine(ReturnByReverseTrajectory());
    }

    private IEnumerator ReturnByReverseTrajectory()
    {
        var points = trajectoryRecorder.GetTrajectoryPoints();

        for (int i = points.Count - 1; i >= 0; i--)
        {
            Vector2 targetPoint = points[i];

            while (Vector2.Distance(transform.position, targetPoint) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPoint,
                    recallSpeed * Time.deltaTime
                );

                yield return null;
            }
        }

        Vector3 attachTargetPosition = GetAttachedTargetPosition();

        while (Vector2.Distance(transform.position, attachTargetPosition) > attachDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                attachTargetPosition,
                recallSpeed * Time.deltaTime
            );

            yield return null;

            attachTargetPosition = GetAttachedTargetPosition();
        }

        AttachToPlayer();
    }

    private void LateUpdate()
    {
        if (currentState != BallState.AttachedToPlayer) return;

        SnapToAttachedTarget();
    }

    private void AttachToPlayer()
    {
        currentState = BallState.AttachedToPlayer;
        recallCoroutine = null;
        attachedTarget = holdPoint != null ? holdPoint : playerTarget;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        SetColliderState(false);

        SnapToAttachedTarget();
    }

    private void SetColliderState(bool isEnabled)
    {
        if (ballCollider != null)
        {
            ballCollider.enabled = isEnabled;
        }
    }

    private void SnapToAttachedTarget()
    {
        transform.position = GetAttachedTargetPosition();
    }

    private Vector3 GetAttachedTargetPosition()
    {
        Transform target = attachedTarget != null
            ? attachedTarget
            : holdPoint != null
                ? holdPoint
                : playerTarget;

        return target != null ? target.position : transform.position;
    }
}
