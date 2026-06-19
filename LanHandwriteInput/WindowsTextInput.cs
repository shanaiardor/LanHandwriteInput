using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LanHandwriteInput
{
    internal static class WindowsTextInput
    {
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const ushort VK_BACK = 0x08;
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_RETURN = 0x0D;
        private const ushort VK_V = 0x56;

        public static void PasteText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (text == "\b")
            {
                SendVirtualKey(VK_BACK);
                return;
            }

            Clipboard.SetText(text);
            Thread.Sleep(50);
            SendChord(VK_CONTROL, VK_V);
        }

        public static void SendEnter()
        {
            SendVirtualKey(VK_RETURN);
        }

        public static void SendText(string text)
        {
            foreach (var ch in text)
            {
                if (ch == '\b')
                {
                    SendVirtualKey(VK_BACK);
                }
                else
                {
                    SendUnicodeChar(ch);
                }
            }
        }

        private static void SendUnicodeChar(char ch)
        {
            var inputs = new[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = ch,
                            dwFlags = KEYEVENTF_UNICODE
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = ch,
                            dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP
                        }
                    }
                }
            };

            SendInputs(inputs);
        }

        private static void SendVirtualKey(ushort virtualKey)
        {
            var inputs = new[]
            {
                CreateKeyInput(virtualKey, 0),
                CreateKeyInput(virtualKey, KEYEVENTF_KEYUP)
            };

            SendInputs(inputs);
        }

        private static void SendChord(ushort modifierKey, ushort key)
        {
            var inputs = new[]
            {
                CreateKeyInput(modifierKey, 0),
                CreateKeyInput(key, 0),
                CreateKeyInput(key, KEYEVENTF_KEYUP),
                CreateKeyInput(modifierKey, KEYEVENTF_KEYUP)
            };

            SendInputs(inputs);
        }

        private static INPUT CreateKeyInput(ushort virtualKey, uint flags)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        dwFlags = flags
                    }
                }
            };
        }

        private static void SendInputs(INPUT[] inputs)
        {
            var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
            if (sent != inputs.Length)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"SendInput failed: sent {sent}/{inputs.Length}, INPUT size {Marshal.SizeOf<INPUT>()}");
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
    }
}

