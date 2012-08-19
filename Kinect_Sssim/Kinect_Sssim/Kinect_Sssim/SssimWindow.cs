using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Windows.Controls;

using System.Runtime.InteropServices;


namespace Kinect_Sssim
{
    public static class SssimWindow
    {


        #region WINAPI

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String sClassName, String sWindowText);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hwndChildAfter, String lpszClass, String lpszWindow);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        #endregion



        /// <summary>
        ///  太陽系シミュレータースタジオ [ 640 x 480 ]のウィンドウハンドル
        /// </summary>
        public static IntPtr get_Sssim_hWnd()
        {
            IntPtr hWnd;
            string sWindowText = null;


            #region 太陽系シミュレータースタジオ [ 640 x 480 ]のウィンドウハンドル

            //太陽系シミュレータースタジオのウィンドウハンドル
            string sClassName = "QWidget";
            if ((hWnd = FindWindow(sClassName, sWindowText)) == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }


            //子ウィンドウのハンドル
            sClassName = "QWidget";
            hWnd = FindWindowEx(hWnd, IntPtr.Zero, sClassName, sWindowText);

            // Sssimの描画エリアハンドル取得
            sClassName = "QWidgetOwnDC";
            hWnd = FindWindowEx(hWnd, IntPtr.Zero, sClassName, sWindowText);

            #endregion

            return hWnd;
        }


        public static void Test_Move(IntPtr hWnd)
        {

            #region 動かす
            int sl = 30;
            // WM_KEYDOWNメッセージ送信
            MouseWinAPI.SendLeftButtonDown(hWnd, 155, 333);
            MouseWinAPI.SendMouseMove(hWnd, 155, 334); System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendMouseMove(hWnd, 155, 334); System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendMouseMove(hWnd, 155, 335); System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendMouseMove(hWnd, 155, 336); System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendMouseMove(hWnd, 155, 337); System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendMouseMove(hWnd, 155, 338); System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendMouseMove(hWnd, 155, 339); System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendMouseMove(hWnd, 155, 340); System.Threading.Thread.Sleep(sl);

            System.Threading.Thread.Sleep(sl);
            MouseWinAPI.SendLeftButtonUp(hWnd, 155, 341);

            #endregion

        }



    }
}
