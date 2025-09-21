using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

 [RequireComponent(typeof(LineRenderer))]
 public class Navigator : MonoBehaviour
 {
     // ...existing code...
     // Allows GameManager to enable/disable guiding lines
     public void SetGuidingLinesActive(bool active)
     {
         if (lineRenderer == null)
             lineRenderer = GetComponent<LineRenderer>();
         lineRenderer.enabled = active;
     }
    public GameObject start;
    public GameObject end;

    public float updateInterval = 0.2f; // Path recalculation frequency
    public int smoothingSubdivisions = 8; // Higher = smoother line
    public float offsetY = 1f; // Vertical offset above the floor

    private LineRenderer lineRenderer;
    private NavMeshPath navMeshPath;
    private float timer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        navMeshPath = new NavMeshPath();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdatePath();
        }
    }

    void UpdatePath()
    {
        if (start == null || end == null)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        Vector3 startPos = start.transform.position;
        Vector3 endPos = end.transform.position;

        if (NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, navMeshPath))
        {
            if (navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                var smoothPath = GetSmoothPath(navMeshPath.corners, smoothingSubdivisions);

                // Apply the vertical offset here!
                for (int i = 0; i < smoothPath.Count; i++)
                {
                    smoothPath[i] = new Vector3(
                        smoothPath[i].x,
                        smoothPath[i].y + offsetY,
                        smoothPath[i].z
                    );
                }

                lineRenderer.positionCount = smoothPath.Count;
                lineRenderer.SetPositions(smoothPath.ToArray());
            }
            else
            {
                lineRenderer.positionCount = 0; // No valid path
            }
        }
        else
        {
            lineRenderer.positionCount = 0; // Calculation failed
        }
    }

    // --- Catmull-Rom Smoothing Utility ---

    List<Vector3> GetSmoothPath(Vector3[] points, int subdivisions)
    {
        var smoothed = new List<Vector3>();
        if (points.Length < 2)
            return smoothed;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 p0 = i == 0 ? points[i] : points[i - 1];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = (i + 2 < points.Length) ? points[i + 2] : points[i + 1];

            for (int j = 0; j < subdivisions; j++)
            {
                float t = j / (float)subdivisions;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                smoothed.Add(point);
            }
        }
        smoothed.Add(points[points.Length - 1]);
        return smoothed;
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * ((2f * p1) +
                       (-p0 + p2) * t +
                       (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                       (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
    }
}
