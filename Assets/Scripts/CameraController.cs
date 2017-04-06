﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {

    public bool frozen = false; // frozen if dragging game piece

    public float pinchSensitivity = 0.001f;
    public float mouseZoomSensitivity = 5.0f;
    public float fingerMoveSensitivity = 0.04f;
    public float mouseMoveSensitivity = 0.8f;

    public float zoomRatio = 1.7f / 30f;
    public float minZoom = 5.0f;
    public float maxZoom = 20.0f;

    public float maxMove = 10.0f;

    float currentZoom;

    Camera camera;

    void Start () {
        camera = gameObject.GetComponent<Camera>();
        currentZoom = camera.orthographicSize;
	}
	
	void Update () {
        if(frozen)
            return;

        // Scroll wheel for zooming in/out
        float zoom = Input.GetAxis("Mouse ScrollWheel") * mouseZoomSensitivity;
        if(zoom != 0 && camera.orthographicSize - zoom > minZoom && camera.orthographicSize - zoom < maxZoom) {
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - zoom, minZoom, maxZoom);
            currentZoom = camera.orthographicSize;
        }

        // Hold right-mouse to move view
        if(Input.GetMouseButton(1)) {
            float horizontalMovement = Input.GetAxis("Mouse X") * mouseMoveSensitivity * zoomRatio * currentZoom;
            float verticalMovement = Input.GetAxis("Mouse Y") * mouseMoveSensitivity * zoomRatio * currentZoom;
            if(horizontalMovement != 0 && verticalMovement != 0) {
                camera.transform.position = new Vector3(
                    Mathf.Clamp(camera.transform.position.x - horizontalMovement, -1 * maxMove, 1 * maxMove),
                    Mathf.Clamp(camera.transform.position.y - verticalMovement, -1 * maxMove, 1 * maxMove),
                    camera.transform.position.z
                );
            }
        }

        // Touchscreen camera movement
        if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved) {
            float horizontalMovement = Input.GetTouch(0).deltaPosition.x * fingerMoveSensitivity * zoomRatio * currentZoom;
            float verticalMovement = Input.GetTouch(0).deltaPosition.y * fingerMoveSensitivity * zoomRatio * currentZoom;
            /* 
             * iPad has issue where when a second finger is placed down for zoom, it isn't recognized as a "second finger" right away.
             * So when users start to zoom, they get teleported!
             * Limit movement per frame to 2 units in any direction. This still allows for fast camera scrolling but still cancels the extremes.
             */
            if (horizontalMovement != 0 && horizontalMovement < 2.0f && horizontalMovement > -2.0f &&
                verticalMovement != 0 && verticalMovement < 2.0f && verticalMovement > -2.0f) {
                camera.transform.Translate(-1 * horizontalMovement, -1 * verticalMovement, 0);
            }
        }


        // Pinch to zoom, from http://answers.unity3d.com/questions/63909/pinch-zoom-camera.html
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) {
            Vector2 curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; // current distance between finger touches
            Vector2 prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); // difference in previous locations using delta positions
            float touchDelta = curDist.magnitude - prevDist.magnitude;
            float zoomAmount = touchDelta / Input.GetTouch(0).deltaTime;

            // Debug.Log("zA: " + zoomAmount);
            
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - zoomAmount * pinchSensitivity, minZoom, maxZoom);
            currentZoom = camera.orthographicSize;
        }
    }

    public static bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 1; // will always be at least 1 because of the grid ui
    }
}
