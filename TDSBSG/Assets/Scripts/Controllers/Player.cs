﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region References & variables
    Toolbox toolbox;
    EventManager em;
    PossessableInfo possInfo;
    IPossessable primaryPossession;
    List<IPossessable> secondaryPossessions = new List<IPossessable>();
    Transform primaryPossessionTransform;
    public LayerMask possessSelectionMask;

    float possessionRange = 5;
    #endregion

    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        possInfo = toolbox.GetComponent<PossessableInfo>();
    }

    private void OnEnable()
    {
        em.OnInitializeGame += OnInitializeGame;
        em.OnStartGame += OnStartGame;
        em.OnInputEvent += OnInputEvent;
        em.OnRequestPlayerReference += OnRequestPlayerReference;
        em.OnMouseInputEvent += OnMouseInputEvent;
    }

    private void OnDisable()
    {
        em.OnInitializeGame -= OnInitializeGame;
        em.OnStartGame -= OnStartGame;
        em.OnInputEvent -= OnInputEvent;
        em.OnRequestPlayerReference -= OnRequestPlayerReference;
        em.OnMouseInputEvent -= OnMouseInputEvent;
    }

    private void OnInitializeGame()
    {
        ResetAll();
    }

    private void OnStartGame()
    {
        TryPossessClosestPossessable();
    }

    private GameObject OnRequestPlayerReference()
    {
        return gameObject;
    }

    private bool TryPossessClosestPossessable()
    {
        if (possInfo.possessables.Count > 0)
        {
            IPossessable closestPossessable = possInfo.possessables[0];
            float distanceToClosest = -1;
            bool first = true;
            int count = possInfo.possessables.Count;
            for (int i = 0; i < count; i++)
            {
                if (possInfo.possessables[i] != primaryPossession)
                {
                    float distanceToThis = (transform.position -
                        possInfo.possessables[i].GetGameObject().transform.position).magnitude;
                    if (first || distanceToThis < distanceToClosest)
                    {
                        first = false;
                        closestPossessable = possInfo.possessables[i];

                        distanceToClosest = distanceToThis;

                    }
                }
            }

            if (distanceToClosest > possessionRange)
            {
                return false;
            }

            PossessPossessable(closestPossessable);
            return true;
        }
        Debug.LogWarning("No possessables found");
        return false;
    }

    private void FixedUpdate()
    {
        if (primaryPossession != null)
        {
            transform.position = primaryPossessionTransform.position;
        }
    }

    private void PossessPossessable(IPossessable newPossession)
    {
        switch (newPossession.GetPossessableType())
        {
            case EPossessableType.PRIMARY:
                if (primaryPossession != null)
                {
                    primaryPossession.UnPossess();
                }
                newPossession.Possess();
                primaryPossession = newPossession;
                primaryPossessionTransform = primaryPossession.GetGameObject().transform;
                break;
            case EPossessableType.SECONDARY:
                newPossession.Possess();
                secondaryPossessions.Add(newPossession);
                break;
            default:
                break;
        }
    }

    private void OnMouseInputEvent(int button, bool down, Vector3 mousePosition)
    {
        if (button == 0 && down)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, possessSelectionMask))
            {
                if (hit.collider.GetComponent(typeof(IPossessable)))
                {
                    IPossessable hitPossessable = hit.collider.GetComponent<IPossessable>();
                    float distanceToClickedObject = (hit.collider.transform.position - transform.position).magnitude;
                    if (distanceToClickedObject <= possessionRange)
                    {
                        PossessPossessable(hitPossessable);
                    }
                    else if (primaryPossession.GetConnectedPossessablesList().Contains(hitPossessable))
                    {
                        PossessPossessable(hitPossessable);
                    }
                }
            }
        }
    }

    private void OnInputEvent(EInputType newInput)
    {
        if (newInput == EInputType.POSSESS_KEYDOWN)
        {
            TryPossessClosestPossessable();
        }

        else if (primaryPossession != null)
        {
            primaryPossession.GiveInput(newInput);
        }

        //if (currentSecondaryPossessions.Count > 0)
        //{
        //    for (int i = 0; i < currentSecondaryPossessions.Count; i++)
        //    {
        //        currentSecondaryPossessions[i].GiveInput(newInput);
        //    }
        //}

        //TODO: Handle player only inputs if neccessary

    }

    private void ResetAll()
    {
        if (secondaryPossessions.Count > 0)
        {
            for (int i = 0; i < secondaryPossessions.Count; i++)
            {
                if (secondaryPossessions[i] != null)
                {
                    secondaryPossessions[i].UnPossess();
                }
            }
        }

        secondaryPossessions = new List<IPossessable>();
        primaryPossession = null;
        primaryPossessionTransform = null;
    }
}
