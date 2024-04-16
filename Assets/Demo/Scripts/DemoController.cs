using LemonInput;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Text = TMPro.TextMeshProUGUI;

public class DemoController : MonoBehaviour
{
	public float Speed;
	public Transform DemoBulletSpawnPoint;
	public DemoBullet BulletPrefab;
	public float DemoBulletSpeed;
	public float DemoBulletLifetime;

	public RectTransform RebindRowParent;
	public LRebindRow RebindRowPrefab;
	public Text MousePositionLabel;
	public Text MouseScrollLabel;
	public Text HorizontalBindingLabel;
	public Text VerticalBindingLabel;
	public Text ShootBindingLabel;
	public Text RebindStatusLabel;

	/// <summary> The LInput Instance. </summary>
	private LInput _input;
	private InputTypes _inputViewType;

	private List<LRebindRow> _rebindRows = new();

	private Rigidbody _rb;
	private Vector3 _velocity;

	public void Awake()
	{
		// Create the input instance
		_input = new LInput();
		_rb = GetComponent<Rigidbody>();
	}

	public void Start()
	{
		// example of listening to an input event
		_input.ListenInputEvent(InputEventTypes.Performed, _input.Shoot, ShootBullet);

		UpdateBindingLabels();
		CreateRebindableRows();
	}

	public void Update()
	{
		// example of reading vector2 and float values
		MousePositionLabel.text = $"{_input.ReadVector2(_input.CursorPosition)}";
		MouseScrollLabel.text = $"{_input.ReadFloat(_input.CursorScroll)}";

		// example of reading positive/negative axis float values
		_velocity.x = _input.ReadFloat(_input.Horizontal) * Speed;
		_velocity.z = _input.ReadFloat(_input.Vertical) * Speed;

		// example of using triggered input instead of events
		if(_input.Triggered(_input.Shoot))
		{
			Debug.Log("Shoot Input was pressed.");
		}

		// example of using pressed input instead of events
		if(_input.IsPressed(_input.Shoot))
		{
			Debug.Log("Shoot Input is being held down.");
		}
		
		// example of using released input instead of events
		if (_input.IsReleased(_input.Shoot))
		{
			Debug.Log("Shoot Input was released.");
		}

		_rb.velocity = _velocity;
	}

	public void EnablePlayerInputs(bool enable)
	{
		if(enable)
		{
			_input.Controls.Player.Enable();
			return;
		}

		_input.Controls.Player.Disable();
	}

	/// <summary> Saves all registered bindings. </summary>
	public void SaveBindings()
	{
		_input.SaveAllRegisteredBindings();
		RebindStatusLabel.text = "Successfully saved bindings to file.";
	}

	/// <summary> Loads all registered bindings. </summary>
	public void LoadBindings()
	{
		_input.LoadAllRegisteredBindings();
		RebindStatusLabel.text = "Successfully loaded bindings from file.";
		UpdateRowButtonLabels();
	}

	/// <summary> Loads all default registered bindings. </summary>
	public void LoadDefaultBindings()
	{
		_input.LoadAllDefaultBindings();
		RebindStatusLabel.text = "Successfully set bindings to default.";
		UpdateRowButtonLabels();
	}

	public void SwitchToKeyboardBindingsView()
	{
		ClearRebindRows();
		_inputViewType = InputTypes.Keyboard;
		CreateRebindableRows();
		UpdateBindingLabels();
	}

	public void SwitchToGamepadBindingsView()
	{
		ClearRebindRows();
		_inputViewType = InputTypes.Gamepad;
		CreateRebindableRows();
		UpdateBindingLabels();
	}

	/// <summary> Clears the binding status label text. </summary>
	public void ClearBindingStatusLabel()
	{
		RebindStatusLabel.text = "";
	}

	private void UpdateRowButtonLabels()
	{
		foreach (LRebindRow row in _rebindRows)
		{
			row.UpdateButtonLabels();
		}
	}

