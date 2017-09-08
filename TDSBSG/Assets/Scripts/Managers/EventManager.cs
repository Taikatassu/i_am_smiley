﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public delegate void EmptyVoid();
    public delegate void IntVoid(int integer);
    public delegate void BoolVoid(bool boolean);
    public delegate void StringVoid(string str);
    public delegate void InputVoid(EInputType inputType);
    public delegate void IntBoolRobotTypeVoid(int integer, bool boolean, ERobotType eRobotType);
    public delegate void IntBoolVector3Void(int integer, bool boolean, Vector3 vec3);
    public delegate void IntRobotTypeVoid(int integer, ERobotType eRobotType);
    public delegate void GameObjectVoid(GameObject go);
    public delegate GameObject EmptyGameObject();
    public delegate void RobotTypeVoid(ERobotType eRobotType);

    public event EmptyVoid OnInitializeGame;
    public void BroadcastInitializeGame()
    {
        if (OnInitializeGame != null)
        {
            OnInitializeGame();
        }
    }

    public event EmptyVoid OnStartGame;
    public void BroadcastStartGame()
    {
        if (OnStartGame != null)
        {
            OnStartGame();
        }
    }

    public event IntBoolVector3Void OnMouseInputEvent;
    public void BroadcastMouseInputEvent(int button, bool down, Vector3 mousePosition)
    {
        if (OnMouseInputEvent != null)
        {
            OnMouseInputEvent(button, down, mousePosition);
        }
    }

    public event InputVoid OnInputEvent;
    public void BroadcastInputEvent(EInputType newInput)
    {
        if (OnInputEvent != null)
        {
            OnInputEvent(newInput);
        }
    }

    public event EmptyGameObject OnRequestPlayerReference;
    public GameObject BroadcastRequestPlayerReference()
    {
        if (OnRequestPlayerReference != null)
        {
            return OnRequestPlayerReference();
        }
        else
        {
            return FindObjectOfType<Player>().gameObject;
        }
    }

    public event RobotTypeVoid OnDisobeyingDetected;
    public void BroadcastDisobeyingDetected(ERobotType detectedRobotType)
    {
        if (OnDisobeyingDetected != null)
        {
            OnDisobeyingDetected(detectedRobotType);
        }
    }

    public event IntBoolRobotTypeVoid OnRoomEntered;
    public void BroadcastRoomEntered(int roomSecurityLevel, bool isAllowed, ERobotType enteringRobotType)
    {
        if (OnRoomEntered != null)
        {
            OnRoomEntered(roomSecurityLevel, isAllowed, enteringRobotType);
        }
    }

    public event IntBoolRobotTypeVoid OnDoorEntered;
    public void BroadcastDoorEntered(int doorSecurityLevel, bool isAllowed, ERobotType enteringRobotType)
    {
        if (OnDoorEntered != null)
        {
            OnDoorEntered(doorSecurityLevel, isAllowed, enteringRobotType);
        }
    }

    public event IntRobotTypeVoid OnAlertStateChange;
    public void BroadcastAlertStateChange(int newState, ERobotType wantedRobot)
    {
        Debug.Log("BroadcastAlertStateChange,  newState: " + newState);
        if (OnAlertStateChange != null)
        {
            OnAlertStateChange(newState, wantedRobot);
        }
    }

    public event IntVoid OnSecurityTierChange;
    public void BroadcastSecurityTierChange(int newTier)
    {
        if (OnSecurityTierChange != null)
        {
            OnSecurityTierChange(newTier);
        }
    }


    public event StringVoid OnRequestLoadLevel;
    public void BroadcastRequestLoadLevel(string nameOfLoadScene)
    {
        if (OnRequestLoadLevel != null)
        {
            OnRequestLoadLevel(nameOfLoadScene);
        }
    }

    public event BoolVoid OnPauseStateChange;
    public void BroadcastPauseStateChange(bool newPauseState)
    {
        if (OnPauseStateChange != null)
        {
            OnPauseStateChange(newPauseState);
        }
    }
}
