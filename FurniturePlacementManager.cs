using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class FurniturePlacementManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    private GameObject placedObject; // Reference to the instantiated object
    private Vector3 initialTouchPosition;
    private Quaternion initialRotation;
    private float rotationSpeed = 5f;

    private void Awake()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();
    }

    private void Update()
    {
        if (Input.touchCount == 1 && !IsPointerOverUI(Input.GetTouch(0)))
        {
            Touch touch = Input.GetTouch(0);

            // Handle touch phase
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Raycast to detect planes
                    List<ARRaycastHit> hits = new List<ARRaycastHit>();
                    if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                    {
                        if (placedObject == null)
                        {
                            // Instantiate object at the hit position
                            placedObject = Instantiate(placedObject, hits[0].pose.position, hits[0].pose.rotation);
                            initialTouchPosition = touch.position;
                            initialRotation = placedObject.transform.rotation;
                        }
                    }
                    break;

                case TouchPhase.Moved:
                    // If object is instantiated, move it based on touch delta position
                    if (placedObject != null)
                    {
                        Vector2 deltaPosition = touch.deltaPosition * Time.deltaTime;
                        MoveObject(deltaPosition);
                    }
                    break;

                case TouchPhase.Ended:
                    placedObject = null;
                    break;
            }
        }
    }

    public void SwitchObject(GameObject newObjectPrefab)
    {
        if (placedObject != null)
        {
            Destroy(placedObject); // Destroy the current object
        }

        placedObject = Instantiate(newObjectPrefab, Vector3.zero, Quaternion.identity);
    }

    // Check if touch is over UI
    bool IsPointerOverUI(Touch touch)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
