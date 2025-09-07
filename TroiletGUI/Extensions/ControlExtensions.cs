using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TroiletGUI.Extensions
{
    internal static class ControlExtensions
    {
        public static readonly Brush HOVER_BRUSH = new SolidColorBrush(Color.FromArgb(130, 230, 230, 230));

        public delegate void OnButtonClick(object sender, MouseButtonEventArgs e);

        public static void MakeButton(this Control cntrl, OnButtonClick onClick, Brush? onHover = null)
        {
            if (onHover == null)
                onHover = HOVER_BRUSH;

            Brush oldCol = cntrl.Background;
            cntrl.MouseEnter += (s, e) => cntrl.Background = onHover;
            cntrl.MouseLeave += (s, e) => cntrl.Background = oldCol;
            cntrl.MouseDown += (s, e) => onClick(s, e);
        }
    }
}
