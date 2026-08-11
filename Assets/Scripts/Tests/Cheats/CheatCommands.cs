using System.Threading.Tasks;
using Dungeon.Config.Profiles;
using Dungeon.Core.Events;
using Dungeon.Core.Events.Inventory;
using Dungeon.Gameplay.Items;
using Dungeon.Visual.UI.Framework;
using Unity.Pipeline.Commands;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Dungeon.Tests.Cheats
{
    public enum WindowType
    {
        Inventory,
        Equipment
    }

    public enum MouseButtonInput
    {
        Left,
        Right,
        Middle,
        Forward,
        Back
    }

    public static class CheatCommands
    {
        [CliCommand("populate-inventory", "Clears and populates the inventry with safe profile data")]
        public static async Task<string> PopulateInventory(
            [CliArg("save-profile", "The address of the save profile to load", Required = true, DefaultValue = "SaveProfile")] string saveProfile,
            [CliArg("clear", "Whether the inventory should be cleared", Required = false, DefaultValue = true)] bool clearInventory)
        {

            if (clearInventory)
            {
                GameplayEventBus.Raise(new InventoryClearEvent());
            }

            var handle = Addressables.LoadAssetAsync<SaveProfile>(saveProfile);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var item in handle.Result.Items)
                {
                    var runtimeItem = ItemFactory.CreateItem(item.ItemData);
                    GameplayEventBus.Raise(new InventoryChangedEvent(InventoryChangedEvent.ChangeEvents.Added, runtimeItem,Vector3.zero, item.Amount));
                }
                return $"Response: Save profile {saveProfile} loaded.";
            }

            return $"Response: Unable to find {saveProfile}. Validate the addressables address is correct.";

            
        }

        [CliCommand("cheat-input-keyboard", "Trigger the new input system keyboard press ")]
        public static async Task<string> InputSystemKeyboard(
            [CliArg("key", "Keyboard button to press. Should be a valid InputSystem.Key", DefaultValue = Key.I, Required =true)] Key key,
            [CliArg("duration", "How long the key should be pressed in milliseconds", DefaultValue = 50, Required = false)] int duration)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return "Response: No keyboard found.";
            }

            var control = keyboard[key.ToString()] as KeyControl;
            if (control == null)
            {
                return $"Response: Could not find control for key {key}.";
            }

            // Press
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            await Task.Delay(duration);

            // Release
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());

            return $"Response: Triggered {key} press for {duration}/ms.";
        }



        [CliCommand("cheat-input-mouse-button", "Trigger a mouse button press")]
        public static async Task<string> InputSystemMouseButton(
            [CliArg("button", "Mouse button to press", DefaultValue = MouseButtonInput.Left, Required = true)] MouseButtonInput button,
            [CliArg("duration", "How long the button should be held in milliseconds", DefaultValue = 50, Required = false)] int duration)
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return "Response: No mouse found.";
            }

            ButtonControl control = button switch
            {
                MouseButtonInput.Left => mouse.leftButton,
                MouseButtonInput.Right => mouse.rightButton,
                MouseButtonInput.Middle => mouse.middleButton,
                MouseButtonInput.Forward => mouse.forwardButton,
                MouseButtonInput.Back => mouse.backButton,
                _ => null
            };

            if (control == null)
            {
                return $"Response: Could not find control for {button}.";
            }

            // Press
            using (StateEvent.From(mouse, out var eventPtr))
            {
                control.WriteValueIntoEvent(1f, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }

            await Task.Delay(duration);

            // Release
            using (StateEvent.From(mouse, out var eventPtr))
            {
                control.WriteValueIntoEvent(0f, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }

            return $"Response: Triggered {button} mouse button for {duration}ms.";
        }

        [CliCommand("cheat-input-mouse-look", "Hold a mouse button and drag to simulate camera look")]
    public static async Task<string> InputSystemMouseLook(
        [CliArg("button", "Mouse button to hold", DefaultValue = MouseButtonInput.Right, Required = true)] MouseButtonInput button,
        [CliArg("deltaX", "Total horizontal movement in pixels over the duration", DefaultValue = 500f, Required = false)] float deltaX,
        [CliArg("deltaY", "Total vertical movement in pixels over the duration", DefaultValue = 0f, Required = false)] float deltaY,
        [CliArg("duration", "Total drag duration in milliseconds", DefaultValue = 3000, Required = false)] int duration)
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return "Response: No mouse found.";
        }

        ButtonControl control = button switch
        {
            MouseButtonInput.Left => mouse.leftButton,
            MouseButtonInput.Right => mouse.rightButton,
            MouseButtonInput.Middle => mouse.middleButton,
            MouseButtonInput.Forward => mouse.forwardButton,
            MouseButtonInput.Back => mouse.backButton,
            _ => null
        };

        if (control == null)
        {
            return $"Response: Could not find control for {button}.";
        }

        // Press and hold
        using (StateEvent.From(mouse, out var eventPtr))
        {
            control.WriteValueIntoEvent(1f, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }

        // Drag: split total delta into steps over the duration (~60 steps/sec)
        const int stepIntervalMs = 16;
        int steps = Mathf.Max(1, duration / stepIntervalMs);
        float stepX = deltaX / steps;
        float stepY = deltaY / steps;

        for (int i = 0; i < steps; i++)
        {
            using (StateEvent.From(mouse, out var eventPtr))
            {
                mouse.delta.WriteValueIntoEvent(new Vector2(stepX, stepY), eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }
            await Task.Delay(stepIntervalMs);
        }

        // Release
        using (StateEvent.From(mouse, out var eventPtr))
        {
            control.WriteValueIntoEvent(0f, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }

        return $"Response: Held {button} and dragged ({deltaX}, {deltaY}) over {duration}ms.";
    }

        [CliCommand("toggle-window", "Toggles a specific UI window")]
        public static async Task<string> ToggleWindow([CliArg("window", "The window to toggle", Required = true)] WindowType window)
        {
            var windowManager = Object.FindAnyObjectByType<WindowManager>();
            if (windowManager == null)
            {
                return "Response: WindowManager not found in scene.";
            }

            string windowId = window.ToString().ToLower();
            windowManager.ToggleWindow(windowId);
            return $"Response: Toggled window '{windowId}'.";
        }
    }
}
