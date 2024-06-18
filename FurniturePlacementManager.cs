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
        else if (Input.touchCount == 2 && !IsPointerOverUI(Input.GetTouch(0)) && !IsPointerOverUI(Input.GetTouch(1)))
        {
            // Handle two-finger touch for rotation and scaling
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Scale the object based on the touch delta magnitude difference
            if (placedObject != null)
            {
                ScaleObject(deltaMagnitudeDiff);
            }

            // Rotate the object based on the rotation gesture
            RotateObject(touchOne);
        }
    }

    // Move the object based on touch delta position
    void MoveObject(Vector2 deltaPosition)
    {
        placedObject.transform.position += new Vector3(deltaPosition.x, 0, deltaPosition.y);
    }

    // Scale the object based on touch delta magnitude difference
    void ScaleObject(float deltaMagnitudeDiff)
    {
        placedObject.transform.localScale += new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, deltaMagnitudeDiff);
    }

    // Rotate the object based on touch rotation
    void RotateObject(Touch touch)
    {
        Vector2 touchDeltaPosition = touch.deltaPosition;

        float rotationAngle = touchDeltaPosition.x * rotationSpeed * Mathf.Deg2Rad;
        Quaternion deltaRotation = Quaternion.Euler(Vector3.up * rotationAngle);

        placedObject.transform.rotation = initialRotation * deltaRotation;
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
