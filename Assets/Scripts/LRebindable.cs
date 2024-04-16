using UnityEngine;
using UnityEngine.InputSystem;
using Button = UnityEngine.UI.Button;
using Text = TMPro.TextMeshProUGUI;

namespace LemonInput
{
	[RequireComponent(typeof(Button))]
	public sealed class LRebindable : MonoBehaviour
	{
		public Text ButtonText;

		private string _inputID;
		private int _inputIndex;
		private Button _btn;
		private LInput _input;
		private InputAction _action;
		private Text _statusLabel;
		private InputTypes _inputType;

		public void Awake()
		{
			_btn = GetComponent<Button>();
			_btn.onClick.AddListener(RebindAction);
		}

		public void Initialize(string id, int index, LInput input, InputTypes type)
		{
			_input = input;
			_inputID = id;
			_inputIndex = index;
			_action = _input.GetAction(id);
			_inputType = type;
			UpdateText();
		}

		public void SetStatusLabel(Text label)
		{
			_statusLabel = label;
		}

		public void UpdateText()
		{
			ButtonText.text = _input.GetBindingName(_inputID, _inputIndex);
		}

		private void RebindAction()
		{
			if(_input == null)
			{
				Debug.LogError("Cannot start rebinding. " +
					"LRebindable button was not initialized with an input reference.");
				return;
			}

			// if there is already a rebind action happening return
			if (_input.IsCurrentlyRebinding())
			{
				UpdateText();
				return;
			}

			// update labels
			ButtonText.text = "...";
			if (_statusLabel)
			{
				_statusLabel.text = "Waiting for new binding ...";
			}

			// disable the associated action 
			_action.Disable();

			// listen to lemon inputs rebind events
			_input.ListenRebindSuccess(RebindFinish);
			_input.ListenRebindFailure(RebindFinish);

			// start lemon input rebinding for this action
			if(_inputType == InputTypes.Keyboard)
			{
				_input.RebindAction(_action, _inputIndex, _input.Controls.KeyboardScheme, _input.CancelRebindInputPath);
				return;
			}

			_input.RebindAction(_action, _inputIndex, _input.Controls.GamepadScheme, _input.CancelRebindInputPath);
		}

		private void RebindFinish(InputAction action)
		{
			Debug.Log($"{action.name}: Rebinding Finished.");

			// mute lemon input rebind events
			_input.MuteRebindSuccess(RebindFinish);
			_input.MuteRebindFailure(RebindFinish);

			// enable the associated action again
			_action.Enable();

			// update labels
			UpdateText();
			if (_statusLabel)
			{
				_statusLabel.text = "";
			}
		}
	}
}

