﻿using System;
using System.Drawing;

namespace VK_Unicorn
{
    public enum LogLevel
    {
        NOTIFY,
        GENERAL,
        SUCCESS,
        WARNING,
        ERROR,
    }

    public enum StatusType
    {
        GENERAL,
        SUCCESS,
        ERROR,
    }

    class Utils
    {
        public static void Log(string text, LogLevel logLevel = LogLevel.GENERAL)
        {
            Color? color = null;
            var prefix = string.Empty;

            switch (logLevel)
            {
                case LogLevel.ERROR:
                    color = Color.Red;
                    prefix = "Ошибка: ";
                    break;

                case LogLevel.WARNING:
                    color = Color.Yellow;
                    break;

                case LogLevel.NOTIFY:
                    color = Color.Gray;
                    break;

                case LogLevel.SUCCESS:
                    color = Color.DarkGreen;
                    break;
            }

            Log(prefix + text, color);
        }

        public static void Log(string text, Color? color = null)
        {
            text = "[" + DateTime.Now.ToLongTimeString() + "] " + text;

            var logTextBox = MainForm.Instance.GetLogTextBox();

            logTextBox.SuspendLayout();

            var previousSelectionColor = logTextBox.SelectionColor;
            if (color != null)
            {
                logTextBox.SelectionColor = color.Value;
            }

            if (!string.IsNullOrWhiteSpace(logTextBox.Text))
            {
                logTextBox.AppendText($"{Environment.NewLine}{text}");
            }
            else
            {
                logTextBox.AppendText(text);
            }

            logTextBox.ScrollToCaret();
            logTextBox.SelectionColor = previousSelectionColor;
            logTextBox.ResumeLayout();
        }
    }

    public delegate void Callback();
    public delegate void Callback<T0>(T0 arg0);
    public delegate void Callback<T0, T1>(T0 arg0, T1 arg1);
    public delegate void Callback<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    public delegate void Callback<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    public delegate void Callback<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void Callback<T0, T1, T2, T3, T4, T5>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void Callback<T0, T1, T2, T3, T4, T5, T6>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void Callback<T0, T1, T2, T3, T4, T5, T6, T7>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate void Callback<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public delegate U CallbackWithReturn<U>();
    public delegate U CallbackWithReturn<U, T0>(T0 arg0);
    public delegate U CallbackWithReturn<U, T0, T1>(T0 arg0, T1 arg1);
    public delegate U CallbackWithReturn<U, T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5, T6>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5, T6, T7>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
}
