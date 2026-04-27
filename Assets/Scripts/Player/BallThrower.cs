using UnityEngine;

public class BallThrower : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInputReader inputReader;

    [Header("References")]
    [SerializeField] private BallController ball;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LineRenderer trajectoryRenderer;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 14f;

    [Header("Trajectory Preview")]
    [SerializeField] private bool showTrajectoryWhileAiming = true;
    [SerializeField] private bool drawTrajectoryGizmos = true;
    [SerializeField] private int trajectorySteps = 30;
    [SerializeField] private float trajectoryTimeStep = 0.05f;
    [SerializeField] private LayerMask trajectoryCollisionMask = ~0;
    [SerializeField] private bool stopTrajectoryAtCollision = true;
    [SerializeField] private float trajectoryStartWidth = 0.18f;
    [SerializeField] private float trajectoryEndWidth = 0.04f;

    private Vector3 initialThrowPointLocalPosition;
    private Vector3 initialThrowPointWorldOffset;
    private Rigidbody2D ballRb;
    private readonly Vector3[] trajectoryPoints = new Vector3[64];
    private int trajectoryPointCount;

    private void Awake()
    {
        if (inputReader == null)
        {
            inputReader = GetComponent<PlayerInputReader>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody2D>();
        }

        CacheThrowPointOffsets();
        RegisterHoldPoint();
        ApplyTrajectoryRendererStyle();
        HideTrajectoryPreview();
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OnThrowPressed += HandleThrowPressed;
            inputReader.OnRecallPressed += HandleRecallPressed;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnThrowPressed -= HandleThrowPressed;
            inputReader.OnRecallPressed -= HandleRecallPressed;
        }
    }

    private void LateUpdate()
    {
        UpdateThrowPointFacing();
        UpdateTrajectoryPreview();
    }

    private void OnValidate()
    {
        ApplyTrajectoryRendererStyle();
    }

    private void HandleThrowPressed()
    {
        if (ball == null) return;
        if (throwPoint == null) return;
        if (mainCamera == null) return;
        if (!ball.CanBeThrown()) return;

        UpdateThrowPointFacing();

        Vector2 direction = GetThrowDirection();

        ball.Throw(
            throwPoint.position,
            direction,
            throwForce
        );
    }

    private void HandleRecallPressed()
    {
        if (ball == null) return;

        ball.RecallToPlayer(transform);
    }

    private void CacheThrowPointOffsets()
    {
        if (throwPoint == null) return;

        if (throwPoint.parent == transform)
        {
            initialThrowPointLocalPosition = throwPoint.localPosition;
            return;
        }

        initialThrowPointWorldOffset = throwPoint.position - transform.position;
    }

    private void UpdateThrowPointFacing()
    {
        if (throwPoint == null) return;

        int facingSign = playerController != null ? playerController.FacingSign : 1;

        if (throwPoint.parent == transform && UsesTransformScaleFacing())
        {
            throwPoint.localPosition = initialThrowPointLocalPosition;
            return;
        }

        if (throwPoint.parent == transform)
        {
            Vector3 localPosition = initialThrowPointLocalPosition;
            localPosition.x = Mathf.Abs(initialThrowPointLocalPosition.x) * facingSign;
            throwPoint.localPosition = localPosition;
            return;
        }

        Vector3 worldOffset = initialThrowPointWorldOffset;
        worldOffset.x = Mathf.Abs(initialThrowPointWorldOffset.x) * facingSign;
        throwPoint.position = transform.position + worldOffset;
    }

    private void RegisterHoldPoint()
    {
        if (ball == null || throwPoint == null) return;

        ball.SetHoldPoint(throwPoint);
    }

    private bool UsesTransformScaleFacing()
    {
        return playerController != null
            && playerController.FlipMode == FacingFlipMode.TransformScale;
    }

    private void UpdateTrajectoryPreview()
    {
        if (!ShouldShowTrajectoryPreview())
        {
            HideTrajectoryPreview();
            return;
        }

        Vector2 direction = GetThrowDirection();
        float initialSpeed = GetInitialThrowSpeed();
        if (initialSpeed <= 0f)
        {
            HideTrajectoryPreview();
            return;
        }

        trajectoryPointCount = BuildTrajectoryPoints(
            throwPoint.position,
            direction * initialSpeed
        );

        if (trajectoryRenderer == null) return;

        trajectoryRenderer.enabled = trajectoryPointCount > 1;
        if (!trajectoryRenderer.enabled) return;

        trajectoryRenderer.positionCount = trajectoryPointCount;
        for (int i = 0; i < trajectoryPointCount; i++)
        {
            trajectoryRenderer.SetPosition(i, trajectoryPoints[i]);
        }
    }

    private bool ShouldShowTrajectoryPreview()
    {
        return showTrajectoryWhileAiming
            && ball != null
            && throwPoint != null
            && mainCamera != null
            && ball.CanBeThrown()
            && inputReader != null
            && inputReader.HasAimInput;
    }

    private Vector2 GetThrowDirection()
    {
        Vector3 mouseScreenPosition = inputReader.AimScreenPosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouseScreenPosition.x,
                mouseScreenPosition.y,
                Mathf.Abs(mainCamera.transform.position.z)
            )
        );

        mouseWorldPosition.z = 0f;
        return ((Vector2)mouseWorldPosition - (Vector2)throwPoint.position).normalized;
    }

    private float GetInitialThrowSpeed()
    {
        float mass = ballRb != null ? ballRb.mass : 1f;
        return throwForce / Mathf.Max(mass, 0.0001f);
    }

    private int BuildTrajectoryPoints(Vector2 startPosition, Vector2 initialVelocity)
    {
        int maxPoints = Mathf.Min(trajectorySteps + 1, trajectoryPoints.Length);
        if (maxPoints <= 1) return 0;

        trajectoryPoints[0] = startPosition;

        Vector2 gravity = Physics2D.gravity * (ballRb != null ? ballRb.gravityScale : 1f);
        int pointCount = 1;

        for (int i = 1; i < maxPoints; i++)
        {
            float time = trajectoryTimeStep * i;
            Vector2 nextPoint = startPosition
                + (initialVelocity * time)
                + (0.5f * gravity * time * time);

            Vector2 previousPoint = trajectoryPoints[pointCount - 1];

            if (stopTrajectoryAtCollision)
            {
                Vector2 segment = nextPoint - previousPoint;
                float distance = segment.magnitude;

                if (distance > 0f)
                {
                    RaycastHit2D hit = Physics2D.Raycast(
                        previousPoint,
                        segment.normalized,
                        distance,
                        trajectoryCollisionMask
                    );

                    if (hit.collider != null)
                    {
                        trajectoryPoints[pointCount] = hit.point;
                        return pointCount + 1;
                    }
                }
            }

            trajectoryPoints[pointCount] = nextPoint;
            pointCount++;
        }

        return pointCount;
    }

    private void HideTrajectoryPreview()
    {
        trajectoryPointCount = 0;

        if (trajectoryRenderer != null)
        {
            trajectoryRenderer.positionCount = 0;
            trajectoryRenderer.enabled = false;
        }
    }

    private void ApplyTrajectoryRendererStyle()
    {
        if (trajectoryRenderer == null) return;

        float startWidth = Mathf.Max(0f, trajectoryStartWidth);
        float endWidth = Mathf.Max(0f, trajectoryEndWidth);

        trajectoryRenderer.widthMultiplier = 1f;
        trajectoryRenderer.widthCurve = new AnimationCurve(
            new Keyframe(0f, startWidth),
            new Keyframe(1f, endWidth)
        );
    }

    private void OnDrawGizmos()
    {
        if (!drawTrajectoryGizmos || trajectoryPointCount < 2) return;

        Gizmos.color = Color.cyan;

        for (int i = 1; i < trajectoryPointCount; i++)
        {
            Gizmos.DrawLine(trajectoryPoints[i - 1], trajectoryPoints[i]);
        }
    }
}
