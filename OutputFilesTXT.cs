using PluginContracts;
using System.Drawing;
using System.Windows.Forms;
using TSOutputWriter;
using System.IO;
using System.Text;
using OutputHelperLib;
using System.Collections.Generic;
using System;



namespace OutputFilesTXT
{
    public class OutputFilesTXT : OutputPlugin
    {


        public string[] InputType { get; } = { "String" };
        public string OutputType { get; } = "None";

        public object Input { get; set; }
        public object Output { get; set; }
        public bool KeepStreamOpen { get; } = false;
        public ThreadsafeOutputWriter Writer { get; set;  }
        //for this writer, we don't actually need to write the header
        public Dictionary<int, string> OutputHeaderData { get; set; }
        public bool headerWritten { get; set; } = false;
        public void WriteHeader() { }
        public bool InheritHeader { get; } = false;

        public FileMode fileMode { get; set; } = FileMode.Create;


        #region IPlugin Details and Info

        public string PluginName { get; } = "Save .txt Files to Folder";
        public string PluginType { get; } = "Save Output File(s)";
        public string PluginVersion { get; } = "1.0.6";
        public string PluginAuthor { get; } = "Ryan L. Boyd (ryan@ryanboyd.io)";
        public string PluginDescription { get; } = "This plugin will save your output to .txt files contained within a folder. Requires input from parent plugin to be a string.";
        public bool TopLevel { get; } = false;
        public string PluginTutorial { get; } = "https://youtu.be/pVFUeVD_Jts";

        private bool fileWriteError { get; set; } = false;


        public Icon GetPluginIcon
        {
            get
            {
                return Properties.Resources.icon;
            }
        }

        #endregion




        #region Settings and ChangeSettings() Method

        public string OutputLocation { get; set; } = "";
        private bool AppendFiles = false;
        public string SelectedEncoding { get; set; } = "utf-8";



        public void ChangeSettings()
        {



            using (var form = new SettingsForm_OutputFilesTXT(OutputLocation, AppendFiles, SelectedEncoding))
            {


                form.Icon = Properties.Resources.icon;
                form.Text = PluginName;


                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SelectedEncoding = form.SelectedEncoding;
                    OutputLocation = form.TextFileDirectory;
                    AppendFiles = form.AppendFiles;
                }
            }



        }
        #endregion







        //sets GetTextList with the files to be analyzed
        public Payload RunPlugin(Payload Incoming)
        {

            Payload pData = new Payload();
            pData.FileID = Incoming.FileID;
            pData.SegmentID = Incoming.SegmentID;


            //if we have *at least* one item in the segment IDs, we're going to want to append numbers to filenames
            bool AppendSegID = (Incoming.SegmentID.Count >= 1);
            bool AppendNumbers = (Incoming.StringList.Count > 1);
                    
            for (int counter = 0; counter < Incoming.StringList.Count; counter++)
            {



                

                string OutputFilename = Incoming.FileID;

                //clean up filename
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    OutputFilename = OutputFilename.Replace(c, '_');
                }

                if (Path.GetExtension(OutputFilename) != ".txt") OutputFilename += ".txt";


                //if we have more than one item to write, we want to replace the extension (e.g., ".txt")
                //to prepend it with the output item number (e.g., ";1.txt")
                if (AppendSegID)
                {
                    OutputFilename = Path.GetFileNameWithoutExtension(OutputFilename);
                    OutputFilename += ";" + (Incoming.SegmentID[counter]).ToString();
                    OutputFilename += ".txt";
                }

                //clean up filename
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    OutputFilename = OutputFilename.Replace(c, '_');
                }

                if (AppendNumbers)
                {
                    OutputFilename = Path.GetFileNameWithoutExtension(OutputFilename);
                    OutputFilename += ";" + (Incoming.SegmentNumber[counter]).ToString();
                    OutputFilename += Path.GetExtension(Incoming.FileID);
                    OutputFilename += ".txt";


                }


                //clean up filename
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    OutputFilename = OutputFilename.Replace(c, '_');
                }


                //sets up the actual output to write
                string OutputToWrite = Incoming.StringList[counter];

                string OutputPath = Path.Combine(OutputLocation, OutputFilename);
                ////if the incoming object is one of the types of variables that we can write (string or string[])
                ////then we'll convert it and write it. Otherwise, we don't write it
                //if ((OutputObject.InputObject is string) || (OutputObject.InputObject is string[]))
                //{
                //    if (OutputObject.InputObject is string)
                //    {
                //        OutputToWrite = (string)OutputObject.InputObject;
                //    }
                //    else if (OutputObject.InputObject is string[])
                //    {
                //        OutputToWrite = string.Join(" ", (string[])OutputObject.InputObject);
                //    }

                //we use a new instance of the writer for this (rather than the writer specified at the top for the plugin -- "Writer"
                //because that one isn't safe for parallel operations.
                try
                {

                    using (ThreadsafeOutputWriter Plugin_Safe_Writer = new ThreadsafeOutputWriter(OutputPath, Encoding.GetEncoding(SelectedEncoding), fileMode))
                    {
                        Plugin_Safe_Writer.WriteString(OutputToWrite);
                    }
                }
                catch
                {
                    fileWriteError = true;
                }

            }


            return (new Payload());


        }



        public void Initialize()
        {
            fileWriteError = false;

            //makes sure that we're using the write file writing mode
            if (AppendFiles)
            {
                fileMode = FileMode.Append;
            }
            else
            {
                fileMode = FileMode.Create;
            }

        }

        public bool InspectSettings()
        {
            if (string.IsNullOrEmpty(OutputLocation))
            {
                return false;
            }
            else
            {
                return true;
            }

        }


        public Payload FinishUp(Payload Input)
        {
            Payload nullPL = this.RunPlugin(Input);
            if (fileWriteError)
            {
                MessageBox.Show("The \"" + PluginName + "\" plugin was not able to write one or more of your .txt files. This can happen if you are trying to use invalid filenames (e.g., filenames are too long or contain invalid filename characters) to write your .txt files, or if there is an issue with your write permissions.", PluginName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return (nullPL);
        }
        




        #region Import/Export Settings
        public void ImportSettings(Dictionary<string, string> SettingsDict)
        {
            SelectedEncoding = SettingsDict["SelectedEncoding"];
            OutputLocation = SettingsDict["OutputLocation"];
            AppendFiles = Boolean.Parse(SettingsDict["AppendFiles"]);
        }

        public Dictionary<string, string> ExportSettings(bool suppressWarnings)
        {
            Dictionary<string, string> SettingsDict = new Dictionary<string, string>();
            SettingsDict.Add("SelectedEncoding", SelectedEncoding);
            SettingsDict.Add("OutputLocation", OutputLocation);
            SettingsDict.Add("AppendFiles", AppendFiles.ToString());
            return (SettingsDict);
        }
        #endregion

    }

}


