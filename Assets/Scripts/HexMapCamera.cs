﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float rotationSpeed;

    public HexGrid grid;

    Transform swivel, stick;
    float zoom = 1f;
    float rotationAngle;

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        float rotationDelta = Input.GetAxis("Rotation");


        if (zoomDelta != 0) AdjustZoom(zoomDelta);
        if (xDelta != 0 || zDelta != 0) { AdjustPosition(xDelta, zDelta); }
        if (rotationDelta != 0) AdjustRotation(rotationDelta);
    }

    private void Awake()
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float moveSpeed = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom);

        float distance = moveSpeed * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;

        transform.localPosition = ClampPosition(position);
    }

    void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;

        if (rotationAngle < 0f) { rotationAngle += 360f; }
        else if (rotationAngle >= 360f) { rotationAngle -= 360f; }

        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.chunkCountX * HexMetrics.chunkSizeX - .5f) * (2f * HexMetrics.innerRadius);
        float zMax = (grid.chunkCountZ * HexMetrics.chunkSizeZ - 1) * (1.5f * HexMetrics.outerRadius);

        position.z = Mathf.Clamp(position.z, 0f, zMax);
        position.x = Mathf.Clamp(position.x, 0, xMax);

        return position;
    }
}
