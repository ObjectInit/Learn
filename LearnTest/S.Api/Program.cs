﻿#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2020/4/9 14:34:47 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.S.Api
{
    class Program : IMain
    {
        public class MouseFlag
        {
            enum MouseEventFlag : uint
            {
                Move = 0x0001,
                LeftDown = 0x0002,
                LeftUp = 0x0004,
                RightDown = 0x0008,
                RightUp = 0x0010,
                MiddleDown = 0x0020,
                MiddleUp = 0x0040,
                XDown = 0x0080,
                XUp = 0x0100,
                Wheel = 0x0800,
                VirtualDesk = 0x4000,
                Absolute = 0x8000
            }

            [DllImport("user32.dll")]
            static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);

            [DllImport("user32.dll")]
            public static extern int SetCursorPos(int x, int y);

            public static void MouseLeftClickEvent(int dx, int dy, uint data)
            {
                SetCursorPos(dx, dy);
                System.Threading.Thread.Sleep(2 * 1000);
                mouse_event(MouseEventFlag.LeftDown, dx, dy, data, UIntPtr.Zero);
                mouse_event(MouseEventFlag.LeftUp, dx, dy, data, UIntPtr.Zero);
            }

            public static void MouseRightClickEvent(int dx, int dy, uint data)
            {
                SetCursorPos(dx, dy);
                System.Threading.Thread.Sleep(2 * 1000);
                mouse_event(MouseEventFlag.RightDown, dx, dy, data, UIntPtr.Zero);
                mouse_event(MouseEventFlag.RightUp, dx, dy, data, UIntPtr.Zero);
            }
           
        }
        public void Main(string[] args)
        {
            System.Threading.Thread.Sleep(3 * 1000);
            MouseFlag.MouseLeftClickEvent(10, 1000, 0);
        }
    }
}