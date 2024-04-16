# LemonInput Unity
LemonInput Unity is a reusable wrapper for the new Unity Input System.
It contains methods and examples that allow easy usage of the new input system.
The goal was to create a clean and reusable script for any new project.

LemonInput is fully commented, tooltipped, and segmented for ease of use.
It includes a full demo and examples of all functionality.

If you're looking to learn how to use the new input system, this is also a great resource.

# Features
- Mimic the old input system with *Triggered/IsPressed/IsReleased*, or use Events with callbacks.
- Add/Remove inputs easily within a single file after you've mapped them in your control scheme.
- Listen/Mute input events using an empty callback as opposed to the current 'InputAction.CallbackContext'.
- Easily retrieve any input name, binding, value, or action.
- Easily rebind any primary or secondary inputs.
- Easily access any inputs through a single string ID.
- Includes examples of both Primary and Secondary inputs for Keyboard and Gamepad Controllers.
- Includes examples of rebinding Keyboard and Gamepad Controllers.
- Includes examples of saving/loading controls to a file.
- One script with two included example scripts for full rebinding menu/buttons in the Unity canvas.
![5452543](https://github.com/mkwozniak/LemonInputUnity/assets/51724102/2bb08a51-36a6-4590-8fbf-36aef59d8c90)
![341434](https://github.com/mkwozniak/LemonInputUnity/assets/51724102/a5abce1f-851d-4978-9d41-8ab2cc698d16)





# Requirements
- Your project must support/have the new Unity InputSystem package installed.
- The demo scenes were created in Unity version **2022.3.13**.


# How To Use
To use LemonInput properly, you must have a defined a Unity control scheme file and created/generated it within your project.

> If you want to use your own name for your control scheme, simply change the type at the top of **LInputRegistry.cs**.

**LInput** is a single partial class that is separted into two files. 

**LInputRegistry.cs** is where you'll be adding/editing your inputs.

You shouldn't have to modify **LInput.cs**, but you may need to if the current setup is undesirable.

> LemonInput is not a **MonoBehavior**, and an instance needs to be created in your own **MonoBehavior** somewhere first. This can be easily modified.

> If you want a static Singleton, create the instance in your desired **MonoBehavior** and pass in *true* in the Constructor. You can now access **LInput** from any script using *LInput.Single*.

> If you want it to be both a **MonoBehavior** *and* a Singleton, you'll need to define an *Awake* method and create the Singleton there.

## Getting Started
Once you've added a new action to your control scheme, modify **LInputRegistry** by adding a new string variable.
This variable represents an ID. This ID can be used in **LemonInput** to do everything related to an input.

*Creating this variable is optional, but highly recommended. If you choose not to do this, you'll need to pass a raw string everytime.*

Then, all you need to do is register this input, and it's ready to go.

> Use *RegisterValue* for value type inputs, like mouse position, scroll wheel or horizontal/vertical axis.

> Use *RegisterAction* for button type inputs, like the space bar or shift.

> Use *RegisterRebindable* in addition with those above to register an input as rebindable.


## Important Methods

### RegisterValue(*InputAction*, ref *string ID*)
*RegisterValue* takes an **InputAction** from your control scheme and a reference to the string ID that you created.
This is for *value only* inputs, such as mouse position, scroll wheel, etc. You can now use *ReadFloat* or *ReadVector2* to read the value of this input at any time.
> --


### RegisterAction(*InputAction*, ref *string ID*)
*RegisterAction* takes an **InputAction** from your control scheme and a reference to the string ID you created.
This is for *button only* inputs, such as key presses, button presses, etc. 

You can now use *ListenInputEvent* to assign a callback to this action for either **Started**, **Performed**, or **Cancelled**.

*Callbacks for all events are Actions that use no arguments.*

Alternatively, you can avoid using events by either using the *Triggered*, *IsPressed*, or *IsReleased*.
> --


### RegisterRebindable(*GUID*, *string path*, *int bindingIndex*)
*RegisterBindable* registers your input as rebindable, and its binding for the given index can now be changed at any time.

Unity lets you access any **InputAction** *GUID* through the *.id* property, as well as its path through the *.effectivePath* property for a particular binding.

The binding index is usually 0, but may vary depending on what type of input you created and whether or not you want multiple buttons for that input. (Primary & Secondary)

Examples of this are all in the demo, and it's a really straightforward system.
> --


### Triggered(*string ID*)
Takes a registered ID and returns true if the input has been triggered.
> --


### IsPressed(*string ID*)
Takes a registered ID and returns true if the input is being held down.

This will only work with inputs registered with *RegisterAction*.
> --


### IsReleased(*string ID*)
Takes a registered ID and returns true if the input was just released.

This will only work with inputs registered with *RegisterAction*.
> --


### ReadFloat(*string ID*)
Takes a registered ID and returns the *float* value of that input.

Unity will throw an error if this input isn't valid to be a float. So be mindful of what types your inputs are.

This will only work with inputs registered with *RegisterValue*.
> --


### ReadVector2(*string ID*)
Takes a registered ID and returns the *Vector2* value of that input.

Unity will throw an error if this input isn't valid to be a Vector2. So be mindful of what types your inputs are.

This will only work with inputs registered with *RegisterValue*.
> --


### ListenInputEvent(*InputEventType*, *string ID*, *callback*)
Takes an **InputEventType** (**Started**, **Performed**, or **Cancelled**), the registered ID, and a no argument callback.
Whenever that input event happens, your callback will be notified.
> --


### MuteInputEvent(*InputEventType*, *string ID*, *callback*)
Takes an **InputEventType** (**Started**, **Performed**, or **Cancelled**), the registered ID, and a no argument callback.
Your callback will no longer be notified whenever that input event happens.
> --


### GetBindingName(*string ID*, *int bindingIndex = 0*)
Takes a registered ID, and an optional argument binding index.

Returns the display string for that particular binding.
> --


### GetAction(*string ID*, *int bindingIndex = 0*)
Takes a registered ID, and an optional argument for binding index then returns the associated Unity **InputAction**.
> --


### SaveAllRegisteredBindings()
Saves any bindings registered through *RegisterRebindable* to a file.

This will not overwrite your control scheme, so you'll need to load changed bindings.

You can modify the file path, name, and extension in **LInputRegistry.cs**
> --


### LoadAllRegisteredBindings()
Loads any bindings saved from a file and applies them.
> --

### LoadAllDefaultBindings()
Sets all registered bindings to their default value that they were first registered with.
