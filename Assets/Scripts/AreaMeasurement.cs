using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using UnityEngine.UI;

public class AreaMeasurement : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer; // Assign a LineRenderer to draw lines.
    [SerializeField]
    private TMPro.TextMeshPro areaText; // Assign a UI Text to display the calculated area.
    [SerializeField]
    private GameObject dotPrefab; // Prefab for dots placed at touch points.
    [SerializeField]
    private GameObject fbxModelPrefab; // Prefab for the FBX model to place.
    [SerializeField]
    private Button placeModelButton; // Button to place the model.

    private List<Vector3> worldPoints = new List<Vector3>(); // List of world points for area calculation.
    private List<GameObject> dots = new List<GameObject>(); // List to store instantiated dot objects.
    private Camera arCamera;
    private GameObject placedModel;

    private const float closeThreshold = 0.1f; // Distance threshold to detect closing the polygon.
    private bool polygonClosed = false; // Flag to stop adding dots after polygon is closed.

    void Start()
    {
        arCamera = Camera.main; // Assign AR Camera.

        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer not assigned.");
        }

        if (placeModelButton != null)
        {
            placeModelButton.gameObject.SetActive(false); // Hide the button initially.
            placeModelButton.onClick.AddListener(OnPlaceModelButtonClicked);
        }
    }

    void Update()
    {
        if (polygonClosed) return; // Stop adding dots if polygon is closed.

        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Only respond to the TouchPhase.Began event
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;
                Vector3 worldPosition = arCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 0.5f));

                // If the polygon is already closed, ignore further inputs
                if (worldPoints.Count > 2 && Vector3.Distance(worldPosition, worldPoints[0]) < closeThreshold)
                {
                    ClosePolygon();
                    return;
                }

                // Instantiate dot at touch point
                GameObject dot = Instantiate(dotPrefab, worldPosition, Quaternion.identity);
                dots.Add(dot);

                // Add touch point to the list
                worldPoints.Add(worldPosition);

                // Update the LineRenderer
                UpdateLine();
            }
        }
    }

    private void UpdateLine()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPositions(worldPoints.ToArray());
    }

    private void ClosePolygon()
    {
        // Connect the last point to the first to close the shape
        worldPoints.Add(worldPoints[0]);
        UpdateLine();

        // Calculate and display the area
        float area = CalculatePolygonArea(worldPoints);
        UpdateAreaText(area);

        // Enable the "Place Model" button
        if (placeModelButton != null && area > 0)
        {
            placeModelButton.gameObject.SetActive(true);
        }

        polygonClosed = true; // Stop further dot instantiation
    }

    private float CalculatePolygonArea(List<Vector3> points)
    {
        float area = 0f;

        for (int i = 0; i < points.Count - 1; i++) // Exclude the last duplicate point
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[(i + 1) % points.Count]; // Wrap around to the first point
            area += (p1.x * p2.z) - (p2.x * p1.z); // Use x and z for 2D calculation
        }

        return Mathf.Abs(area * 0.5f); // Return the absolute value
    }

    private void UpdateAreaText(float areaInMeters)
    {
        if (areaText != null)
        {
            // Convert area to square feet
            float areaInSqFt = areaInMeters * 10.764f;

            // Convert area to marla
            float areaInMarla = areaInSqFt / 272.25f;

            // Update the text to show all units
            areaText.text = $"Total Area: {areaInMeters:F2} m²\n" +
                            $"{areaInSqFt:F2} sqft\n" +
                            $"{areaInMarla:F2} marla(s)";
        }
    }
    

    private void OnPlaceModelButtonClicked()
    {
        if (fbxModelPrefab == null || worldPoints.Count < 3) return;

        // Calculate the center of the polygon
        Vector3 center = CalculateCentroid(worldPoints);

        // Instantiate the FBX model at the center
        placedModel = Instantiate(fbxModelPrefab, center, Quaternion.identity);

        // Ensure the model aligns with the detected surface
        if (Physics.Raycast(center + Vector3.up, Vector3.down, out RaycastHit hit, 2f))
        {
            placedModel.transform.position = hit.point;
            placedModel.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        // Hide the "Place Model" button after placement
        placeModelButton.gameObject.SetActive(false);
    }

    private Vector3 CalculateCentroid(List<Vector3> points)
    {
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
        {
            centroid += point;
        }
        return centroid / points.Count;
    }
}