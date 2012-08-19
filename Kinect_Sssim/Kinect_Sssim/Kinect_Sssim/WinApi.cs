using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Windows.Controls;

using System.Runtime.InteropServices;

namespace Kinect_Sssim
{
    class WinApi
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


        // EnumWindowsから呼び出されるコールバック関数WNDENUMPROCのデリゲート
        private delegate int EnumerateWindowsCallback(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumerateWindowsCallback lpEnumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(
            IntPtr hWnd,
            StringBuilder s,
            int nMaxCount
            );


        public void init()
        {


            #region SSSIMSTUDIO という名前のプロセスIDをすべて取得する

            string sClassName = "SSSIMSTUDIO";

            // SSSIMSTUDIO という名前のプロセスをすべて取得する
            System.Diagnostics.Process[] hProcesses = System.Diagnostics.Process.GetProcessesByName(sClassName);
            int ProcessID = hProcesses.First().Id;

            if (ProcessID == 0)
            {
                //MessageBox.Show("SssimStudioを起動してください");
                return;
            }


            #endregion


            //プロセスIDからウィンドウハンドルを得る
            //hWnd = GetWindowHandle(ProcessID);


        }


        //public IntPtr GetThread(IntPtr handle)
        //{
        //    IntPtr threadEntry32 = IntPtr.Zero;
        //    THREADENTRY32 

        //    if (Thread32First(hWnd, threadEntry32))
        //    {
        //        do
        //        {


        //        } while (Thread32Next(hWnd, threadEntry32));
        //    }

        //    return threadEntry32;
        //}





        ////プロセスIDからウィンドウハンドルを得る
        //public IntPtr GetWindowHandle(string szClass)
        //{
        //    //EnumWindows(new EnumerateWindowsCallback(EnumerateWindows), 0);
        //    IntPtr handle = IntPtr.Zero;

        //    var ret = EnumWindows((IntPtr hWnd, int lParam) =>
        //    {
        //        int lpdwProcessId;
        //        var i = GetWindowThreadProcessId(hWnd, out lpdwProcessId);

        //        //GetWindowText(hWnd, szClass, sizeof(szClass));
        //        GetClassName(hWnd, szClass, sizeof(szClass));

        //        if (lpdwProcessId == ProcessID) {
        //            handle = hWnd;
        //            return 0;
        //        }
        //        // 列挙を継続するには0以外を返す必要がある
        //        return 1;
        //    }, 0);

        //    return handle;
        //}




        //プロセスIDからウィンドウハンドルを得る
        public IntPtr GetWindowHandle(int ProcessID)
        {
            //EnumWindows(new EnumerateWindowsCallback(EnumerateWindows), 0);
            IntPtr handle = IntPtr.Zero;

            var ret = EnumWindows((IntPtr hWnd, int lParam) =>
            {
                int lpdwProcessId;
                var i = GetWindowThreadProcessId(hWnd, out lpdwProcessId);

                if (lpdwProcessId == ProcessID)
                {
                    handle = hWnd;
                    return 0;
                }
                // 列挙を継続するには0以外を返す必要がある
                return 1;
            }, 0);

            return handle;
        }



        // ウィンドウを列挙するためのコールバックメソッド
        public static int EnumerateWindows(IntPtr hWnd, int lParam)
        {

            //// ウィンドウが可視かどうか調べる
            //if (IsWindowVisible(hWnd))
            //    // 可視の場合
            //    PrintCaptionAndProcess(hWnd);

            // 列挙を継続するには0以外を返す必要がある
            return 1;
        }


        //private void textBlock1_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    var text = sender as TextBlock;

        //    // ドラッグ アンド ドロップを開始する
        //    DragDrop.DoDragDrop(text, text.Text, DragDropEffects.Copy);
        //    DragDrop.DropEvent += (RoutedEvent a)=>{

        //    }
        //}


    }
}
