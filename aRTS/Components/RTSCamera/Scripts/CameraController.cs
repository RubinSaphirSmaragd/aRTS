using Godot;
using System;

public class CameraController : Camera
{
	enum CAMERA_ACTIONS
	{
		MOVING,
		ROTATING_VIEW,
	}

	[Export(PropertyHint.Range, "1,100,")]
	private float movementSpeed = 30f;
	[Export(PropertyHint.Range, "0.01,0.99,")]
	private float movementDamping = 0.74f;
	// Value in percentage of which the movement starts
	// value of 0.3 means that when you place the cursor 30% or less away from an edge it will start pushing the camera
	// [Export(PropertyHint.Range, "0.0,1.0,")]
	// private float edgeSize = 0.0f;

	[Export(PropertyHint.Range, "10,100,")]
	private float minZoom = 10f;
	[Export(PropertyHint.Range, "10,100,")]
	private float maxZoom = 10f;

	[Export(PropertyHint.Range, "1,3,")]
	private float zoomSensibility = 2.5f;

	[Export(PropertyHint.Range, "1,3,")]
	private float rotationSensibility = 2.5f;

	private float pitch;

	private float yaw;

	private CameraState cameraState = CameraState.MOVING;

	Vector2 velocity;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Capture Camera
		Input.SetMouseMode(Input.MouseMode.Confined);
		// Connect vars to camera
		pitch = Rotation.x;
		yaw = Rotation.y;

		Node node = GetChild(0);
		node.Connect("onChangeAction", this, "changeAction");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		switch (cameraState)
		{
			case CameraState.MOVING:
				velocity.x = Mathf.Clamp(velocity.x * movementDamping, -1.0f, 1.0f);
				velocity.y = Mathf.Clamp(velocity.y * movementDamping, -1.0f, 1.0f);

				if (velocity != Vector2.Zero)
				{
					move(velocity);
				}
				break;
		}
	}

	public void changeVelocity(Vector2 velo)
	{
		velocity = velo;
	}

	private void move(Vector2 velocityVector)
	{
		Transform newTrans = GlobalTransform;
		// Move along cameras X axis
		newTrans.origin += GlobalTransform.basis.x * velocity.x * movementSpeed * GetProcessDeltaTime();
		// Calculate a forward camera direction that is perpendicular to the XZ plane
		Vector3 forward = GlobalTransform.basis.x.Cross(Vector3.Up);
		//Move the camera along that forward direction
		newTrans.origin += forward * velocity.y * movementSpeed * GetProcessDeltaTime();

		GlobalTransform = newTrans;
	}

	public void zoom(float direction)
	{
		// Zooming using fov
		float newFov = Fov + (sign(direction) * Mathf.Pow(Math.Abs(direction), zoomSensibility) / 100 * GetProcessDeltaTime());
		Fov = Mathf.Clamp(newFov, minZoom, maxZoom);
	}

public void rotateView(Vector2 axis){
 float pitchRotationAmount = -axis.y / 100 * GetProcessDeltaTime() * rotationSensibility;
 float yawRotationAmount = -axis.x / 100 * GetProcessDeltaTime() * rotationSensibility;
 pitch += pitchRotationAmount;
 pitch = Mathf.Clamp(pitch, (float)(-Math.PI / 2), 0.0f);
 yaw += yawRotationAmount;

 Vector3 newRotation = Rotation;
 newRotation.x = pitch;
 newRotation.y = yaw;

 Rotation = newRotation;
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

private void changeAction(CameraState state)
{
	GD.Print("changeAction wurde ausgeführt!!");
			cameraState = state;

		switch (state)
		{
			case CameraState.MOVING:
				Input.SetMouseMode(Input.MouseMode.Confined);
				break;
			case CameraState.ROTATING_VIEW:
				Input.SetMouseMode(Input.MouseMode.Captured);
				break;
			default:
				GD.Print("Das sollte nicht passieren in changeAction der Klasse CameraController");
				break;
		}
}
}
