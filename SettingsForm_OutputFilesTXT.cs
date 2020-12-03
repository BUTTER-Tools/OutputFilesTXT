using System.Text;
using System.Windows.Forms;

namespace OutputFilesTXT
{
    internal partial class SettingsForm_OutputFilesTXT : Form
    {


        #region Get and Set Options

        public string TextFileDirectory { get; set; }
        public bool AppendFiles { get; set; }
        public string SelectedEncoding { get; set; }

       #endregion



        public SettingsForm_OutputFilesTXT(string TextFileDirectory, bool AppendFileInput, string SelectedEncoding)
        {
            InitializeComponent();

            foreach (var encoding in Encoding.GetEncodings())
            {
                EncodingDropdown.Items.Add(encoding.Name);
            }

            try
            {
                EncodingDropdown.SelectedIndex = EncodingDropdown.FindStringExact(SelectedEncoding);
            }
            catch
            {
                EncodingDropdown.SelectedIndex = EncodingDropdown.FindStringExact(Encoding.Default.BodyName);
            }

            AppendFileCheckbox.Checked = AppendFileInput;
            SelectedFolderTextbox.Text = TextFileDirectory;

        }






        private void SetFolderButton_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = true;
                dialog.Description = "Please choose the output location for your .txt files";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SelectedFolderTextbox.Text = dialog.SelectedPath.ToString();
                }
            }
        }


        private void OKButton_Click(object sender, System.EventArgs e)
        {
            this.SelectedEncoding = EncodingDropdown.SelectedItem.ToString();
            this.AppendFiles = AppendFileCheckbox.Checked;
            this.TextFileDirectory = SelectedFolderTextbox.Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
