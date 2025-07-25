﻿using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class UI : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject uiPanel;
    public float interactionDistance = 100f;
    public LayerMask interactLayer;

    private Transform player;
    private FirstPersonController firstPersonController;

    private bool isActive = false;

    private void OnEnable()
    {
        FirstPersonController.OnLocalPlayerReady += AssignPlayer;
    }

    private void OnDisable()
    {
        FirstPersonController.OnLocalPlayerReady -= AssignPlayer;
    }

    private void Start()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("UI Panel not assigned!");
        }

        // Изначально игрока нет
        player = null;
        firstPersonController = null;
    }

    private void AssignPlayer(Transform playerTransform, FirstPersonController controller)
    {
        player = playerTransform;
        firstPersonController = controller;
        Debug.Log("Player assigned via event: " + player.name);
    }

    private void Update()
    {
        if (player == null || firstPersonController == null)
        {
            return;
        }

        if (uiPanel == null)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);
        //Debug.Log($"Player pos: {player.position} | UI pos: {transform.position}");

        uiPanel.SetActive(isActive);

        if (distance <= interactionDistance)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                ToggleActive();
            }
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame && isActive)
        {
            isActive = false;
            RemoveFreeze();
        }
    }

    private void ToggleActive()
    {
        isActive = !isActive;

        if (isActive)
            AddFreeze();
        else
            RemoveFreeze();
    }

    private void AddFreeze()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        firstPersonController?.FreezeMovement();
    }

    private void RemoveFreeze()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        firstPersonController?.UnfreezeMovement();
    }
}
