using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// LowLevelMouseProc https://docs.microsoft.com/en-us/windows/win32/winmsg/lowlevelmouseproc

namespace User32Api {

    [StructLayout(LayoutKind.Sequential)]
    public struct Input {
        public const uint INPUT_MOUSE = 0;
        public const uint INPUT_KEYBOARD = 1;
        public const uint INPUT_HARDWARE = 2;

        public static readonly int Size = Marshal.SizeOf(typeof(Input));
        public uint type;
        public InputUnion inputUnion;

        public Input(Mouse.Mouse.MouseInput mouseInput) {
            type = INPUT_MOUSE;
            inputUnion = new InputUnion {
                mi = mouseInput
            };
        }

        public Input(Keyboard.Keyboard.KeyboardInput keyboardInput) {
            type = INPUT_KEYBOARD;
            inputUnion = new InputUnion {
                ki = keyboardInput
            };
        }

        #region External
        // https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes
        [DllImport("Kernel32.Dll")]
        public static extern uint GetLastError();

        [DllImport("Kernel32.Dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("User32.Dll")]
        public static extern uint SendInput(uint count, Input[] inputs, int sizeOf);

        [DllImport("User32.Dll")]
        public static extern int GetSystemMetrics(int index);

        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
        [DllImport("User32.Dll")]
        public static extern IntPtr SetWindowsHookExA(int idHook, Delegate lpfn, IntPtr hmod, uint threadId);

        [DllImport("User32.Dll")]
        public static extern IntPtr UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("User32.Dll")]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Helper Functions
        public static short GetHighOrder(int i) {
            return (short)(i >> 16);
        }

        public static short GetLowOrder(int i) {
            return (short)(i & 0x0000FFFF);
        }

        public static byte GetHighOrder(short i) {
            return (byte)(i >> 8);
        }

        public static byte GetLowOrder(short i) {
            return (byte)(i & 0x00FF);
        }

        public static uint SendInput(params Input[] inputs) {
            return SendInput((uint)inputs.Length, inputs, Size);
        }
        #endregion
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion {
        [FieldOffset(0)] public Mouse.Mouse.MouseInput mi;
        [FieldOffset(0)] public Keyboard.Keyboard.KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput {
        public uint msg;
        public ushort paramL;
        public ushort paramH;
    }

    namespace Keyboard {
        public static class Keyboard {

            #region Constants

            #region Flags
            // https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput
            public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
            public const uint KEYEVENTF_KEYUP = 0x0002; // If not specified, key is being pressed
            public const uint KEYEVENTF_SCANCODE = 0x0008;
            public const uint KEYEVENTF_UNICODE = 0x0004;
            #endregion

            #region Shift State
            public const ushort SHIFT_PRESSED = 1;
            public const ushort CTRL_PRESSED = 2;
            public const ushort ALT_PRESSED = 4;
            public const ushort HANKAKU_PRESSED = 8;
            public const ushort RESERVED_1 = 16;
            public const ushort RESERVED_2 = 32;
            #endregion

            #region Map Types
            public const uint MAPVK_VK_TO_CHAR = 2;
            public const uint MAPVK_VK_TO_VSC = 0;
            public const uint MAPVK_VSC_TO_VK = 1;
            public const uint MAPVK_VSC_TO_VK_EX = 4;
            #endregion

            #endregion

            #region Properties
            public static bool Default_VirtualKeys { get; set; }
            public static bool ShiftDown {
                get => LeftShiftDown || RightShiftDown;
                set {
                    if(ShiftDown == value) return;
                    if(value) LeftShiftDown = value;
                    else { LeftShiftDown = value; RightShiftDown = value; }
                }
            }
            public static bool LeftShiftDown {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_LSHIFT);
                    return (status & 0x8000) == 0x8000;
                }
                set {
                    if(value == LeftShiftDown)
                        return;
                    SetState(VirtualKeys.VK_LSHIFT, value, true);
                }
            }
            public static bool RightShiftDown {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_RSHIFT);
                    return (status & 0x8000) == 0x8000;
                }
                set {
                    if(value == RightShiftDown)
                        return;
                    SetState(VirtualKeys.VK_RSHIFT, value, false);
                }
            }
            public static bool MenuDown {
                get => LeftMenuDown || RightMenuDown;
                set {
                    if(MenuDown == value) return;
                    if(value) LeftMenuDown = value;
                    else { LeftMenuDown = value; RightMenuDown = value; }
                }
            }
            public static bool LeftMenuDown {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_LMENU);
                    return (status & 0x8000) == 0x8000;
                }
                set {
                    if(value == LeftMenuDown)
                        return;
                    SetState(VirtualKeys.VK_LMENU, value, false);
                }
            }
            public static bool RightMenuDown {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_RMENU);
                    return (status & 0x8000) == 0x8000;
                }
                set {
                    if(value == RightMenuDown)
                        return;
                    SetState(VirtualKeys.VK_RMENU, value, false);
                }
            }
            public static bool ControlDown {
                get => LeftControlDown || RightControlDown;
                set {
                    if(ControlDown == value) return;
                    if(value) LeftControlDown = value;
                    else { LeftControlDown = value; RightControlDown = value; }
                }
            }
            public static bool LeftControlDown {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_LCONTROL);
                    return (status & 0x8000) == 0x8000;
                }
                set {
                    if(value == LeftControlDown)
                        return;
                    SetState(VirtualKeys.VK_LCONTROL, value, false);
                }
            }
            public static bool RightControlDown {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_RCONTROL);
                    return (status & 0x8000) == 0x8000;
                }
                set {
                    if(value == RightControlDown)
                        return;
                    SetState(VirtualKeys.VK_RCONTROL, value, false);
                }
            }
            public static bool CapsLockToggled {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_CAPITAL);
                    return (status & 0x0001) == 0x001;
                }
                set {
                    if(value == CapsLockToggled)
                        return;
                    SetState(VirtualKeys.VK_CAPITAL, value, true);
                }
            }
            public static bool NumLockToggled {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_NUMLOCK);
                    return (status & 0x0001) == 0x001;
                }
                set {
                    if(value == NumLockToggled)
                        return;
                    SetState(VirtualKeys.VK_NUMLOCK, value, false);
                }
            }
            public static bool ScrollLockToggled {
                get {
                    ushort status = GetKeyState(VirtualKeys.VK_SCROLL);
                    return (status & 0x0001) == 0x001;
                }
                set {
                    if(value == ScrollLockToggled)
                        return;
                    SetState(VirtualKeys.VK_SCROLL, value, false);
                }
            }

            private static readonly byte[] keyboardState = new byte[256];
            public static byte[] KeyboardState {
                get {
                    GetKeyboardState(keyboardState);
                    return keyboardState;
                }
            }
            #endregion

            [StructLayout(LayoutKind.Sequential)]
            public struct KeyboardInput {
                public ushort vk;
                public ushort scan;
                public uint flags;
                public uint time;
                public IntPtr extraInfo;

                public KeyboardInput(ushort key, bool keyDown) {
                    vk = Default_VirtualKeys ? key : (ushort)0;
                    scan = Default_VirtualKeys ? (ushort)0 : (ushort)MapVirtualKeyA(key, MAPVK_VK_TO_VSC);
                    flags = (keyDown ? 0 : KEYEVENTF_KEYUP) | (Default_VirtualKeys ? 0 : KEYEVENTF_SCANCODE);
                    time = 0;
                    extraInfo = IntPtr.Zero;
                }

                public KeyboardInput(ushort key, bool keyDown, bool isVirtualKey) {
                    vk = isVirtualKey ? key : (ushort)0;
                    scan = isVirtualKey ? (ushort)0 : (ushort)MapVirtualKeyA(key, MAPVK_VK_TO_VSC);
                    flags = (keyDown ? 0 : KEYEVENTF_KEYUP) | (isVirtualKey ? 0 : KEYEVENTF_SCANCODE);
                    time = 0;
                    extraInfo = IntPtr.Zero;
                }

                public KeyboardInput(char key, bool keyDown, bool isVirtualKey) {
                    vk = isVirtualKey ? GetVKCode(key) : (ushort)0;
                    scan = isVirtualKey ? (ushort)0 : (ushort)MapVirtualKeyA(GetVKCode(key), MAPVK_VK_TO_VSC);
                    flags = (keyDown ? 0 : KEYEVENTF_KEYUP) | (isVirtualKey ? 0 : KEYEVENTF_SCANCODE);
                    time = 0;
                    extraInfo = IntPtr.Zero;
                }

                public KeyboardInput(char key, bool keyDown, IntPtr locale) {
                    vk = VkKeyScanExA((byte)key, locale);
                    scan = 0;
                    flags = keyDown ? 0 : KEYEVENTF_KEYUP;
                    time = 0;
                    extraInfo = IntPtr.Zero;
                }
            }

            #region Externals
            [DllImport("User32.Dll")]
            public static extern ushort GetKeyState(int virtKey);

            [DllImport("User32.Dll")]
            public static extern bool GetKeyboardState(byte[] array);

            [DllImport("User32.Dll")]
            public static extern bool SetKeyboardState(byte[] array);

            [DllImport("User32.Dll")]
            public static extern ushort VkKeyScanA(byte ch);

            [DllImport("User32.Dll")]
            public static extern ushort VkKeyScanExA(byte ch, IntPtr hkl);

            [DllImport("User32.Dll")]
            public static extern uint MapVirtualKeyA(uint code, uint mapType);

            // TODO Figure out how to make setting the type of keyboard easier.
            // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadkeyboardlayouta
            // https://docs.microsoft.com/en-us/windows/win32/intl/language-identifiers
            // https://docs.microsoft.com/en-us/windows/win32/intl/language-identifier-constants-and-strings
            [DllImport("User32.Dll")]
            public static extern IntPtr LoadKeyboardLayoutA(IntPtr pwszKLID, uint flags);
            #endregion

            #region Helper Functions
            public static bool IsKeyDown(ushort key) {
                ushort status = GetKeyState(key);
                return (status & 0x8000) == 0x8000;
            }
            public static bool IsKeyDown(char key) {
                ushort status = GetKeyState(GetVKCode(key));
                return (status & 0x8000) == 0x8000;
            }

            public static ushort GetVKCode(char key) {
                return (ushort)Input.GetLowOrder(VkKeyScanA((byte)key));
            }

            public static bool SetState(ushort key, bool keyDown) {
                return SetState(key, keyDown, Default_VirtualKeys);
            }

            public static bool SetState(ushort key, bool keyDown, bool isVirtualKey) {
                return Input.SendInput(new Input(new KeyboardInput(key, keyDown, isVirtualKey))) > 0;
            }

            public static bool SetState(char key, bool keyDown) {
                return SetState(key, keyDown, Default_VirtualKeys);
            }

            public static bool SetState(char key, bool keyDown, bool isVirtualKey) {
                return Input.SendInput(new Input(new KeyboardInput(key, keyDown, isVirtualKey))) > 0;
            }

            public static bool SetState(char key, IntPtr locale, bool keyDown) {
                return Input.SendInput(new Input(new KeyboardInput(key, keyDown, locale))) > 0;
            }

            public static bool PressKey(ushort key) {
                return PressKey(key, Default_VirtualKeys);
            }

            public static bool PressKey(ushort key, bool isVirtualKey) {
                return SetState(key, true, isVirtualKey);
            }

            public static bool PressKey(char key) {
                return PressKey(key, Default_VirtualKeys);
            }

            public static bool PressKey(char key, bool isVirtualKey) {
                return SetState(key, true, isVirtualKey);
            }

            public static bool PressKey(char key, IntPtr locale) {
                return SetState(key, locale, true);
            }

            public static bool ReleaseKey(ushort key) {
                return ReleaseKey(key, Default_VirtualKeys);
            }

            public static bool ReleaseKey(ushort key, bool isVirtualKey) {
                return SetState(key, false, isVirtualKey);
            }

            public static bool ReleaseKey(char key) {
                return ReleaseKey(key, Default_VirtualKeys);
            }

            public static bool ReleaseKey(char key, bool isVirtualKey) {
                return SetState(key, false, isVirtualKey);
            }

            public static bool ReleaseKey(char key, IntPtr locale) {
                return SetState(key, locale, false);
            }

            public async static Task<bool> TypeKeys(IEnumerable<ushort> keys, bool useVirtualKeys, int delay = 0, int holdFor = 0) {
                foreach(var key in keys) {
                    bool result = await TypeKey(key, useVirtualKeys, delay, holdFor);
                    if(!result)
                        return result;
                }

                return true;
            }

            public async static Task<bool> TypeKey(ushort key, bool useVirtualKeys, int delay = 0, int holdFor = 0) {
                bool result = Input.SendInput(new Input(new KeyboardInput(key, true, useVirtualKeys))) > 0;
                if(!result)
                    return result;
                await Task.Delay(holdFor);
                result = Input.SendInput(new Input(new KeyboardInput(key, false, useVirtualKeys))) > 0;
                if(!result)
                    return result;
                await Task.Delay(delay);
                return result;
            }

            public async static Task<bool> TypeKeys(IEnumerable<char> keys, int delayBetweenKeys = 0, int holdFor = 0) {
                return await TypeKeys(keys, Default_VirtualKeys, delayBetweenKeys, holdFor);
            }

            public async static Task<bool> TypeKeys(IEnumerable<char> keys, bool useVirtualKeys, int delayBetweenKeys = 0, int holdFor = 0) {
                bool result = true;
                bool pressedShift = false;

                foreach(var key in keys) {
                    ushort vkCode = (ushort)Input.GetLowOrder(VkKeyScanA((byte)key));

                    if(PrepareKey(key, vkCode))
                        pressedShift = !pressedShift;

                    if(!(result = PressKey(vkCode, useVirtualKeys)))
                        break;
                    await Task.Delay(holdFor);
                    if(!(result = ReleaseKey(vkCode, useVirtualKeys)))
                        break;
                    await Task.Delay(delayBetweenKeys);
                }

                if(pressedShift)
                    ShiftDown = false;

                return result;
            }

            public async static Task<bool> TypeKey(char key, bool useVirtualKeys, int delay = 0, int holdFor = 0) {
                bool result = Input.SendInput(new Input(new KeyboardInput(key, true, useVirtualKeys))) > 0;
                if(!result)
                    return result;
                await Task.Delay(holdFor);
                result = Input.SendInput(new Input(new KeyboardInput(key, false, useVirtualKeys))) > 0;
                if(!result)
                    return result;
                await Task.Delay(delay);
                return result;
            }

            public static bool PrepareKey(char key, ushort vkCode) {
                if(char.IsLetter(key)) {
                    if(char.IsUpper(key) ? (CapsLockToggled ? ShiftDown : !ShiftDown) : (CapsLockToggled ? !ShiftDown : ShiftDown)) {
                        ShiftDown = !ShiftDown;
                        return true;
                    }
                } else {
                    char vKey = (char)MapVirtualKeyA(vkCode, MAPVK_VK_TO_CHAR);
                    if(key == vKey ? ShiftDown : !ShiftDown) {
                        ShiftDown = !ShiftDown;
                        return true;
                    }
                }
                return false;
            }

            #endregion

        }

        public static class VirtualKeys {
            // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            // https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput
            // https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
            // https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keyup
            public const ushort VK_LBUTTON = 0x01;
            public const ushort VK_RBUTTON = 0x02;
            public const ushort VK_CANCEL = 0x03;
            public const ushort VK_MBUTTON = 0x04;
            public const ushort VK_XBUTTON1 = 0x05;
            public const ushort VK_XBUTTON2 = 0x06;
            public const ushort UNDEFINED_1 = 0x07;
            public const ushort VK_BACK = 0x08;
            public const ushort VK_TAB = 0x09;
            public const ushort RESERVED_1 = 0x0A;
            public const ushort RESERVED_2 = 0x0B;
            public const ushort VK_CLEAR = 0x0C;
            public const ushort VK_RETURN = 0x0D;
            public const ushort UNDEFINED_2 = 0x0E;
            public const ushort UNDEFINED_3 = 0x0F;
            public const ushort VK_SHIFT = 0x10;
            public const ushort VK_CONTROL = 0x11;
            public const ushort VK_MENU = 0x12;
            public const ushort VK_PAUSE = 0x13;
            public const ushort VK_CAPITAL = 0x14;
            public const ushort VK_KANA = 0x15;
            public const ushort VK_HANGUEL = 0x15;
            public const ushort VK_HANGUL = 0x15;
            public const ushort UNDEFINED_4 = 0x16;
            public const ushort VK_JUNJA = 0x17;
            public const ushort VK_FINAL = 0x18;
            public const ushort VK_HANJA = 0x19;
            public const ushort VK_KANJI = 0x19;
            public const ushort UNDEFINED_5 = 0x1A;
            public const ushort VK_ESCAPE = 0x1B;
            public const ushort VK_CONVERT = 0x1C;
            public const ushort VK_NONCONVERT = 0x1D;
            public const ushort VK_ACCEPT = 0x1E;
            public const ushort VK_MODECHANGE = 0x1F;
            public const ushort VK_SPACE = 0x20;
            public const ushort VK_PRIOR = 0x21;
            public const ushort VK_NEXT = 0x22;
            public const ushort VK_END = 0x23;
            public const ushort VK_HOME = 0x24;
            public const ushort VK_LEFT = 0x25;
            public const ushort VK_UP = 0x26;
            public const ushort VK_RIGHT = 0x27;
            public const ushort VK_DOWN = 0x28;
            public const ushort VK_SELECT = 0x29;
            public const ushort VK_PRINT = 0x2A;
            public const ushort VK_EXECUTE = 0x2B;
            public const ushort VK_SNAPSHOT = 0x2C;
            public const ushort VK_INSERT = 0x2D;
            public const ushort VK_DELETE = 0x2E;
            public const ushort VK_HELP = 0x2F;
            public const ushort VK_0 = 0x30;
            public const ushort VK_1 = 0x31;
            public const ushort VK_2 = 0x32;
            public const ushort VK_3 = 0x33;
            public const ushort VK_4 = 0x34;
            public const ushort VK_5 = 0x35;
            public const ushort VK_6 = 0x36;
            public const ushort VK_7 = 0x37;
            public const ushort VK_8 = 0x38;
            public const ushort VK_9 = 0x39;
            public const ushort UNDEFINED_6 = 0x3A;
            public const ushort UNDEFINED_7 = 0x3B;
            public const ushort UNDEFINED_8 = 0x3C;
            public const ushort UNDEFINED_9 = 0x3;
            public const ushort UNDEFINED_10 = 0x3E;
            public const ushort UNDEFINED_11 = 0x3F;
            public const ushort UNDEFINED_12 = 0x40;
            public const ushort VK_A = 0x41;
            public const ushort VK_B = 0x42;
            public const ushort VK_C = 0x43;
            public const ushort VK_D = 0x44;
            public const ushort VK_E = 0x45;
            public const ushort VK_F = 0x46;
            public const ushort VK_G = 0x47;
            public const ushort VK_H = 0x48;
            public const ushort VK_I = 0x49;
            public const ushort VK_J = 0x4A;
            public const ushort VK_K = 0x4B;
            public const ushort VK_L = 0x4C;
            public const ushort VK_M = 0x4D;
            public const ushort VK_N = 0x4E;
            public const ushort VK_O = 0x4F;
            public const ushort VK_P = 0x50;
            public const ushort VK_Q = 0x51;
            public const ushort VK_R = 0x52;
            public const ushort VK_S = 0x53;
            public const ushort VK_T = 0x54;
            public const ushort VK_U = 0x55;
            public const ushort VK_V = 0x56;
            public const ushort VK_W = 0x57;
            public const ushort VK_X = 0x58;
            public const ushort VK_Y = 0x59;
            public const ushort VK_Z = 0x5A;
            public const ushort VK_LWIN = 0x5B;
            public const ushort VK_RWIN = 0x5C;
            public const ushort VK_APPS = 0x5D;
            public const ushort RESERVED_3 = 0x5E;
            public const ushort VK_SLEEP = 0x5F;
            public const ushort VK_NUMPAD0 = 0x60;
            public const ushort VK_NUMPAD1 = 0x61;
            public const ushort VK_NUMPAD2 = 0x62;
            public const ushort VK_NUMPAD3 = 0x63;
            public const ushort VK_NUMPAD4 = 0x64;
            public const ushort VK_NUMPAD5 = 0x65;
            public const ushort VK_NUMPAD6 = 0x66;
            public const ushort VK_NUMPAD7 = 0x67;
            public const ushort VK_NUMPAD8 = 0x68;
            public const ushort VK_NUMPAD9 = 0x69;
            public const ushort VK_MULTIPLY = 0x6A;
            public const ushort VK_ADD = 0x6B;
            public const ushort VK_SEPARATOR = 0x6C;
            public const ushort VK_SUBTRACT = 0x6D;
            public const ushort VK_DECIMAL = 0x6E;
            public const ushort VK_DIVIDE = 0x6F;
            public const ushort VK_F1 = 0x70;
            public const ushort VK_F2 = 0x71;
            public const ushort VK_F3 = 0x72;
            public const ushort VK_F4 = 0x73;
            public const ushort VK_F5 = 0x74;
            public const ushort VK_F6 = 0x75;
            public const ushort VK_F7 = 0x76;
            public const ushort VK_F8 = 0x77;
            public const ushort VK_F9 = 0x78;
            public const ushort VK_F10 = 0x79;
            public const ushort VK_F11 = 0x7A;
            public const ushort VK_F12 = 0x7B;
            public const ushort VK_F13 = 0x7C;
            public const ushort VK_F14 = 0x7D;
            public const ushort VK_F15 = 0x7E;
            public const ushort VK_F16 = 0x7F;
            public const ushort VK_F17 = 0x80;
            public const ushort VK_F18 = 0x81;
            public const ushort VK_F19 = 0x82;
            public const ushort VK_F20 = 0x83;
            public const ushort VK_F21 = 0x84;
            public const ushort VK_F22 = 0x85;
            public const ushort VK_F23 = 0x86;
            public const ushort VK_F24 = 0x87;
            public const ushort UNASSIGNED_1 = 0x88;
            public const ushort UNASSIGNED_2 = 0x89;
            public const ushort UNASSIGNED_3 = 0x8A;
            public const ushort UNASSIGNED_4 = 0x8B;
            public const ushort UNASSIGNED_5 = 0x8C;
            public const ushort UNASSIGNED_6 = 0x8D;
            public const ushort UNASSIGNED_7 = 0x8E;
            public const ushort UNASSIGNED_8 = 0x8F;
            public const ushort VK_NUMLOCK = 0x90;
            public const ushort VK_SCROLL = 0x91;
            public const ushort OEM_SPECIFIC_1 = 0x92;
            public const ushort OEM_SPECIFIC_2 = 0x93;
            public const ushort OEM_SPECIFIC_3 = 0x94;
            public const ushort OEM_SPECIFIC_4 = 0x95;
            public const ushort OEM_SPECIFIC_5 = 0x96;
            public const ushort UNASSIGNED_9 = 0x97;
            public const ushort UNASSIGNED_10 = 0x98;
            public const ushort UNASSIGNED_11 = 0x99;
            public const ushort UNASSIGNED_12 = 0x9A;
            public const ushort UNASSIGNED_13 = 0x9B;
            public const ushort UNASSIGNED_14 = 0x9C;
            public const ushort UNASSIGNED_15 = 0x9D;
            public const ushort UNASSIGNED_16 = 0x9E;
            public const ushort UNASSIGNED_17 = 0x9F;
            public const ushort VK_LSHIFT = 0xA0;
            public const ushort VK_RSHIFT = 0xA1;
            public const ushort VK_LCONTROL = 0xA2;
            public const ushort VK_RCONTROL = 0xA3;
            public const ushort VK_LMENU = 0xA4;
            public const ushort VK_RMENU = 0xA5;
            public const ushort VK_BROWSER_BACK = 0xA6;
            public const ushort VK_BROWSER_FORWARD = 0xA7;
            public const ushort VK_BROWSER_REFRESH = 0xA8;
            public const ushort VK_BROWSER_STOP = 0xA9;
            public const ushort VK_BROWSER_SEARCH = 0xAA;
            public const ushort VK_BROWSER_FAVORITES = 0xAB;
            public const ushort VK_BROWSER_HOME = 0xAC;
            public const ushort VK_VOLUME_MUTE = 0xAD;
            public const ushort VK_VOLUME_DOWN = 0xAE;
            public const ushort VK_VOLUME_UP = 0xAF;
            public const ushort VK_MEDIA_NEXT_TRACK = 0xB0;
            public const ushort VK_MEDIA_PREV_TRACK = 0xB1;
            public const ushort VK_MEDIA_STOP = 0xB2;
            public const ushort VK_MEDIA_PLAY_PAUSE = 0xB3;
            public const ushort VK_LAUNCH_MAIL = 0xB4;
            public const ushort VK_LAUNCH_MEDIA_SELECT = 0xB5;
            public const ushort VK_LAUNCH_APP1 = 0xB6;
            public const ushort VK_LAUNCH_APP2 = 0xB7;
            public const ushort RESERVED_4 = 0xB8;
            public const ushort RESERVED_5 = 0xB9;
            public const ushort VK_SemiColon = 0xBA; // US Keyboard
            public const ushort VK_OEM_1 = 0xBA;
            public const ushort VK_OEM_PLUS = 0xBB;
            public const ushort VK_OEM_COMMA = 0xBC;
            public const ushort VK_OEM_MINUS = 0xBD;
            public const ushort VK_OEM_PERIOD = 0xBE;
            public const ushort VK_OEM_2 = 0xBF;
            public const ushort VK_FORWARD_SLASH = 0xBF; // US Keyboard
            public const ushort VK_OEM_3 = 0xC0;
            public const ushort VK_TILE = 0xC0; // US Keyboard
            public const ushort RESERVED_6 = 0xC1;
            public const ushort RESERVED_7 = 0xC2;
            public const ushort RESERVED_8 = 0xC3;
            public const ushort RESERVED_9 = 0xC4;
            public const ushort RESERVED_10 = 0xC5;
            public const ushort RESERVED_11 = 0xC6;
            public const ushort RESERVED_12 = 0xC7;
            public const ushort RESERVED_13 = 0xC8;
            public const ushort RESERVED_14 = 0xC9;
            public const ushort RESERVED_15 = 0xCA;
            public const ushort RESERVED_16 = 0xCB;
            public const ushort RESERVED_17 = 0xCC;
            public const ushort RESERVED_18 = 0xCD;
            public const ushort RESERVED_19 = 0xCE;
            public const ushort RESERVED_20 = 0xCF;
            public const ushort RESERVED_21 = 0xD0;
            public const ushort RESERVED_22 = 0xD1;
            public const ushort RESERVED_23 = 0xD2;
            public const ushort RESERVED_24 = 0xD3;
            public const ushort RESERVED_25 = 0xD4;
            public const ushort RESERVED_26 = 0xD5;
            public const ushort RESERVED_27 = 0xD6;
            public const ushort RESERVED_28 = 0xD7;
            public const ushort UNASSIGNED_18 = 0xD8;
            public const ushort UNASSIGNED_19 = 0xD9;
            public const ushort UNASSIGNED_20 = 0xDA;
            public const ushort VK_OEM_4 = 0xDB;
            public const ushort VK_OPEN_SQUARE_BRACKET = 0xDB; // US Keyboard
            public const ushort VK_OEM_5 = 0xDC;
            public const ushort VK_BACK_SLASH = 0xDC; // US Keyboard
            public const ushort VK_OEM_6 = 0xDD;
            public const ushort VK_CLOSE_SQUARE_BRACKET = 0xDC; // US Keyboard
            public const ushort VK_OEM_7 = 0xDE;
            public const ushort VK_SINGLE_QUOTE = 0xDC; // US Keyboard
            public const ushort VK_OEM_8 = 0xDF;
            public const ushort RESERVED_29 = 0xE0;
            public const ushort OEM_SPECIFIC_6 = 0xE1;
            public const ushort VK_OEM_102 = 0xE2;
            public const ushort OEM_SPECIFIC_7 = 0xE3;
            public const ushort OEM_SPECIFIC_8 = 0xE4;
            public const ushort VK_PROCESSKEY = 0xE5;
            public const ushort OEM_SPECIFIC_9 = 0xE6;
            public const ushort VK_PACKET = 0xE7;
            public const ushort UNASSIGNED_21 = 0xE8;
            public const ushort OEM_SPECIFIC_10 = 0xE9;
            public const ushort OEM_SPECIFIC_11 = 0xEA;
            public const ushort OEM_SPECIFIC_12 = 0xEB;
            public const ushort OEM_SPECIFIC_13 = 0xEC;
            public const ushort OEM_SPECIFIC_14 = 0xED;
            public const ushort OEM_SPECIFIC_15 = 0xEE;
            public const ushort OEM_SPECIFIC_16 = 0xEF;
            public const ushort OEM_SPECIFIC_17 = 0xF0;
            public const ushort OEM_SPECIFIC_18 = 0xF1;
            public const ushort OEM_SPECIFIC_19 = 0xF2;
            public const ushort OEM_SPECIFIC_20 = 0xF3;
            public const ushort OEM_SPECIFIC_21 = 0xF4;
            public const ushort OEM_SPECIFIC_22 = 0xF5;
            public const ushort VK_ATTN = 0xF6;
            public const ushort VK_CRSEL = 0xF7;
            public const ushort VK_EXSEL = 0xF8;
            public const ushort VK_EREOF = 0xF9;
            public const ushort VK_PLAY = 0xFA;
            public const ushort VK_ZOOM = 0xFB;
            public const ushort VK_NONAME = 0xFC;
            public const ushort VK_PA1 = 0xFD;
            public const ushort VK_OEM_CLEAR = 0xFE;
        }
    }

    namespace Mouse {
        public static class Mouse {

            #region Constants
            public const uint WHEEL_DELTA = 120;

            public const uint XBUTTON1 = 0x0001; // Mouse Data if MOUSEEVENTF_XDOWN or MOUSEEVENTF_XUP
            public const uint XBUTTON2 = 0x0002; // Mouse Data if MOUSEEVENTF_XDOWN or MOUSEEVENTF_XUP

            public const int OneScreen = 65_535;

            #region Mouse Input Flags
            // Flags https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
            public const uint MOUSEEVENTF_ABSOLUTE = 0x8000; // If set, then dx and dy are normalized absolute coordinates, otherwise, movement is relative 
            public const uint MOUSEEVENTF_HWHEEL = 0x01000;  // The wheel was moved horizontally. The amount of movement is specified in mouseData.
            public const uint MOUSEEVENTF_MOVE = 0x0001;     // Movement occurred.
            public const uint MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000;

            public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
            public const uint MOUSEEVENTF_LEFTUP = 0x0004;
            public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
            public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
            public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
            public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

            public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000; // Maps coordinates to the entire desktop. Must be used with MOUSEEVENTF_ABSOLUTE if used.
            public const uint MOUSEEVENTF_WHEEL = 0x0800;   // The wheel was moved. The amount of movement is specified in mouseData.

            public const uint MOUSEEVENTF_XDOWN = 0x0080;   // An X button was pressed.
            public const uint MOUSEEVENTF_XUP = 0x0100;     // An X button was released.
            #endregion

            #region Mouse Input Notifications
            // Mouse Input Notifications https://docs.microsoft.com/en-us/windows/win32/inputdev/mouse-input-notifications
            public const int WM_CAPTURECHANGED = 0x0215; // Sent to the window that is losing the mouse capture
            public const int WM_LBUTTONDBLCLK = 0x0203;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_MBUTTONDBLCLK = 0x0209;
            public const int WM_MBUTTONDOWN = 0x0207;
            public const int WM_MBUTTONUP = 0x0208;
            public const int WM_MOUSEACTIVATE = 0x0021; // Sent when the cursor is in an inactive window and the user presses a mouse button
            public const int WM_MOUSEHOVER = 0x0021; // When the cursor hovers over the client area for the time specified in a call to TrackMouseEvent.
            public const int WM_MOUSEHWHEEL = 0x020E;
            public const int WM_MOUSELEAVE = 0x02A3; // When the cursor leaves the client area for the time specified in a call to TrackMouseEvent.
            public const int WM_MOUSEMOVE = 0x0200;
            public const int WM_MOUSEWHEEL = 0x020A;
            public const int WM_NCHITTEST = 0x0084;
            public const int WM_NCLBUTTONDBLCLK = 0x00A3;
            public const int WM_NCLBUTTONDOWN = 0x00A1;
            public const int WM_NCLBUTTONUP = 0x00A2;
            public const int WM_NCMBUTTONDBLCLK = 0x00A9;
            public const int WM_NCMBUTTONDOWN = 0x00A7;
            public const int WM_NCMBUTTONUP = 0x00A8;
            public const int WM_NCMOUSEHOVER = 0x02A0;
            public const int WM_NCMOUSELEAVE = 0x02A2;
            public const int WM_NCMOUSEMOVE = 0x00A0;
            public const int WM_NCRBUTTONDBLCLK = 0x00A6;
            public const int WM_NCRBUTTONDOWN = 0x00A4;
            public const int WM_NCRBUTTONUP = 0x00A5;
            public const int WM_NCXBUTTONDBLCLK = 0x00AD;
            public const int WM_NCXBUTTONDOWN = 0x00AB;
            public const int WM_NCXBUTTONUP = 0x00AC;
            public const int WM_RBUTTONDBLCLK = 0x0206;
            public const int WM_RBUTTONDOWN = 0x0204;
            public const int WM_RBUTTONUP = 0x0205;
            public const int WM_XBUTTONDBLCLK = 0x020D;
            public const int WM_XBUTTONDOWN = 0x020B;
            public const int WM_XBUTTONUP = 0x020C;
            #endregion

            #endregion

            #region External
            [DllImport("User32.Dll")]
            private static extern bool GetCursorPos(ref Point point);
            [DllImport("User32.Dll")]
            private static extern bool GetPhysicalCursorPos(ref Point point);
            [DllImport("User32.Dll")]
            private static extern bool SetCursorPos(int x, int y);
            public static uint GetLastError() => Input.GetLastError();
            #endregion

            #region Delegates and Events
            public delegate int MouseHookEvent(int nCode, IntPtr wParam, IntPtr lParam);
            public delegate void MouseEvent();
            public delegate void MouseInputEvent(MouseInput mouse);

            private static event MouseHookEvent GlobalMouseEvent;

            public static event MouseEvent OnLeftDown;
            public static event MouseEvent OnLeftUp;
            public static event MouseEvent OnLeftClick;

            public static event MouseEvent OnRightDown;
            public static event MouseEvent OnRightUp;
            public static event MouseEvent OnRightClick;

            public static event MouseEvent OnMiddleDown;
            public static event MouseEvent OnMiddleUp;
            public static event MouseEvent OnMiddleClick;

            public static event MouseInputEvent OnXDown;
            public static event MouseInputEvent OnXUp;
            public static event MouseInputEvent MouseScroll;
            public static event MouseInputEvent Move;

            public static event MouseEvent OnX1Down;
            public static event MouseEvent OnX1Up;
            public static event MouseEvent OnX1Click;

            public static event MouseEvent OnX2Down;
            public static event MouseEvent OnX2Up;
            public static event MouseEvent OnX2Click;
            #endregion

            #region Structs and Enums
            [StructLayout(LayoutKind.Sequential)]
            public struct MouseInput {
                public int x;
                public int y;
                public uint mouseData;
                public uint flags;
                public uint time;
                public IntPtr extraInfo;

                public MouseInput(MouseButton button, bool down = true) {
                    x = y = 0;
                    mouseData = time = 0;
                    extraInfo = IntPtr.Zero;
                    switch(button) {
                        case MouseButton.Left:
                            flags = down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
                            break;
                        case MouseButton.Right:
                            flags = down ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
                            break;
                        case MouseButton.Middle:
                            flags = down ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
                            break;
                        case MouseButton.X1:
                            flags = down ? MOUSEEVENTF_XDOWN : MOUSEEVENTF_XUP;
                            mouseData = XBUTTON1;
                            break;
                        case MouseButton.X2:
                            flags = down ? MOUSEEVENTF_XDOWN : MOUSEEVENTF_XUP;
                            mouseData = XBUTTON2;
                            break;
                        default:
                            flags = 0;
                            break;
                    }
                }

                public MouseInput(int x, int y, bool absolute) {
                    this.x = x;
                    this.y = y;
                    mouseData = 0;
                    flags = MOUSEEVENTF_MOVE | (absolute ? MOUSEEVENTF_ABSOLUTE : 0);
                    time = 0;
                    extraInfo = IntPtr.Zero;
                }

                public static MouseInput FromPointer(IntPtr ptr) {
                    return Marshal.PtrToStructure<MouseInput>(ptr);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Point {
                public int X;
                public int Y;

                public Point(int x, int y = 0) {
                    X = x;
                    Y = y;
                }

                public static bool operator ==(Point point1, Point point2) {
                    return point1.X == point2.X && point1.Y == point2.Y;
                }

                public static bool operator !=(Point point1, Point point2) {
                    return !(point1 == point2);
                }

                public override bool Equals(object obj) {
                    return base.Equals(obj);
                }

                public override int GetHashCode() {
                    // Does it need a better hash?
                    return base.GetHashCode();
                }

                public bool Equals(Point point) {
                    return this == point;
                }

                public override string ToString() {
                    return $"{{{X}, {Y}}}";
                }

                
            }

            public enum MouseButton {
                Left, Right, Middle, X1, X2
            }

            #endregion

            #region Properties
            private static IntPtr GlobalMouseHook { get; set; } = IntPtr.Zero;
            public static bool LeftButtonDown { get; private set; }
            public static bool RightButtonDown { get; private set; }
            public static bool MiddleButtonDown { get; private set; }
            public static bool X1ButtonDown { get; private set; }
            public static bool X2ButtonDown { get; private set; }
            public static Point Position {
                get {
                    Point pos = new Point();
                    GetCursorPos(ref pos);
                    return pos;
                }
            }
            public static Point LastPosition { get; set; } // Only updates when global hooks are enabled
            public static Point PhysicalPosition {
                get {
                    Point pos = new Point();
                    GetPhysicalCursorPos(ref pos);
                    return pos;
                }
            }
            public static int X {
                get {
                    return Position.X;
                }
            }
            public static int Y {
                get {
                    return Position.Y;
                }
            }

            #endregion

            #region Helper Functions
            public static bool MoveBy(int x, int y) {
                return Input.SendInput(new Input(new MouseInput(x, y, false))) > 0;
            }

            public static bool MoveTo(int x, int y) {
                return SetCursorPos(x, y);
            }

            public static bool NormalizedMoveTo(int x, int y) {
                return Input.SendInput(new Input(new MouseInput(x, y, true))) > 0;
            }

            public static bool ScaledMove(double scaleX, double scaleY) {
                return Input.SendInput(new Input(new MouseInput((int)(OneScreen * scaleX), (int)(OneScreen * scaleY), true))) > 0;
            }

            public static async void Click(MouseButton button = MouseButton.Left, int delay = 0, int holdFor = 0) {
                await Task.Delay(delay);
                Press(button);
                await Task.Delay(holdFor);
                Release(button);
            }

            public static void Press(MouseButton button = MouseButton.Left) {
                Input.SendInput(new Input(new MouseInput(button)));
            }

            public static void Release(MouseButton button = MouseButton.Left) {
                Input.SendInput(new Input(new MouseInput(button, false)));
            }

            #endregion

            public static void HookGlobalEvents() {
                if(GlobalMouseHook != IntPtr.Zero) {
                    System.Diagnostics.Debug.WriteLine("Already Hooked");
                    Console.WriteLine("Already Hooked");
                    return;
                }

                GlobalMouseEvent += OnGlobalMouseEvent;

                GlobalMouseHook = Input.SetWindowsHookExA(Hooks.WH_MOUSE_LL, GlobalMouseEvent, IntPtr.Zero, 0);

                if(GlobalMouseHook == IntPtr.Zero) {
                    System.Diagnostics.Debug.WriteLine("Hook Error: " + GetLastError());
                    Console.WriteLine("Hook Error: " + GetLastError());
                } else {
                    System.Diagnostics.Debug.WriteLine("Hook Successfull: " + GlobalMouseHook);
                    Console.WriteLine("Hook Successfull: " + GlobalMouseHook);
                }
            }

            public static void UnhookGlobalEvents() {
                if(GlobalMouseHook == IntPtr.Zero) {
                    System.Diagnostics.Debug.WriteLine("Not Hooked");
                    Console.WriteLine("Not Hooked");
                    return;
                }
                GlobalMouseEvent -= OnGlobalMouseEvent;
                Input.UnhookWindowsHookEx(GlobalMouseHook);
                System.Diagnostics.Debug.WriteLine("Unhook Successfull: " + GlobalMouseHook);
                Console.WriteLine("Unhook Successfull: " + GlobalMouseHook);
                GlobalMouseHook = IntPtr.Zero;
            }

            private static int OnGlobalMouseEvent(int nCode, IntPtr wParam, IntPtr lParam) {
                if(nCode >= 0) {
                    int type = wParam.ToInt32();
                    switch(type) {
                        case WM_LBUTTONDOWN:
                        case WM_LBUTTONUP:
                            LeftButton(type == WM_LBUTTONDOWN);
                            break;
                        case WM_RBUTTONDOWN:
                        case WM_RBUTTONUP:
                            RightButton(type == WM_RBUTTONDOWN);
                            break;
                        case WM_MBUTTONDOWN:
                        case WM_MBUTTONUP:
                            MiddleButton(type == WM_MBUTTONDOWN);
                            break;
                        case WM_XBUTTONDOWN:
                        case WM_XBUTTONUP:
                            XButton(type == WM_XBUTTONDOWN);
                            break;
                        case WM_MOUSEMOVE:
                            if(LastPosition != Position) {
                                Move?.Invoke(MouseInput.FromPointer(lParam));
                                LastPosition = Position;
                            }
                            break;
                        case WM_MOUSEWHEEL:
                        case WM_MOUSEHWHEEL:
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("Undefined: " + type);
                            Console.WriteLine("Undefined: " + type);
                            break;
                    }
                }

                return Input.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

                void LeftButton(bool down) {
                    if(down) {
                        LeftButtonDown = true;
                        OnLeftDown?.Invoke();
                    } else {
                        OnLeftUp?.Invoke();
                        if(LeftButtonDown)
                            OnLeftClick?.Invoke();
                        OnLeftUp?.Invoke();
                        LeftButtonDown = false;
                    }
                }

                void RightButton(bool down) {
                    if(down) {
                        RightButtonDown = true;
                        OnRightDown?.Invoke();
                    } else {
                        OnRightUp?.Invoke();
                        if(RightButtonDown)
                            OnRightClick?.Invoke();
                        RightButtonDown = false;
                    }
                }

                void MiddleButton(bool down) {
                    if(down) {
                        MiddleButtonDown = true;
                        OnMiddleDown?.Invoke();
                    } else {
                        OnMiddleUp?.Invoke();
                        if(MiddleButtonDown)
                            OnMiddleClick?.Invoke();
                        MiddleButtonDown = false;
                    }
                }

                void XButton(bool down) {
                    MouseInput mouseInput = MouseInput.FromPointer(lParam);
                    int button = Input.GetHighOrder((int)mouseInput.mouseData);
                    System.Diagnostics.Debug.WriteLine("Button: " + button);
                    if(down) {
                        OnXDown?.Invoke(mouseInput);
                        if(button == XBUTTON1) {
                            X1ButtonDown = true;
                            OnX1Down?.Invoke();
                        } else if(button == XBUTTON2) {
                            X2ButtonDown = true;
                            OnX2Down?.Invoke();
                        }
                    } else {
                        OnXUp?.Invoke(mouseInput);
                        if(button == XBUTTON1) {
                            OnX1Up?.Invoke();
                            if(X1ButtonDown)
                                OnX1Click?.Invoke();
                            X1ButtonDown = false;
                        } else if(button == XBUTTON2) {
                            OnX2Up?.Invoke();
                            if(X2ButtonDown)
                                OnX2Click?.Invoke();
                            X2ButtonDown = false;
                        }
                    }
                }
            }

        }
    }

    public static class Hooks {
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/about-hooks
        // Thread or Global
        public const int WH_CALLWNDPROC = 4; // Monitors messages before the system sends them to the destination window procedure
                                             // Thread or Global
        public const int WH_CALLWNDPROCRET = 12; // Monitors messages after they have been processed by the destination window procedure
                                                 // Thread or Global
        public const int WH_CBT = 5;
        // Thread or Global
        public const int WH_DEBUG = 9;
        // Thread or Global
        public const int WH_FOREGROUNDIDLE = 11;
        // Thread or Global
        public const int WH_GETMESSAGE = 3;
        // Global only
        public const int WH_JOURNALPLAYBACK = 1;
        // Global only
        public const int WH_JOURNALRECORD = 0;
        // Thread or Global
        public const int WH_KEYBOARD = 2;
        // Global only
        public const int WH_KEYBOARD_LL = 13;
        // Global only
        public const int WH_MOUSE = 7;
        // Global only
        public const int WH_MOUSE_LL = 14;
        // Thread or Global
        public const int WH_MSGFILTER = -1;
        // Thread or Global
        public const int WH_SHELL = 10;
        // Global only
        public const int WH_SYSMSGFILTER = 6;
    }

}
