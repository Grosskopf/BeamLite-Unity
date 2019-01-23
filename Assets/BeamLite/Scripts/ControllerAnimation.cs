using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ControllerAnimation : MonoBehaviour {
    
    public Animator HandAnimator;
    public bool isRightHand=true;
    private bool isGrabbing = false;
    void Start () {

        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_ControllerEvents_ListenerExample", "VRTK_ControllerEvents", "the same"));
            return;
        }
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerTouchStart);
        GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerTouchEnd);
        GetComponent<VRTK_ControllerEvents>().GripTouchStart += new ControllerInteractionEventHandler(DoGripTouchStart);
        GetComponent<VRTK_ControllerEvents>().GripTouchEnd += new ControllerInteractionEventHandler(DoGripTouchEnd);

    }

    private void DoGripTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        HandAnimator.speed = 0.0f;
        if (isRightHand)
        {
            HandAnimator.Play("RighthandArmature|RighthandIdle", 0, 0.0f);
        }
        else
        {
            HandAnimator.Play("LefthandArmature|LefthandIdle", 0, 0.0f);
        }
        isGrabbing = false;
    }

    private void DoGripTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        HandAnimator.speed = 0.0f;
        if (isRightHand)
        {
            HandAnimator.Play("RighthandArmature|RighthandGrab", 0, 0.0f);
        }
        else
        {
            HandAnimator.Play("LefthandArmature|Lefthandgrab", 0, 0.0f);
        }
        isGrabbing = true;
    }

    private void DoTriggerTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (!isGrabbing)
        {
            HandAnimator.speed = 0.0f;
            if (isRightHand)
            {
                HandAnimator.Play("RighthandArmature|RighthandIdle", 0, 0.0f);
            }
            else
            {
                HandAnimator.Play("LefthandArmature|LefthandIdle", 0, 0.0f);
            }
        }
        else
        {
            DoGripTouchStart(sender, e);
        }
    }

    private void DoTriggerTouchStart(object sender, ControllerInteractionEventArgs e)
    {

        if (isRightHand)
        {
            HandAnimator.Play("RighthandArmature|RighthandPointing", 0, 0.0f);
        }
        else
        {
            HandAnimator.Play("LefthandArmature|LefthandPointing", 0, 0.0f);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