	private void ClearRebindRows()
	{
		for(int i = 0; i < RebindRowParent.childCount; i++)
		{
			Destroy(RebindRowParent.GetChild(i).gameObject);
		}
	}

	/// <summary> Updates the demo labels on the canvas. </summary>
	private void UpdateBindingLabels()
	{
		// example of retrieving binding names by index
		if(_inputViewType == InputTypes.Keyboard )
		{
			HorizontalBindingLabel.text = $"{_input.GetBindingName(_input.Horizontal, 0)} OR {_input.GetBindingName(_input.Horizontal, 3)}";
			VerticalBindingLabel.text = $"{_input.GetBindingName(_input.Vertical, 0)} OR {_input.GetBindingName(_input.Vertical, 3)}";
			ShootBindingLabel.text = $"{_input.GetBindingName(_input.Shoot, 0)} OR {_input.GetBindingName(_input.Shoot, 1)}";
			return;
		}

		HorizontalBindingLabel.text = $"{_input.GetBindingName(_input.Horizontal, 7)} OR {_input.GetBindingName(_input.Horizontal, 7)}";
		VerticalBindingLabel.text = $"{_input.GetBindingName(_input.Vertical, 7)} OR {_input.GetBindingName(_input.Vertical, 7)}";
		ShootBindingLabel.text = $"{_input.GetBindingName(_input.Shoot, 2)} OR {_input.GetBindingName(_input.Shoot, 2)}";
	}

	/// <summary> Creates all rebindable rows for the rebind canvas view. </summary>
	private void CreateRebindableRows()
	{
		// Axis rebinds must use the proper index
		// ie Horizontal:
		// 0 is both A/D for primary
		// 3 is both Left/Right for secondary
		// By using 1, you access A. By using 4, you access Left.
		// By using 2, you access D. By using 5, you access Right.

		if(_inputViewType == InputTypes.Keyboard)
		{
			CreateRebindableRow(_input.Horizontal, "Move Left", 1, 4);
			CreateRebindableRow(_input.Horizontal, "Move Right", 2, 5);
			CreateRebindableRow(_input.Vertical, "Move Down", 1, 4);
			CreateRebindableRow(_input.Vertical, "Move Up", 2, 5);
			CreateRebindableRow(_input.Shoot, "Shoot", 0, 1);
			return;
		}

		// These example gamepad inputs do not have secondaries
		// This simply creates two primary buttons, modify that how you like
		CreateRebindableRow(_input.Horizontal, "Move Left", 7, 7);
		CreateRebindableRow(_input.Horizontal, "Move Right", 8, 8);
		CreateRebindableRow(_input.Vertical, "Move Down", 7, 7);
		CreateRebindableRow(_input.Vertical, "Move Up", 8, 8);
		CreateRebindableRow(_input.Shoot, "Shoot", 2, 2);
	}

	/// <summary> Create a single rebindable row in the canvas view. </summary>
	/// <param name="ID">The name ID of the input.</param>
	/// <param name="displayName">The display name of the input.</param>
	/// <param name="primary">The primary binding index.</param>
	/// <param name="secondary">The secondary binding index.</param>
	private void CreateRebindableRow(string ID, string displayName, int primary, int secondary)
	{
		LRebindRow newRow = Instantiate(RebindRowPrefab, RebindRowParent);
		newRow.SetStatusLabel(RebindStatusLabel);
		newRow.Initialize(_input, ID, displayName, primary, secondary, _inputViewType);
		_rebindRows.Add(newRow);
	}

	/// <summary> Shoots a bullet for the demo. </summary>
	private void ShootBullet()
	{
		DemoBullet newBullet = Instantiate(BulletPrefab, DemoBulletSpawnPoint.position, Quaternion.identity);
		newBullet.Direction = transform.forward;
		newBullet.Speed = DemoBulletSpeed;
		newBullet.Lifetime = DemoBulletLifetime;
	}
}
