using System;
using System.IO;
using System.Windows.Forms;

namespace SharpEdit
{
    public partial class Form1 : Form
    {
        // Tracks the path of the currently opened file
        private string _CurrentFilePath = "";

        // Flag to check if the text has been modified since the last save
        private bool _IsDirty = false;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates the Form title with the file name and an asterisk if there are unsaved changes.
        /// </summary>
        private void _UpdateFormTitle()
        {
            string fileName = string.IsNullOrEmpty(_CurrentFilePath) ? "Untitled" : Path.GetFileName(_CurrentFilePath);
            string prefix = _IsDirty ? "*" : "";
            this.Text = $"{prefix}Sharp Edit - {fileName}";
        }

        // --- File Menu Operations ---

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _CurrentFilePath = openFileDialog.FileName;

                // Read file content and load it into the RichTextBox
                using (StreamReader reader = new StreamReader(openFileDialog.OpenFile()))
                {
                    rtbText.Text = reader.ReadToEnd();
                }

                // Reset the dirty flag after loading a new file
                _IsDirty = false;
                _UpdateFormTitle();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If file already exists, save directly. Otherwise, trigger Save As.
            if (!string.IsNullOrEmpty(_CurrentFilePath))
            {
                try
                {
                    File.WriteAllText(_CurrentFilePath, rtbText.Text);
                    _IsDirty = false;
                    _UpdateFormTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during save: " + ex.Message);
                }
            }
            else
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _CurrentFilePath = saveFileDialog.FileName;
                    File.WriteAllText(_CurrentFilePath, rtbText.Text);

                    _IsDirty = false;
                    _UpdateFormTitle();
                    MessageBox.Show("File saved successfully!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // This triggers the FormClosing event automatically
            this.Close();
        }

        // --- Form Events ---

        private void rtbText_TextChanged(object sender, EventArgs e)
        {
            // Mark the file as modified and update title
            if (!_IsDirty)
            {
                _IsDirty = true;
                _UpdateFormTitle();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Logic to handle unsaved changes before closing
            if (_IsDirty)
            {
                DialogResult result = MessageBox.Show(
                    "Do you want to save changes to this file?",
                    "Sharp Edit",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                    // If after saving it's still dirty (e.g. Save As was cancelled), stay in the app
                    if (_IsDirty) e.Cancel = true;
                }
                else if (result == DialogResult.Cancel)
                {
                    // Stop the form from closing
                    e.Cancel = true;
                }
                // If No, the form will close naturally
            }
        }
    }
}