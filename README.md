# User32Api
Provides some api to use some User32 function in C#

# Description
Just some code I came up with when I wanted to be able to move the mouse, simulate mouse clicks, detect mouse clicks and simulate keystrokes in C#. I'm not really good at documentation but I'll See what I can do. The code may not be complete and contributions are welcome, though perhaps people don't need this kind of thing.

# Mouse - Events
The Mouse is a static class in the User32Api.Mouse namespace. Everything is handled through static fields and static functions. You can subsribe to Mouse events like you would subscribe and unsubscribe to any other event as you would normally do. These event's do not stop the input from going to the next call, it merely notifys you that they happened. In order to enable Global Mouse Events you need to call Mouse.HookGlobalEvents() and to unhook them call Mouse.UnhookGlobalEvents(). These will handle hooking and unhooking in the background and will prevent hooking and unhooking if already hooked or not yet hooked. The result of the hooking and unhooking are posted to the Console and Debug.

Currently events for the Left/Right/Middle/X Button down and up are implemented and events for all those buttons (Left/Right/Middle/X1/X2) for when they are clicked (pressed and released) are supported.

Mouse.Move is called when the LastPosition and Position are different with each call updating LastPosition (LastPosition is otherwise not updated).

Mouse.MouseScroll needs to be implemented.

# Mouse - Inputs
The Mouse class has a lot of helper functions. You can move the Mouse by calling MoveBy(int, int), set the position using MoveTo(int, int). Move the mouse to a normalized position using NormalizedMoveTo(int, int) or even move the mouse based on a scaled position using ScaledMove(double, double) (i.e. 0.5 and 0.5 will move the mouse to the center of the screen).

There are functions for pressing and releasing the mouse button where you specify which buttons is affected using the MouseButton enum (the default button is the Left Button) or call an asyn Click method which allows you to specify a delay before it clicks and how long to hold down the button.

# Keyboard
I was thinking about adding global events for the keyboard like the class but haven't gotten around to it yet. Perhaps I will.

What is suppoted is simulating the pressing of keys. There are some properties to see if a button is down (shift, ctrl, menu) or if a button is toggled (capslock, scroll lock, and numlock). You can check a specific side (left or right) or if either is pressed (i.e. Keyboard.ShiftDown opposed to Keyboard.LeftShiftDown). Setting this boolean properties will press, release or toggle them depending on the value set. You can set the property that checks for either to press the key and that will press the Left by default if one of the keys isn't down already.

The class supports using virtual keys or keys from scan codes. The default behavior can be changed by setting Keyboard.Default_VirtualKeys and that will be used when not specified.

There is also support for typing a set of keys, such as a string using Keyboard.TypeKeys. You can specify the delay betweeen keys and how long to hold them for (default: 0). I even went through the trouble of trying to make sure the case matched if if you hold shift or have caplocks on by mistake.

# End
That should be all. If I need to change this let me know or do it yourself.
