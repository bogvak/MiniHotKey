using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MiniHotKey
{
    class InterceptKeys
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static uint _ruLayout;
        private static uint _enLayout;
        private static string en_US = "00000409";
        private static string ru_RU = "00000419";
        private static uint KLF_ACTIVATE = 0x00000001;
        private static uint KLF_SETFORPROCESS = 0x00000100;
        private static int HWND_BROADCAST = 0xffff;
        private static uint WM_INPUTLANGCHANGEREQUEST = 0x0050;

        public static void InitializeComponent()
        {
            _ruLayout = (uint)LoadKeyboardLayout(ru_RU, KLF_SETFORPROCESS);
            _enLayout = (uint)LoadKeyboardLayout(en_US, KLF_SETFORPROCESS);
            _hookID = SetHook(_proc);
            //Application.Run();
            //UnhookWindowsHookEx(_hookID);
        }

        public static void UnInitializeComponent()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Console.WriteLine((Keys)vkCode);
                // DialogResult dialogResult = MessageBox.Show(vkCode.ToString(), "Some Title", MessageBoxButtons.YesNo);
                if (vkCode == 20)
                {
                    ChangeLanguageLayout();
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void ChangeLanguageLayout()
        {
            var threadId = GetProcessMainThreadId(0);
            var keyboardLayoutId = (uint)GetKeyboardLayout((UInt32)threadId);

            PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, keyboardLayoutId == _enLayout ? new IntPtr(_ruLayout) : new IntPtr(_enLayout));
        }

        private static Int32 GetProcessMainThreadId(Int32 processId = 0)
        {
            var process = 0 == processId ? Process.GetCurrentProcess() : Process.GetProcessById(processId);
            return process.Threads[0].Id;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(int hhwnd, uint msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(UInt32 idThread);
    }
}
