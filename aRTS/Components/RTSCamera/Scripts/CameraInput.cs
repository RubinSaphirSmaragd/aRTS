using Godot;
using System;

public class CameraInput : Node
{

	// Variable export, to show them in the editor.
	[Export(PropertyHint.Range, "-10,20,0.2")]
	private float screenEdgeSize = 0.3f;
	[Export]
	private float mouseWheelDamping = 0.9f;

	// Used to calculate raw movement while pushing an edge
	private float horizontal = 0.0f;
	private float vertical = 0.0f;

	// Used to store mouse wheel inertia to enable smooth stopping
	private float mouseWheel = 0.0f;


	private CameraState cameraState;


	// Signals
	[Signal]
	delegate void onChangeVelocity(float velocity);
	[Signal]
	delegate void onRotateView(float relative);
	[Signal]
	delegate void onChangeAction(CameraState newState);
	[Signal]
	delegate void onZoom(float val);




	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Connect("onChangeAction", this, "changeAction");
		this.Connect("onChangeAction", GetNode<Camera>("Camera"), "changeAction");

		// TODO: Muss vielleicht nochmal angepasst werden
		EmitSignal("onChangeAction", "CameraState.MOVING");

	}


	private void changeAction(CameraState newState)
	{
		cameraState = newState;
	}

	// TODO: Braucht man das?
	private void toggleAction()
	{
		cameraState = 1 - cameraState;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		// Use JustPressed for an efficient way to change the actions
		if (Input.IsActionJustPressed("ToggleCameraAction"))
		{
			EmitSignal("onChangeAction", CameraState.ROTATING_VIEW);

		}
		else if (Input.IsActionJustReleased("ToggleCameraAction"))
		{
			EmitSignal("onChangeAction", CameraState.MOVING);
		}

		// TODO: Herausfinden wofür das gebraucht wird
		switch (cameraState)
		{

			case CameraState.MOVING:
			//TODO: Das muss ich mir nochmal anschauen
				if (horizontal != 0 || vertical != 0){
				EmitSignal("onChangeVelocity",new Vector2(horizontal, vertical));
				}
				break;
			case CameraState.ROTATING_VIEW:
				break;
			default:
				GD.Print("Mache gar nichts");
				break;
		}
		// Mousewheel
		if (mouseWheel != 0)
		{
			mouseWheel *= mouseWheelDamping;
			EmitSignal("onZoom", mouseWheel);
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseMotion)
		{
			if (cameraState == CameraState.ROTATING_VIEW)
			{
				InputEventMouseMotion mousePos = (InputEventMouseMotion)inputEvent;
				EmitSignal("onRotateView", mousePos.Relative);

				// Get screen size
				Vector2 viewSize = GetViewport().GetVisibleRect().Size - Vector2.One;
				// Get mouse position in percentage relative to screen
				Vector2 relativePos = mousePos.Position / viewSize;
				// Convert to a range between -1 and 1
				relativePos = (relativePos * 2) - Vector2.One;


				if (cameraState == CameraState.MOVING)
				{
					// Store move information in an buffer to use it in process
					horizontal = Math.Max(Math.Abs(relativePos.x) - (1.0f - screenEdgeSize), 0);
					vertical = Math.Max(Math.Abs(relativePos.y) - (1.0f - screenEdgeSize), 0);

					// Converts values to a range from 0 to 1
					horizontal = rangeLerp(horizontal, 0.0f, screenEdgeSize, 0.0f, 1.0f);
					vertical = rangeLerp(vertical, 0.0f, screenEdgeSize, 0.0f, 1.0f);

					// Apply the direction
					horizontal *= sign(relativePos.x);
					vertical *= sign(relativePos.y);
				}

				else if (cameraState == CameraState.ROTATING_VIEW)
				{
					horizontal = relativePos.x;
					vertical = relativePos.y;
					return;
				}
			}


		}

		if (inputEvent is InputEventMouseButton)
		{
			InputEventMouseButton mouseButton = (InputEventMouseButton)inputEvent;
			// Wheel Scroll
			if (mouseButton.ButtonIndex == (int)ButtonList.WheelUp || mouseButton.ButtonIndex == (int)ButtonList.WheelDown)
			{
				if (mouseButton.IsPressed() && !mouseButton.IsEcho())
				{
					float direction = mouseButton.ButtonIndex == (int)ButtonList.WheelUp ? -1 : 0 + mouseButton.ButtonIndex == (int)ButtonList.WheelDown ? 1 : 0;
					mouseWheel += direction * GetProcessDeltaTime() * 1000;
				}
			}
		}
	}

	private float rangeLerp(float value, float istart, float istop, float ostart, float ostop)
	{
		return ostart + (ostop - ostart) * value / (istop - istart);
	}

	private float sign(float val)
	{
		float returnVal = 0;
		if (val < 0)
		{
			returnVal = -1;
		}
		if (returnVal > 0)
		{
			returnVal = 1;
		}
		return returnVal;
	}
}
