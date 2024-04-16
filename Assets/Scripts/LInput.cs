using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace LemonInput
{
	public enum InputEventTypes
	{
		Started,
		Performed,
		Cancelled,
	}

	public enum InputTypes
	{
		Keyboard,
		Gamepad,
	}

	/// <summary>
	/// The LInput class responsible for all InputActions.
	/// </summary>
	public sealed partial class LInput
	{
		public static LInput Single;
		public bool IsSingleton;

		public Dictionary<string, InputAction> Actions = new();
		public Dictionary<string, Action> Started = new();
		public Dictionary<string, Action> Performed = new();
		public Dictionary<string, Action> Cancelled = new();
		public Dictionary<string, bool> Pressed = new();
		public Dictionary<string, bool> Released = new();
		public Dictionary<string, string> Bindings = new();
		public Dictionary<string, string> DefaultBindings = new();

		private bool _rebinding;
		private Action<InputAction> _onRebindSuccess;
		private Action<InputAction> _onRebindFailed;

		/// <summary>
		/// The constructor for a new LemonInput instance
		/// </summary>
		/// <param name="isSingleton">Whether or not this instance is a singleton instance. </param>
		public LInput(bool isSingleton = true)
		{
			IsSingleton = isSingleton;

			// setup singleton if so
			if(IsSingleton)
			{
				if(Single != null)
				{
					Single = null;
				}
				Single = this;
			}

			Initialize();
		}

		/// <summary> Checks if there is currently a rebind operation happening. </summary>
		/// <returns>True if there is a rebind operation in progress. </returns>
		public bool IsCurrentlyRebinding()
		{
			return _rebinding;
		}

		/// <summary> Listen to the successful rebind event. </summary>
		/// <param name="callback">The callback to listen with.</param>
		public void ListenRebindSuccess(Action<InputAction> callback)
		{
			_onRebindSuccess += callback;
		}

		/// <summary> Listen to the failed rebind event. </summary>
		/// <param name="callback">The callback to listen with.</param>
		public void ListenRebindFailure(Action<InputAction> callback)
		{
			_onRebindFailed += callback;
		}

		/// <summary> Mute the successful rebind event. </summary>
		/// <param name="callback">The callback to mute with.</param>
		public void MuteRebindSuccess(Action<InputAction> callback)
		{
			_onRebindSuccess -= callback;
		}

		/// <summary> Mute the failed rebind event. </summary>
		/// <param name="callback">The callback to mute with.</param>
		public void MuteRebindFailure(Action<InputAction> callback)
		{
			_onRebindFailed -= callback;
		}

		/// <summary>
		/// Checks if a particular input action was triggered.
		/// </summary>
		/// <param name="name">The name ID of the input.</param>
		/// <returns>True if the input was triggered.</returns>
		public bool Triggered(string name)
		{
			if(!ActionExists(name, $"Cannot read trigger {name}. " +
					$"Does not exist in 'Actions'."))
			{
				return false;
			}

			return Actions[name].triggered;
		}

		/// <summary>
		/// Checks if a particular input action is being pressed.
		/// </summary>
		/// <param name="name">The name ID of the input.</param>
		/// <returns>True if the input is being pressed down.</returns>
		public bool IsPressed(string name)
		{
			if(!ActionExists(name, $"Cannot check pressed for {name}. " +
					$"Does not exist in 'Actions'."))
			{
				return false;
			}

			return Pressed[name];
		}

		/// <summary>
		/// Checks if a particular input action was released.
		/// </summary>
		/// <param name="name">The name ID of the input.</param>
		/// <returns>True if the input was released. False otherwise.</returns>
		public bool IsReleased(string name)
		{
			if(!ActionExists(name, $"Cannot check released for {name}. " +
					$"Does not exist in 'Actions'."))
			{
				return false;
			}

			if (Pressed[name])
			{
				Released[name] = false;
				return false;
			}

			if (Released[name])
			{
				Released[name] = false;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Read the float value of an input action.
		/// This will throw an error if the InputAction is not a float.
		/// </summary>
		/// <param name="name">The name of the input.</param>
		/// <returns>The current float value of the particular input.</returns>
		public float ReadFloat(string name)
		{
			if(!ActionExists(name, $"Cannot read float {name}. " +
					$"Does not exist in 'Actions'."))
			{
				return 0f;
			}

			return Actions[name].ReadValue<float>();
		}

		/// <summary>
		/// Read the Vector2 value of an input action.
		/// This will throw an error if the InputAction is not a Vector2.
		/// </summary>
		/// <param name="name">The name of the input.</param>
		/// <returns>The current Vector2 value of the particular input.</returns>
		public Vector2 ReadVector2(string name)
		{
			if(!ActionExists(name, $"Cannot read Vector2 {name}. " +
					$"Does not exist in 'Actions'."))
			{
				return Vector2.zero;
			}

			return Actions[name].ReadValue<Vector2>();
		}

		/// <summary>
		/// Listen to any inputs Started, Performed or Cancelled event.
		/// The input must have been registered using RegisterAction.
		/// </summary>
		/// <param name="eventType">The event type to listen to. Started, Performed, or Cancelled.</param>
		/// <param name="input">The name of the input.</param>
		/// <param name="callback">The callback associated with the event.</param>
		public void ListenInputEvent(InputEventTypes eventType, string input, Action callback)
		{
			if(!ActionExists(input, $"Cannot 'Listen' to input {input}. Does not exist in 'Actions'." +
					$"Make sure to add it using RegisterAction."))
			{
				return;
			}

			ListenMuteInput(eventType, input, callback, true);
		}

		/// <summary>
		/// Mute any inputs Started, Performed or Cancelled event.
		/// The input must have been registered using RegisterAction.
		/// </summary>
		/// <param name="eventType">The event type to mute. Started, Performed, or Cancelled.</param>
		/// <param name="input">The name of the input.</param>
		/// <param name="callback">The callback associated with the event.</param>
		public void MuteInputEvent(InputEventTypes eventType, string input, Action callback)
		{
			if(!ActionExists(input, $"Cannot 'Mute' input {input}. Does not exist in 'Actions'." +
					$"Make sure to add it using RegisterAction."))
			{
				return;
			}

			ListenMuteInput(eventType, input, callback, false);
		}

		/// <summary>
		/// Starts the rebinding process for a particular input action.
		/// </summary>
		/// <param name="action">The input action to rebind.</param>
		/// <param name="index">The binding index of the action.</param>
		public void RebindKeyboardAction(InputAction action, int index)
		{
			_rebinding = true;
			var rebindOperation = action.PerformInteractiveRebinding(index)
				.WithBindingGroup(Controls.KeyboardScheme.bindingGroup)
				.WithCancelingThrough(CancelRebindInputPath)
				.OnCancel(operation => _onRebindFailed?.Invoke(action))
				.OnComplete(operation => {
					operation.Dispose();
					RegisterRebindable(action.id, action.bindings[index].effectivePath, index);
					SaveControls(Bindings);
					_onRebindSuccess?.Invoke(action);
				})
				.Start();
		}

		/// <summary>
		/// Starts the rebinding process for a particular input action.
		/// </summary>
		/// <param name="action">The input action to rebind.</param>
		/// <param name="index">The binding index of the action.</param>
		public void RebindAction(InputAction action, int index, InputControlScheme scheme, string cancelPath)
		{
			_rebinding = true;
			var rebindOperation = action.PerformInteractiveRebinding(index)
				.WithBindingGroup(scheme.bindingGroup)
				.WithCancelingThrough(cancelPath)
				.OnCancel(operation => _onRebindFailed?.Invoke(action))
				.OnComplete(operation => {
					operation.Dispose();
					RegisterRebindable(action.id, action.bindings[index].effectivePath, index);
					SaveControls(Bindings);
					_onRebindSuccess?.Invoke(action);
				})
				.Start();
		}

		/// <summary>
		/// Get the display name for the binding associated with an input.
		/// If there are multiple bindings for the input, use the index parameter.
		/// </summary>
		/// <param name="input">The name of the input.</param>
		/// <param name="index">The index of the binding</param>
		/// <returns></returns>
		public string GetBindingName(string input, int index = 0)
		{
			if(!ActionExists(input, $"Cannot get binding name {input}. " +
					$"Does not exist in 'Actions'."))
			{
				return "Binding Error";
			}
			return Actions[input].GetBindingDisplayString(index);
		}

		/// <summary>
		/// Get the action associated with a string input ID.
		/// </summary>
		/// <param name="input">The LemonInput string ID.</param>
		/// <returns>InputAction associated with the ID. Returns 'null' if no action exists.</returns>
		public InputAction GetAction(string input)
		{
			if(!ActionExists(input, $"Cannot get rebindable action {input}. " +
					$"Does not exist in 'Actions'."))
			{
				return null;
			}

			return Actions[input];
		}

		/// <summary> Saves all registered bindings. </summary>
		public void SaveAllRegisteredBindings()
		{
			SaveControls(Bindings);
		}

		/// <summary> Loads all registered bindings to the binding dictionary. </summary>
		public void LoadAllRegisteredBindings()
		{
			Bindings = LoadControls();
			ApplyAllBindingOverrides();
		}

		/// <summary> Loads all default bindings to the binding dictionary. </summary>
		public void LoadAllDefaultBindings()
		{
			Bindings = DefaultBindings;
			ApplyAllBindingOverrides();
		}

		/// <summary>
		/// Initializes the control scheme and registers all inputs.
		/// </summary>
		private void Initialize()
		{
			Controls = new Controls();
			RegisterInputs();
			_onRebindSuccess += FinishRebinding;
			_onRebindFailed += FinishRebinding;
		}

		private bool ActionExists(string id, string errorMsg)
		{
			if (!Actions.ContainsKey(id))
			{
				Debug.LogError(errorMsg);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Registers an InputAction with a name for event callbacks.
		/// Allows listening to Started, Performed and Cancelled events.
		/// </summary>
		/// <param name="action">The Unity InputAction to register.</param>
		/// <param name="name">The name that action will reference.</param>
		/// <param name="autoEnable">Whether or not to call Enable on the InputAction.</param>
		private void RegisterAction(InputAction action, ref string name, bool autoEnable = true)
		{
			name = action.name;
			Actions[name] = action;
			Actions[name].started += InputStart;
			Actions[name].performed += InputPerform;
			Actions[name].canceled += InputCancel;
			Pressed[name] = false;
			Released[name] = false;

			Started.Add(name, () => { });
			Performed.Add(name, () => { });
			Cancelled.Add(name, () => { });

			if (autoEnable)
			{
				Actions[name].Enable();
			}
		}

		/// <summary>
		/// Registers an InputAction with a name to read a value from.
		/// Inputs registered this way cannot be listened to.
		/// </summary>
		/// <param name="action">The Unity InputAction to register.</param>
		/// <param name="name">The name that action will reference.</param>
		/// <param name="autoEnable">Whether or not to call Enable on the InputAction.</param>
		private void RegisterValue(InputAction action, ref string name, bool autoEnable = true)
		{
			name = action.name;
			Actions[name] = action;
			if (autoEnable)
			{
				Actions[name].Enable();
			}
		}

		/// <summary>
		/// Registers an action as rebindable.
		/// </summary>
		/// <param name="actionId">The GUID of the action.</param>
		/// <param name="path">The effective path of the binding.</param>
		/// <param name="bindingIndex">The index of the binding.</param>
		/// <param name="isDefault">If this binding is the default binding.</param>
		private void RegisterRebindable(Guid actionId, string path, int bindingIndex, bool isDefault = false)
		{
			string key = string.Format("{0} : {1}", actionId.ToString(), bindingIndex);

			if (isDefault)
			{
				if(DefaultBindings.ContainsKey(key))
				{
					DefaultBindings[key] = path;
				}
				else
				{
					DefaultBindings.Add(key, path);
				}
			}

			if (Bindings.ContainsKey(key))
			{
				Bindings[key] = path;
				return;
			}

			Bindings.Add(key, path);
		}

		/// <summary>
		/// Listen or mute to any inputs Started, Performed or Cancelled event.
		/// </summary>
		/// <param name="eventType">The event type to listen to. Started, Performed, or Cancelled.</param>
		/// <param name="input">The name of the input.</param>
		/// <param name="callback">The callback associated with the event.</param>
		/// <param name="listen">Whether to listen or mute.</param>
		private void ListenMuteInput(InputEventTypes eventType, string input, Action callback, bool listen)
		{
			switch (eventType)
			{
				case InputEventTypes.Started:
					SetInputListener(ref Started, input, callback, listen);
					break;
				case InputEventTypes.Performed:
					SetInputListener(ref Performed, input, callback, listen);
					break;
				case InputEventTypes.Cancelled:
					SetInputListener(ref Cancelled, input, callback, listen);
					break;
			}
		}

		/// <summary>
		/// Subscribes or unsubscribes an input to a particular event map.
		/// </summary>
		/// <param name="map">The dictionary that stores the event.</param>
		/// <param name="name">The name of the input.</param>
		/// <param name="callback">The callback associated with the event.</param>
		/// <param name="listen">Whether or not to subscribe or unsubscribe.</param>
		private void SetInputListener(ref Dictionary<string, Action> map, string name, Action callback, bool listen)
		{
			if (!map.ContainsKey(name))
			{
				Debug.LogError($"Cannot add/remove a listener on {name}. " +
					$"Use RegisterAction not RegisterValue to listen/mute input events.");
				return;
			}

			if(listen)
			{
				map[name] += callback;
				return;
			}

			map[name] -= callback;
		}

		private void ApplyAllBindingOverrides()
		{
			foreach (var item in Bindings)
			{
				string[] split = item.Key.Split(new string[] { " : " }, StringSplitOptions.None);
				Guid id = Guid.Parse(split[0]);
				int index = int.Parse(split[1]);
				ApplyBindingOverride(id, index, item.Value);
			}
		}

		/// <summary>
		/// Sets a binding to a new overrided input path.
		/// </summary>
		/// <param name="guid">The action GUID.</param>
		/// <param name="index">The binding index.</param>
		/// <param name="path">The binding effective path.</param>
		private void ApplyBindingOverride(Guid guid, int index, string path)
		{
			Controls.FindAction(guid.ToString()).ApplyBindingOverride(index, path);
		}

		/// <summary> Internal callback for rebinding finish for both success or failure. </summary>
		/// <param name="action">The action that was being rebinded.</param>
		private void FinishRebinding(InputAction action)
		{
			_rebinding = false;
		}

		/// <summary>
		/// Saves the current binding dictionary to file.
		/// </summary>
		/// <param name="data">The binding dictionary to serialize and save.</param>
		private void SaveControls(Dictionary<string, string> data)
		{
			string path = $"{SaveFileDataPath}/{SaveFileName}.{SaveFileExtension}";
			FileStream file = new FileStream(path, FileMode.OpenOrCreate);
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(file, data);
			file.Close();
		}

		/// <summary>
		/// Loads the binding dictionary from file.
		/// </summary>
		/// <returns>The loaded binding dictionary.</returns>
		private Dictionary<string, string> LoadControls()
		{
			string path = $"{SaveFileDataPath}/{SaveFileName}.{SaveFileExtension}";
			Dictionary<string, string> data = new();

			if (!System.IO.File.Exists(path))
			{
				return data;
			}

			FileStream file = new FileStream(path, FileMode.OpenOrCreate);
			BinaryFormatter bf = new BinaryFormatter();
			data = bf.Deserialize(file) as Dictionary<string, string>;
			file.Close();
			return data;
		}

		/// <summary>
		/// Internal callback for when an input is started.
		/// </summary>
		/// <param name="action">The internal InputAction started.</param>
		private void InputStart(InputAction.CallbackContext action)
		{
			string name = action.action.name;
			if (Started.ContainsKey(action.action.name))
			{
				Started[name]();
			}
		}

		/// <summary>
		/// Internal callback for when an input is performed.
		/// </summary>
		/// <param name="action">The internal InputAction performed.</param>
		private void InputPerform(InputAction.CallbackContext action)
		{
			string name = action.action.name;
			if (Performed.ContainsKey(action.action.name))
			{
				Performed[name]();
				Pressed[name] = true;
				Released[name] = false;
			}
		}

		/// <summary>
		/// Internal callback for when an input is cancelled.
		/// </summary>
		/// <param name="action">The internal InputAction cancelled.</param>
		private void InputCancel(InputAction.CallbackContext action)
		{
			string name = action.action.name;
			if (Cancelled.ContainsKey(name))
			{
				Cancelled[name]();
				Pressed[name] = false;
				Released[name] = true;
			}
		}
	}
}

