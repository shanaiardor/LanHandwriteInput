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
            SendKeyDown(VK_CONTROL);
            SendVirtualKey(VK_V);
            SendKeyUp(VK_CONTROL);
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
            Span<INPUT> inputs = stackalloc INPUT[2];
            inputs[0] = new INPUT
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
            };
            inputs[1] = new INPUT
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
            };

            SendInput((uint)inputs.Length, ref inputs[0], Marshal.SizeOf<INPUT>());
        }

        private static void SendVirtualKey(ushort virtualKey)
        {
            SendKeyDown(virtualKey);
            SendKeyUp(virtualKey);
        }

        private static void SendKeyDown(ushort virtualKey)
        {
            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey
                    }
                }
            };

            SendInput(1, ref input, Marshal.SizeOf<INPUT>());
        }

        private static void SendKeyUp(ushort virtualKey)
        {
            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        dwFlags = KEYEVENTF_KEYUP
                    }
                }
            };

            SendInput(1, ref input, Marshal.SizeOf<INPUT>());
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
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
    }
}
