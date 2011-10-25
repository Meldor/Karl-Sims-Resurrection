using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace ProveMotoreFisico
{
    class WinFormsHelper
    {
        public static ListBox CreateListBox(String[] items, Vector2 position, Vector2? size, GameWindow window)
        {
            ListBox list = new System.Windows.Forms.ListBox();
            list.Items.AddRange(items);
            list.Left = Convert.ToInt32(position.X);
            list.Top = Convert.ToInt32(position.Y);
            System.Windows.Forms.Control.FromHandle(window.Handle).Controls.Add(list);
            if (size != null)
            {
                list.Width = Convert.ToInt32(((Vector2)size).X);
                list.Height = Convert.ToInt32(((Vector2)size).Y);
            }
            return list;
        }
    }
}
