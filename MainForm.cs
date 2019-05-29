using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace TaskStack
{
    public partial class MainForm : Form
    {
        KeyboardHook hook = new KeyboardHook();
        public List<string> tasks;

        public MainForm() {
            InitializeComponent();
            listBox.ItemHeight = 30;
            notifyIcon.Icon = TaskStack.Properties.Resources.icon;

            if (!File.Exists("TaskStack.txt")) {
                File.Create("TaskStack.txt");
            }
            else {
                tasks = File.ReadAllLines("TaskStack.txt").ToList().Where(l => l.Trim().Length > 0).ToList();
                UpdateList();
            }

            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            hook.RegisterHotKey(TaskStack.ModifierKeys.None, Keys.Pause);

            FixFocus();
        }

        private void FixFocus() {
            Show(); Activate(); Focus();
            textBox.Focus();
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e) {
            FixFocus();
        }

        private void ContextMenu_Quit_Clicked(object sender, EventArgs e) {
            notifyIcon.Dispose();
            Environment.Exit(0);
        }

        private void ContextMenu_Open_Clicked(object sender, EventArgs e) {
            FixFocus();
        }

        private void UpdateList() {
            listBox.Items.Clear();
            currentTaskLabel.Text = "";

            if (tasks.Count == 0) return;
            currentTaskLabel.Text = tasks[0];
            tasks.Skip(1).ToList().ForEach(t => {
                listBox.Items.Add(t);
            });
            listBox.Invalidate();
        }


        private void Save() {
            File.WriteAllText("TaskStack.txt", tasks.Aggregate("", (a, b) => a + "\r\n" + b));
        }

        private void PushButton_Click(object sender, EventArgs e) { PushItem(); FixFocus(); }
        private void PopButton_Click(object sender, EventArgs e) { PopItem(); FixFocus(); }
        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) { PushItem(); }
            else if (e.KeyCode == Keys.Oemtilde) { PopItem(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { Hide(); }
        }
        private void PushItem() {
            var text = textBox.Text.Trim();
            if (text.Length > 0) {
                tasks = tasks.Prepend(text).ToList();
                UpdateList();
                Save();
                textBox.Text = "";
            }
        }

        private void PopItem() {
            if (tasks.Count > 0) {
                tasks.RemoveAt(0);
                UpdateList();
                Save();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        private void NotifyIcon1_DoubleClick(object sender, EventArgs e) {
            FixFocus();
        }

        private void NotifyIcon1_Click(object sender, EventArgs e) {
            contextMenuStrip.Show();
        }

        private void MainForm_Activated(object sender, EventArgs e) {
            FixFocus();
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e) {
            var value = listBox.SelectedItem as string;
            if (value == null) return;

            tasks.RemoveAt(listBox.SelectedIndex + 1);
            tasks = tasks.Prepend(value).ToList();
            UpdateList();
            Save();
            FixFocus();
        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index == -1) return; // ???

            e.DrawBackground();
            Brush myBrush = Brushes.Black;
            var bounds = e.Bounds;
            e.Graphics.DrawString(listBox.Items[e.Index].ToString(),
                e.Font,
                myBrush,
                new Rectangle(e.Bounds.X, e.Bounds.Y + 2, e.Bounds.Width, e.Bounds.Height),
                StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }
    }
}
