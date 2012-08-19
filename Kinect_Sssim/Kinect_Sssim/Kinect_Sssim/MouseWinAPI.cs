using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Runtime.InteropServices;


namespace Kinect_Sssim
{
    public class MouseWinAPI
    {

        public const int WM_COMMAND = 0x0111;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_INITDIALOG = 0x0110;

        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;

        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;


        public const int WM_MOVE = 0x0003;
        public const int WM_NOTIFY = 0x004e;

        public const int LB_RESETCONTENT = 0x184;
        public const int LB_ADDSTRING = 0x180;
        public const int LB_GETSEL = 0x187;
        public const int LB_SETHORIZONTALEXTENT = 0x193;
        public const int LB_GETCOUNT = 0x18b;
        public const int LB_SETCURSEL = 0x186;
        public const int LB_GETCURSEL = 0x188;
        public const int LB_GETTEXT = 0x189;
        public const int LB_GETTEXTLEN = 0x18a;
        public const int LB_FINDSTRING = 0x18f;
        public const int LB_ERR = -1;

        public const int IDOK = 1;
        public const int IDCANCEL = 2;
        public const int IDC_EDIT_FIND = 1000;
        public const int IDC_EDIT_REPLACE = 1001;
        public const int IDC_STATIC_VIEW = 1002;
        public const int IDC_STATIC_STATE = 1003;
        public const int IDC_STATIC_FIND = 1004;
        public const int IDC_STATIC_REPLACE = 1005;
        public const int IDC_BUTTON_FIND = 1007;
        public const int IDC_BUTTON_REPLACE = 1008;
        public const int IDC_LIST_FOUND = 1009;
        public const int IDC_TAB = 1010;


        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;



        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);




        public static void SendLeftButtonDown(IntPtr hWnd, int x, int y)
        {
            PostMessage(hWnd, WM_LBUTTONDOWN, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
        }

        public static void SendLeftButtonUp(IntPtr hWnd, int x, int y)
        {
            PostMessage(hWnd, WM_LBUTTONUP, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
        }

        public static void SendLeftButtondblclick(IntPtr hWnd, int x, int y)
        {
            PostMessage(hWnd, WM_LBUTTONDBLCLK, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
        }

        public static void SendRightButtonDown(IntPtr hWnd, int x, int y)
        {
            PostMessage(hWnd, WM_RBUTTONDOWN, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
        }

        public static void SendRightButtonUp(IntPtr hWnd, int x, int y)
        {
            PostMessage(hWnd, WM_RBUTTONUP, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
        }

        public static void SendRightButtondblclick(IntPtr hWnd, int x, int y)
        {
            PostMessage(hWnd, WM_RBUTTONDBLCLK, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
        }

        public static void SendMouseMove(IntPtr hWnd, int x, int y)
        {
            PostMessage(hWnd, WM_MOUSEMOVE, IntPtr.Zero, new IntPtr(y * 0x10000 + x));
        }

        //public static void SendKeyDown(IntPtr hWnd, int key)
        //{
        //    PostMessage(hWnd, WM_KEYDOWN, key, IntPtr.Zero);
        //}

        //public static void SendKeyUp(IntPtr hWnd, int key)
        //{
        //    PostMessage(hWnd, WM_KEYUP, key, new IntPtr(1));
        //}

        //public static void SendChar(IntPtr hWnd, char c)
        //{
        //    SendMessage(hWnd, WM_CHAR, c, IntPtr.Zero);
        //}

        //public static void SendString(IntPtr hWnd, string s)
        //{
        //    foreach (char c in s) SendChar(c);
        //}





        public IntPtr MAKELPARAM(IntPtr hWnd, int x, int y)
        {
            return new IntPtr(y * 0x10000 + x);
        }




    }
}
