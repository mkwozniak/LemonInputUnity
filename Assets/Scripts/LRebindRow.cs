using LemonInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

public sealed class LRebindRow : MonoBehaviour
{
	public Text InputLabel;
	public string RebindableID;
	public string RebindableDisplayName;
	public LRebindable PrimaryButton;
	public LRebindable SecondaryButton;

	private LInput _input;
	private InputTypes _inputType;
	private Text _statusLabel;

	public void Initialize(LInput inputInstance, string inputID, 
		string displayName, int primaryID, int secondaryID,
		InputTypes inputType)
	{
		if(inputInstance == null)
		{
			Debug.LogError("Cannot Initialize LRebindRow. " +
				"Input instance was null.");
			return;
		}

		_input = inputInstance;
		_inputType = inputType;
		RebindableID = inputID;
		RebindableDisplayName = displayName;
		UpdateLabel();
		InitializeButtons(primaryID, secondaryID);
	}

	public void SetStatusLabel(Text label)
	{
		_statusLabel = label;
	}

	public void UpdateButtonLabels()
	{
		PrimaryButton.UpdateText();
		SecondaryButton.UpdateText();
	}

	private void InitializeButtons(int primaryID, int secondaryID)
	{
		PrimaryButton.Initialize(RebindableID, primaryID, _input, _inputType);
		PrimaryButton.SetStatusLabel(_statusLabel);

		SecondaryButton.Initialize(RebindableID, secondaryID, _input, _inputType);
		SecondaryButton.SetStatusLabel(_statusLabel);
	}

	private void UpdateLabel()
	{
		InputLabel.text = RebindableDisplayName;
	}
}
