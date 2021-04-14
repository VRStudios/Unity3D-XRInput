﻿using System;
using UnityEngine;
using VRstudios.API;

namespace VRstudios
{
    public enum XRInputAPIType
    {
        //AutoDetect,// TODO
        InputManager_Old,
        InputSystem_Package,
        OpenVR_Legacy
    }

    [DefaultExecutionOrder(-99)]
    public sealed class XRInput : MonoBehaviour
    {
        public static XRInput singleton { get; private set; }

        public XRInputAPIType apiType;
        private XRInputAPI api;
        private bool disposeAPI;

        private const int controllerStateLength = 4;
        private XRControllerState[] state_controllers = new XRControllerState[controllerStateLength];
        private XRControllerState state_controllerLeft, state_controllerRight;
        private XRControllerState state_controllerFirst, state_controllerMerged;

        private void Start()
        {
            // only one can exist in scene at a time
            if (singleton != null)
            {
                disposeAPI = false;// don't dispose apis if we're not the owner of possible native instances
                Destroy(gameObject);
                return;
		    }
            DontDestroyOnLoad(gameObject);
            singleton = this;

            disposeAPI = true;
            switch (apiType)
            {
                case XRInputAPIType.InputManager_Old: api = new InputManager_Old(); break;
                case XRInputAPIType.InputSystem_Package: api = new InputSystem_Package(); break;
                case XRInputAPIType.OpenVR_Legacy: api = new OpenVR_Legacy(); break;
                default: throw new NotImplementedException();
            }

            api.Init();
        }

	    private void OnDestroy()
	    {
            if (disposeAPI && api != null)
            {
                api.Dispose();
                api = null;
            }
        }

	    private void Update()
        {
            if (api == null) return;

            int controllerCount;
            bool leftSet, rightSet;
            int leftSetIndex, rightSetIndex;
            SideToSet sideToSet;
            if (!api.GatherInput(state_controllers, out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet)) return;

            // if left or right not known use controller index as side
            if (sideToSet == SideToSet.Left || sideToSet == SideToSet.Both) state_controllerLeft = state_controllers[leftSetIndex];
            if (sideToSet == SideToSet.Right || sideToSet == SideToSet.Both) state_controllerRight = state_controllers[rightSetIndex];

            // null memory if no state
            if (!leftSet) state_controllerLeft = new XRControllerState();
            if (!rightSet) state_controllerRight = new XRControllerState();

            // buffer special controller states
            if (controllerCount != 0) state_controllerFirst = state_controllers[0];
            else state_controllerFirst = new XRControllerState();

            state_controllerMerged = new XRControllerState();
            for (uint i = 0; i != controllerCount; ++i)
            {
                var controllerState = singleton.state_controllers[i];
                if (controllerState.connected) state_controllerMerged.connected = true;

                controllerState.buttonTrigger.Merge(ref state_controllerMerged.buttonTrigger);
                controllerState.buttonJoystick.Merge(ref state_controllerMerged.buttonJoystick);
                controllerState.buttonJoystick2.Merge(ref state_controllerMerged.buttonJoystick2);
                controllerState.buttonGrip.Merge(ref state_controllerMerged.buttonGrip);
                controllerState.buttonMenu.Merge(ref state_controllerMerged.buttonMenu);

                controllerState.button1.Merge(ref state_controllerMerged.button1);
                controllerState.button2.Merge(ref state_controllerMerged.button2);
                controllerState.button3.Merge(ref state_controllerMerged.button3);
                controllerState.button4.Merge(ref state_controllerMerged.button4);

                controllerState.touchTrigger.Merge(ref state_controllerMerged.touchTrigger);
                controllerState.touchJoystick.Merge(ref state_controllerMerged.touchJoystick);
                controllerState.touchJoystick2.Merge(ref state_controllerMerged.touchJoystick2);
                controllerState.touchGrip.Merge(ref state_controllerMerged.touchGrip);
                controllerState.touchMenu.Merge(ref state_controllerMerged.touchMenu);

                controllerState.touch1.Merge(ref state_controllerMerged.touch1);
                controllerState.touch2.Merge(ref state_controllerMerged.touch2);
                controllerState.touch3.Merge(ref state_controllerMerged.touch3);
                controllerState.touch4.Merge(ref state_controllerMerged.touch4);

                controllerState.trigger.Merge(ref state_controllerMerged.trigger);
                controllerState.grip.Merge(ref state_controllerMerged.grip);
                controllerState.joystick.Merge(ref state_controllerMerged.joystick);
                controllerState.joystick2.Merge(ref state_controllerMerged.joystick2);
            }

            // fire events
            // <<< buttons
            TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref state_controllerRight.buttonTrigger, XRControllerSide.Right);
            TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref state_controllerLeft.buttonTrigger, XRControllerSide.Left);

            TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref state_controllerRight.buttonJoystick, XRControllerSide.Right);
            TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref state_controllerLeft.buttonJoystick, XRControllerSide.Left);

            TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref state_controllerRight.buttonJoystick2, XRControllerSide.Right);
            TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref state_controllerLeft.buttonJoystick2, XRControllerSide.Left);

            TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref state_controllerRight.buttonGrip, XRControllerSide.Right);
            TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref state_controllerLeft.buttonGrip, XRControllerSide.Left);

            TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref state_controllerRight.buttonMenu, XRControllerSide.Right);
            TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref state_controllerLeft.buttonMenu, XRControllerSide.Left);

            TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref state_controllerRight.button1, XRControllerSide.Right);
            TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref state_controllerLeft.button1, XRControllerSide.Left);

            TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref state_controllerRight.button2, XRControllerSide.Right);
            TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref state_controllerLeft.button2, XRControllerSide.Left);

            TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref state_controllerRight.button3, XRControllerSide.Right);
            TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref state_controllerLeft.button3, XRControllerSide.Left);

            TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref state_controllerRight.button4, XRControllerSide.Right);
            TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref state_controllerLeft.button4, XRControllerSide.Left);

            // <<< touch
            TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref state_controllerRight.touchTrigger, XRControllerSide.Right);
            TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref state_controllerLeft.touchTrigger, XRControllerSide.Left);

            TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref state_controllerRight.touchJoystick, XRControllerSide.Right);
            TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref state_controllerLeft.touchJoystick, XRControllerSide.Left);

            TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref state_controllerRight.touchJoystick2, XRControllerSide.Right);
            TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref state_controllerLeft.touchJoystick2, XRControllerSide.Left);

            TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref state_controllerRight.touchGrip, XRControllerSide.Right);
            TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref state_controllerLeft.touchGrip, XRControllerSide.Left);

            TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref state_controllerRight.touchMenu, XRControllerSide.Right);
            TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref state_controllerLeft.touchMenu, XRControllerSide.Left);

            TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref state_controllerRight.touch1, XRControllerSide.Right);
            TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref state_controllerLeft.touch1, XRControllerSide.Left);

            TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref state_controllerRight.touch2, XRControllerSide.Right);
            TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref state_controllerLeft.touch2, XRControllerSide.Left);

            TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref state_controllerRight.touch3, XRControllerSide.Right);
            TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref state_controllerLeft.touch3, XRControllerSide.Left);

            TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref state_controllerRight.touch4, XRControllerSide.Right);
            TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref state_controllerLeft.touch4, XRControllerSide.Left);

            // <<< analogs
            TestAnalogEvent(TriggerActiveEvent, ref state_controllerRight.trigger, XRControllerSide.Right);
            TestAnalogEvent(TriggerActiveEvent, ref state_controllerLeft.trigger, XRControllerSide.Left);

            TestAnalogEvent(GripActiveEvent, ref state_controllerRight.grip, XRControllerSide.Right);
            TestAnalogEvent(GripActiveEvent, ref state_controllerLeft.grip, XRControllerSide.Left);

            // <<< joysticks
            TestJoystickEvent(JoystickActiveEvent, ref state_controllerRight.joystick, XRControllerSide.Right);
            TestJoystickEvent(JoystickActiveEvent, ref state_controllerLeft.joystick, XRControllerSide.Left);

            TestJoystickEvent(Joystick2ActiveEvent, ref state_controllerRight.joystick2, XRControllerSide.Right);
            TestJoystickEvent(Joystick2ActiveEvent, ref state_controllerLeft.joystick2, XRControllerSide.Left);
        }

		#region Public static interface
        public delegate void ButtonEvent(XRControllerSide side);
        public delegate void AnalogEvent(XRControllerSide side, float value);
        public delegate void JoystickEvent(XRControllerSide side, Vector2 value);

        public static event ButtonEvent ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent;
        public static event ButtonEvent ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent;
        public static event ButtonEvent ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent;
        public static event ButtonEvent ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent;
        public static event ButtonEvent ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent;
        public static event ButtonEvent Button1OnEvent, Button1DownEvent, Button1UpEvent;
        public static event ButtonEvent Button2OnEvent, Button2DownEvent, Button2UpEvent;
        public static event ButtonEvent Button3OnEvent, Button3DownEvent, Button3UpEvent;
        public static event ButtonEvent Button4OnEvent, Button4DownEvent, Button4UpEvent;

        public static event ButtonEvent TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent;
        public static event ButtonEvent TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent;
        public static event ButtonEvent TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent;
        public static event ButtonEvent TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent;
        public static event ButtonEvent TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent;
        public static event ButtonEvent Touch1OnEvent, Touch1DownEvent, Touch1UpEvent;
        public static event ButtonEvent Touch2OnEvent, Touch2DownEvent, Touch2UpEvent;
        public static event ButtonEvent Touch3OnEvent, Touch3DownEvent, Touch3UpEvent;
        public static event ButtonEvent Touch4OnEvent, Touch4DownEvent, Touch4UpEvent;

        public static event AnalogEvent TriggerActiveEvent, GripActiveEvent;
        public static event JoystickEvent JoystickActiveEvent, Joystick2ActiveEvent;

        private static void TestButtonEvent(ButtonEvent onEvent, ButtonEvent downEvent, ButtonEvent upEvent, ref XRControllerButton button, XRControllerSide side)
        {
            if (onEvent != null && button.on) onEvent(side);
            if (downEvent != null && button.down) downEvent(side);
            if (upEvent != null && button.up) upEvent(side);
        }

        private static void TestAnalogEvent(AnalogEvent e, ref XRControllerAnalog analog, XRControllerSide side)
        {
            if (e != null && analog.value != 0) e(side, analog.value);
        }

        private static void TestJoystickEvent(JoystickEvent e, ref XRControllerJoystick joystick, XRControllerSide side)
        {
            if (e != null && joystick.value.magnitude != 0) e(side, joystick.value);
        }

        /// <summary>
        /// Gets full controller state
        /// </summary>
        public static XRControllerState ControllerState(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst;
                case XRController.Left: return singleton.state_controllerLeft;
                case XRController.Right: return singleton.state_controllerRight;
                case XRController.Merged: return singleton.state_controllerMerged;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
	    }

        public static XRControllerButton ButtonTrigger(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonTrigger;
                case XRController.Left: return singleton.state_controllerLeft.buttonTrigger;
                case XRController.Right: return singleton.state_controllerRight.buttonTrigger;
                case XRController.Merged: return singleton.state_controllerMerged.buttonTrigger;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonJoystick(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonJoystick;
                case XRController.Left: return singleton.state_controllerLeft.buttonJoystick;
                case XRController.Right: return singleton.state_controllerRight.buttonJoystick;
                case XRController.Merged: return singleton.state_controllerMerged.buttonJoystick;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonJoystick2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonJoystick2;
                case XRController.Left: return singleton.state_controllerLeft.buttonJoystick2;
                case XRController.Right: return singleton.state_controllerRight.buttonJoystick2;
                case XRController.Merged: return singleton.state_controllerMerged.buttonJoystick2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonGrip(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonGrip;
                case XRController.Left: return singleton.state_controllerLeft.buttonGrip;
                case XRController.Right: return singleton.state_controllerRight.buttonGrip;
                case XRController.Merged: return singleton.state_controllerMerged.buttonGrip;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton ButtonMenu(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonMenu;
                case XRController.Left: return singleton.state_controllerLeft.buttonMenu;
                case XRController.Right: return singleton.state_controllerRight.buttonMenu;
                case XRController.Merged: return singleton.state_controllerMerged.buttonMenu;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button1(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button1;
                case XRController.Left: return singleton.state_controllerLeft.button1;
                case XRController.Right: return singleton.state_controllerRight.button1;
                case XRController.Merged: return singleton.state_controllerMerged.button1;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button2;
                case XRController.Left: return singleton.state_controllerLeft.button2;
                case XRController.Right: return singleton.state_controllerRight.button2;
                case XRController.Merged: return singleton.state_controllerMerged.button2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button3(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button3;
                case XRController.Left: return singleton.state_controllerLeft.button3;
                case XRController.Right: return singleton.state_controllerRight.button3;
                case XRController.Merged: return singleton.state_controllerMerged.button3;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Button4(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button4;
                case XRController.Left: return singleton.state_controllerLeft.button4;
                case XRController.Right: return singleton.state_controllerRight.button4;
                case XRController.Merged: return singleton.state_controllerMerged.button4;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchTrigger(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchTrigger;
                case XRController.Left: return singleton.state_controllerLeft.touchTrigger;
                case XRController.Right: return singleton.state_controllerRight.touchTrigger;
                case XRController.Merged: return singleton.state_controllerMerged.touchTrigger;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchJoystick(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchJoystick;
                case XRController.Left: return singleton.state_controllerLeft.touchJoystick;
                case XRController.Right: return singleton.state_controllerRight.touchJoystick;
                case XRController.Merged: return singleton.state_controllerMerged.touchJoystick;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchJoystick2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchJoystick2;
                case XRController.Left: return singleton.state_controllerLeft.touchJoystick2;
                case XRController.Right: return singleton.state_controllerRight.touchJoystick2;
                case XRController.Merged: return singleton.state_controllerMerged.touchJoystick2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchGrip(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchGrip;
                case XRController.Left: return singleton.state_controllerLeft.touchGrip;
                case XRController.Right: return singleton.state_controllerRight.touchGrip;
                case XRController.Merged: return singleton.state_controllerMerged.touchGrip;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton TouchMenu(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchMenu;
                case XRController.Left: return singleton.state_controllerLeft.touchMenu;
                case XRController.Right: return singleton.state_controllerRight.touchMenu;
                case XRController.Merged: return singleton.state_controllerMerged.touchMenu;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch1(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch1;
                case XRController.Left: return singleton.state_controllerLeft.touch1;
                case XRController.Right: return singleton.state_controllerRight.touch1;
                case XRController.Merged: return singleton.state_controllerMerged.touch1;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch2;
                case XRController.Left: return singleton.state_controllerLeft.touch2;
                case XRController.Right: return singleton.state_controllerRight.touch2;
                case XRController.Merged: return singleton.state_controllerMerged.touch2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch3(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch3;
                case XRController.Left: return singleton.state_controllerLeft.touch3;
                case XRController.Right: return singleton.state_controllerRight.touch3;
                case XRController.Merged: return singleton.state_controllerMerged.touch3;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerButton Touch4(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch4;
                case XRController.Left: return singleton.state_controllerLeft.touch4;
                case XRController.Right: return singleton.state_controllerRight.touch4;
                case XRController.Merged: return singleton.state_controllerMerged.touch4;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerAnalog Trigger(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.trigger;
                case XRController.Left: return singleton.state_controllerLeft.trigger;
                case XRController.Right: return singleton.state_controllerRight.trigger;
                case XRController.Merged: return singleton.state_controllerMerged.trigger;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerAnalog Grip(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.grip;
                case XRController.Left: return singleton.state_controllerLeft.grip;
                case XRController.Right: return singleton.state_controllerRight.grip;
                case XRController.Merged: return singleton.state_controllerMerged.grip;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerJoystick Joystick(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.joystick;
                case XRController.Left: return singleton.state_controllerLeft.joystick;
                case XRController.Right: return singleton.state_controllerRight.joystick;
                case XRController.Merged: return singleton.state_controllerMerged.joystick;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static XRControllerJoystick Joystick2(XRController controller)
        {
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.joystick2;
                case XRController.Left: return singleton.state_controllerLeft.joystick2;
                case XRController.Right: return singleton.state_controllerRight.joystick2;
                case XRController.Merged: return singleton.state_controllerMerged.joystick2;
            }
            throw new NotImplementedException("XR Controller type not implemented" + controller.ToString());
        }

        public static bool SetRumble(XRControllerRumbleSide controller, float strength, float duration = .1f)
        {
            return singleton.api.SetRumble(controller, strength, duration);
        }
        #endregion
    }

	public enum XRController
    {
        /// <summary>
        /// First controller connected
        /// </summary>
        First,

        /// <summary>
        /// All controller states merged
        /// </summary>
        Merged,

        /// <summary>
        /// Left controller states only
        /// </summary>
        Left,

        /// <summary>
        /// Right controller states only
        /// </summary>
        Right
    }

    public enum XRControllerSide
    {
        Unknown,
        Left,
        Right
	}

    public enum XRControllerRumbleSide
    {
        Both,
        Left,
        Right
    }

    public struct XRControllerState
    {
        public bool connected;
        public XRControllerSide side;
        public XRControllerButton touchTrigger, touchJoystick, touchJoystick2, touchGrip, touchMenu;
        public XRControllerButton touch1, touch2, touch3, touch4;
        public XRControllerButton buttonTrigger, buttonJoystick, buttonJoystick2, buttonGrip, buttonMenu;
        public XRControllerButton button1, button2 ,button3, button4;
        public XRControllerAnalog trigger, grip;
        public XRControllerJoystick joystick, joystick2;
    }

    public struct XRControllerButton
    {
        public bool on, down, up;

        internal void Update(bool on)
        {
            down = false;
            up = false;
            if (this.on != on)
            {
                if (on) down = true;
                else if (!on) up = true;
            }
            this.on = on;
	    }

        internal void Merge(ref XRControllerButton button)
        {
            if (on) button.on = true;
            if (down) button.down = true;
            if (up) button.up = true;
		}
    }

    public struct XRControllerAnalog
    {
        public float value;
        public static float tolerance = 0.2f;

        internal void Update(float value)
        {
            if (value < tolerance) value = 0.0f;
            this.value = value;
	    }

        internal void Merge(ref XRControllerAnalog analog)
        {
            if (value >= tolerance) analog.value = value;
        }
    }

    public struct XRControllerJoystick
    {
        public Vector2 value;
        public static float tolerance = 0.2f;

        internal void Update(Vector2 value)
        {
            if (value.magnitude < tolerance) value = Vector2.zero;
            this.value = value;
        }

        internal void Merge(ref XRControllerJoystick joystick)
        {
            if (value.magnitude >= tolerance) joystick.value = value;
        }
    }
}