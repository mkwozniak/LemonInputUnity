using Application = UnityEngine.Application;

namespace LemonInput
{
	public sealed partial class LInput
	{
		/// <summary> The control scheme. </summary>
		public Controls Controls;

		// Add new names, then register them in RegisterInputs
		public string Escape;
		public string Horizontal;
		public string Vertical;
		public string Shoot;
		public string CursorPosition;
		public string CursorScroll;

		public string SaveFileDataPath = "";
		public string SaveFileName = "controls";
		public string SaveFileExtension = "dat";

		public string CancelRebindInputPath = "";

		/// <summary>
		/// Registers all inputs to their respective names.
		/// </summary>
		public void RegisterInputs()
		{
			// Save File Path
			SaveFileDataPath = Application.persistentDataPath;

			// Value inputs
			RegisterValue(Controls.Global.CursorPosition, ref CursorPosition);
			RegisterValue(Controls.Global.CursorScroll, ref CursorScroll);

			RegisterValue(Controls.Player.Horizontal, ref Horizontal);
			RegisterValue(Controls.Player.Vertical, ref Vertical);

			// Action inputs
			RegisterAction(Controls.Global.Escape, ref Escape);
			RegisterAction(Controls.Player.Shoot, ref Shoot);

			// The default input to cancel rebinding. (Note this input cannot be rebinded by default)
			CancelRebindInputPath = Actions[Escape].bindings[0].effectivePath;

			// Rebindable keyboard inputs
			RegisterRebindable(Controls.Player.Horizontal.id, Controls.Player.Horizontal.bindings[0].effectivePath, 0, isDefault: true);
			RegisterRebindable(Controls.Player.Horizontal.id, Controls.Player.Horizontal.bindings[3].effectivePath, 3, isDefault: true);
			RegisterRebindable(Controls.Player.Vertical.id, Controls.Player.Vertical.bindings[0].effectivePath, 0, isDefault: true);
			RegisterRebindable(Controls.Player.Vertical.id, Controls.Player.Vertical.bindings[3].effectivePath, 3, isDefault: true);
			RegisterRebindable(Controls.Player.Shoot.id, Controls.Player.Shoot.bindings[0].effectivePath, 0, isDefault: true);
			RegisterRebindable(Controls.Player.Shoot.id, Controls.Player.Shoot.bindings[1].effectivePath, 1, isDefault: true);

			// Rebindable gamepad inputs
			RegisterRebindable(Controls.Player.Horizontal.id, Controls.Player.Horizontal.bindings[7].effectivePath, 7, isDefault: true);
			RegisterRebindable(Controls.Player.Horizontal.id, Controls.Player.Horizontal.bindings[8].effectivePath, 8, isDefault: true);
			RegisterRebindable(Controls.Player.Vertical.id, Controls.Player.Vertical.bindings[7].effectivePath, 7, isDefault: true);
			RegisterRebindable(Controls.Player.Vertical.id, Controls.Player.Vertical.bindings[8].effectivePath, 8, isDefault: true);
			RegisterRebindable(Controls.Player.Shoot.id, Controls.Player.Shoot.bindings[2].effectivePath, 2, isDefault: true);
		}
	}
}

