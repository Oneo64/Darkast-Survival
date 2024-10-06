using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class PlayerControls
{
	public static bool isKeyboardInput {
		get {
			return Gamepad.current == null;
		}

		private set {}
	}

	public static bool GetInput(string name) {
		bool isKeyboardInput = Gamepad.current == null;

		switch (name) {
			case "move_forwards":
				if (isKeyboardInput) return Input.GetKey(KeyCode.W); else return Gamepad.current.leftStick.y.ReadValue() > 0.2f;

			case "move_backwards":
				if (isKeyboardInput) return Input.GetKey(KeyCode.S); else return Gamepad.current.leftStick.y.ReadValue() < -0.2f;

			case "move_left":
				if (isKeyboardInput) return Input.GetKey(KeyCode.A); else return Gamepad.current.leftStick.x.ReadValue() < -0.33f;

			case "move_right":
				if (isKeyboardInput) return Input.GetKey(KeyCode.D); else return Gamepad.current.leftStick.x.ReadValue() > 0.33f;

			case "run":
				if (isKeyboardInput) return Input.GetKey(KeyCode.LeftShift); else return Gamepad.current.leftStickButton.isPressed;

			case "jump":
				if (isKeyboardInput) return Input.GetKeyDown(KeyCode.Space); else return Gamepad.current.yButton.isPressed && Gamepad.current.yButton.wasPressedThisFrame;

			case "drop":
				if (isKeyboardInput) return Input.GetKeyDown(KeyCode.Q); else return Gamepad.current.bButton.isPressed && Gamepad.current.bButton.wasPressedThisFrame;

			case "throw":
				if (isKeyboardInput) return Input.GetKeyDown(KeyCode.F); else return Gamepad.current.xButton.isPressed;

			case "interact":
				if (isKeyboardInput) return Input.GetKeyDown(KeyCode.E); else return Gamepad.current.aButton.isPressed && Gamepad.current.aButton.wasPressedThisFrame;

			case "use":
				if (isKeyboardInput) return Input.GetKeyDown(KeyCode.Mouse0); else return Gamepad.current.rightShoulder.isPressed && Gamepad.current.rightShoulder.wasPressedThisFrame;

			case "use_automatic":
				if (isKeyboardInput) return Input.GetKey(KeyCode.Mouse0); else return Gamepad.current.rightShoulder.isPressed;

			case "aim":
				if (isKeyboardInput) return Input.GetKey(KeyCode.Mouse1); else return Gamepad.current.leftShoulder.isPressed;

			case "cycle_item_left":
				if (isKeyboardInput) return Input.GetAxis("Mouse ScrollWheel") > 0.01f; else return Gamepad.current.leftTrigger.isPressed;

			case "cycle_item_right":
				if (isKeyboardInput) return Input.GetAxis("Mouse ScrollWheel") < -0.01f; else return Gamepad.current.rightTrigger.isPressed;

			case "reload":
				if (isKeyboardInput) return Input.GetKeyDown(KeyCode.R); else return Gamepad.current.rightShoulder.isPressed && Gamepad.current.rightShoulder.wasPressedThisFrame;
		}

		return false;
	}

	public static float GetInputAxis(string name) {
		bool isKeyboardInput = Gamepad.current == null;

		switch (name) {
			case "look_x":
				if (isKeyboardInput) return Input.GetAxis("Mouse X"); else {
					float x = Gamepad.current.rightStick.x.ReadValue();

					return Mathf.Abs(x) > 0.1f ? x * ((GetInput("aim") ? 50 : 200) * Time.deltaTime) : 0;
				}

			case "look_y":
				if (isKeyboardInput) return Input.GetAxis("Mouse Y"); else {
					float y = Gamepad.current.rightStick.y.ReadValue();

					return Mathf.Abs(y) > 0.1f ? y * ((GetInput("aim") ? 50 : 200) * Time.deltaTime) : 0;
				}
		}

		return 0;
	}
}
