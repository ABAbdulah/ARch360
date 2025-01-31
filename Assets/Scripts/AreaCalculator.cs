using UnityEngine;
using TMPro;
using System.Collections.Generic; // Required for List<>

public class AreaCalculator : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public TMP_Text areaText;
    private List<Vector3> points = new List<Vector3>();

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 1f));
            points.Add(touchPosition);

            if (points.Count > 1)
            {
                DrawLine();
            }

            if (points.Count >= 3 && Vector3.Distance(points[0], touchPosition) < 0.1f)
            {
                CalculateAndDisplayArea();
            }
        }
    }

    void DrawLine()
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    void CalculateAndDisplayArea()
    {
        float area = CalculateTriangleArea(points[0], points[1], points[2]); // For simplicity
        areaText.text = $"Area: {area:F2} sq units";
        areaText.transform.position = (points[0] + points[1] + points[2]) / 3;
    }

    float CalculateTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * Vector3.Cross(p2 - p1, p3 - p1).magnitude;
    }
}
