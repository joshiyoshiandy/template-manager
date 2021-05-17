//: Template Manager

/* 
  Title: MainWindow.xaml.cs
  Date: January 6, 2019
  Author: Andy Joshi
  Version: 2.0
  Copyright: 2018 Andy Joshi
*/

// System namespaces
using System.IO;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;

// Other namespaces
using Microsoft.Win32;

namespace TemplateManager
{
    public partial class MainWindow : Window
    {
        // MASTER_PATH points to the main location where all templates are stored
        private readonly string MAIN_DIR = Directory.GetCurrentDirectory() + "\\Templates";

        // List of all template files located in the main directory
        private List<FileTemplate> ALL_TEMPLATE_FILES = new List<FileTemplate>();

        // Tag attached to the name of a file that needs to be cleaned up at the end of runtime
        private const string DELETE_TAG = "C:\\Users\\Public\\Documents\\[TEMP_COPY] ";

        // Files required in the executable's directory for proper functionality
        private const string REQ_DOC = "Documentation.pdf";

        public MainWindow() // - Determines what to do when MainWindow is launched
        {
            // Check if the main path can be accessed (important to application's function)
            if (!Directory.Exists(MAIN_DIR))
            {
                ShowMsg("Main directory was modified:" + "\n" + MAIN_DIR +
                        "\nUndo all changes to this directory for the application to function.");
            }
            InitializeComponent(); // Standard procedure to initialize and show the form to the user
            DefaultAllControls(); // Set up the default appearance of the form's controls
        }

        // Event for when the user closes the application
        private void Window_Closing(object sender, CancelEventArgs e) // - Enforces cleanup of all temporary files (tempFiles) created via this application 
                                                                      //   See BtnViewTemplate_Click & BtnHelp_Click for more context
        {
            List<FileTemplate> tempFiles = new List<FileTemplate>(); // tempFiles is a list of all files that can be opened for viewing temporarily via the application
            tempFiles.AddRange(ALL_TEMPLATE_FILES); // Adding all template files to this list

            FileTemplate documentation = new FileTemplate(Directory.GetCurrentDirectory() + '\\' + REQ_DOC);
            tempFiles.Add(documentation); // Need to add the help/documentation file to this list because a temporary version of that can also be opened

            foreach (FileTemplate f in tempFiles) // For each tempFile...
            {
                string path = DELETE_TAG + f.Name; // Path of tempFile file if it exists

                while (File.Exists(path)) // Check if the file path exists and do not exit the application until the file is deleted
                {
                    e.Cancel = false; // Close the application if nothing goes wrong
                    if (!IsFileInUse(path)) // Check if file is currently in use
                    {
                        File.Delete(path); // File is not in use, delete it
                    }
                    else // File could not be accessed due to a conflict (see IsFileInUse for cases where files cannot be accessed)
                    {
                        ShowMsg("Please close all viewed templates and help documents before exiting." +
                                "\nLook for the [TEMP COPY] tag in the file name (top of window)."); // Instruct the user to close any temporary files they have open (to delete them)
                        e.Cancel = true; // Files need to be closed and deleted, do not close the application
                    }
                }
            }
        }

        // Event for when the text in the filter textbox is edited by the user
        private void TBoxNameFilter_TextChanged(object sender, TextChangedEventArgs e) // - Determines what templates to show to the user based on their specified keyword(s)
        {
            if (TBoxNameFilter.Text != "") // Check if the textbox is empty
            {
                // Create and show a new list of templates based on the template name property containing the keyword(s) from the textbox (not case sensitive)
                List<FileTemplate> filtered = new List<FileTemplate>(ALL_TEMPLATE_FILES.Where(t => t.FilterByName.Contains(TBoxNameFilter.Text.ToLower())));
                DisplayNewList(filtered);
            }
            else
            {
                DisplayNewList(ALL_TEMPLATE_FILES); // Empty textbox, show all the templates
            }
        }

        // Event for when the Help button is clicked
        private void BtnHelp_Click(object sender, RoutedEventArgs e) // - Creates a copy of the PDF help document and shows the copy to the user
        {
            string original = Directory.GetCurrentDirectory() + '\\' + REQ_DOC; // Get the path of the help file
            if (!File.Exists(original))
            {
                ShowMsg("Help file not found: " + REQ_DOC);
            }
            else
            {
                string copy = DELETE_TAG + REQ_DOC; // Determine the path of the copy of the help file
                ShowTempFile(original, copy); // Create a the copy of the help document and show it to the user
            }
        }

        // Event for when the Refresh button is clicked
        private void BtnRefreshTemplates_Click(object sender, RoutedEventArgs e) // - Resets the MainWindow's controls
        {
            DefaultAllControls(); // Refreshes the form's controls as if it were initially launching
        }

        // Event for when a selection is made or changed for a list-view item
        private void DGTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e) // - Updates the appearance of certain controls depending on what template was selected
        {
            // Check if an item has been selected
            if (DGTemplates.SelectedItem != null)
            {
                FileTemplate t = DGTemplates.SelectedItem as FileTemplate; // Determine what was selected from the list
                UpdateDirectory(t.Directory); // Show its directory in the directory textbox
                EnableButtons(); // Enable the template-specific command buttons
            }
            else
            {
                DisableButtons(); // No template has been selected, disable the template-specific buttons
            }
        }

        // Event for when the Create Template button is clicked
        private void BtnCreateTemplate_Click(object sender, RoutedEventArgs e) // - Checks for the required dll and launches the NewTemplateWindow
        {
            // Launch the NewTemplate window
            NewTemplateWindow newTemplateWindow = new NewTemplateWindow(MAIN_DIR);
            newTemplateWindow.Show();
        }

        // Event for when the Download Template button is clicked
        private void BtnDLTemplate_Click(object sender, RoutedEventArgs e) // - Copies an existing template, pastes it in a new location desired by the user, and launches the new file
        {
            FileTemplate t = DGTemplates.SelectedItem as FileTemplate; // Determine which template was selected in the list

            // Open a save dialog window asking the user where they want to save the file and only show them existing files that have identical file extensions
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*" + t.Extension + "*)|*" + t.Extension + "*",
                FilterIndex = 2,
                InitialDirectory = @"C:\",
                RestoreDirectory = true
            };

            if (sfd.ShowDialog() == true) // Check if the user's file name already contains the appropriate file extension
            {
                string savePath = sfd.FileName;
                string[] s = sfd.FileName.Split('.');

                if ("." + s[s.Length - 1] != t.Extension) // Check if the user has already specified the proper file extension
                {
                    savePath += t.Extension; // User did not include the file extension in their name so add the correct one
                }

                if (File.Exists(savePath)) { File.Delete(savePath); } // User may be trying overwrite an existing file so delete any file with that name if it exists

                try
                {
                    File.Copy(t.FilePath, savePath); // Copy the desired template to the new user-specified file location
                    Process.Start(savePath); // Launch the copied file
                }
                catch (IOException)
                {
                    CommonError(); // Something went wrong when trying to copy the file, try again
                }
            }
        }

        // Event for when the View Template button is clicked
        private void BtnViewTemplate_Click(object sender, RoutedEventArgs e) // - Creates a copy of the template wanting to be viewed and shows the copy to the user
        {
            FileTemplate t = DGTemplates.SelectedItem as FileTemplate; // Determine which template was selected in the list
            string copy = DELETE_TAG + t.Name; // See BtnHelp_Clicked for explanation (similar process)
            ShowTempFile(t.FilePath, copy);
        }

        private List<FileTemplate> CollectAllFiles(string path) // - Collects a list of all template files within a specified directory and returns it
        {
            List<FileTemplate> filesCollected = new List<FileTemplate>();
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            if (dirInfo.Exists)
            {
                // Get all subfolders and files in directory
                DirectoryInfo[] subDirInfo = dirInfo.GetDirectories();
                FileInfo[] fileInfo = dirInfo.GetFiles();

                // For each subfolder, get its subfolders and files and add them to the list
                foreach (DirectoryInfo subDir in subDirInfo)
                {
                    filesCollected = filesCollected.Concat(CollectAllFiles(subDir.FullName)).ToList();
                }

                // Create a FileTemplate object for each file and add it to the current list of templates in this directory
                foreach (FileInfo file in fileInfo)
                {
                    FileTemplate t = new FileTemplate(file.FullName);
                    if (t.Name.Substring(0, 2) != "~$") // Filter out any temp files (created by MS Office) that might be lingering
                    {
                        filesCollected.Add(t);
                    }
                }
            }
            return filesCollected;
        }

        public bool IsFileInUse(string path) // - Checks to see if a file is available to access and returns false if so
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                FileStream stream = null;
                try
                {
                    stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (IOException)
                {
                    // The file is unavailable because it is either:
                    // - still being written to
                    // - being processed by another thread
                    // - does not exist (has already been processed)
                    return true;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
                return false; // No exception was caught so the file can be used
            }
            else
            {
                return true; // File does not exist, cannot be accessed
            }
        }

        private void ShowTempFile(string originalPath, string copyPath) // - Copies a file and launches the copy of the original for viewing purposes
        {
            if (IsFileInUse(originalPath))
            {
                // Original file is in use or cannot be found
                ShowMsg("Could not view the file. The original file is in use or has been modified." +
                        "\nRefreshing list...");
                ResetDataGrid();
            }
            else
            {
                if (!File.Exists(copyPath)) // Check if the temporary file has already been generated
                {
                    // A temporary file does not already exist so create a copy of the original file and show it
                    File.Copy(originalPath, copyPath);
                    Process.Start(copyPath);
                }
                else // The temporary file already exists
                {
                    if (!IsFileInUse(copyPath))
                    {
                        // The temporary file is not already in use so launch it
                        Process.Start(copyPath);
                    }
                    else
                    {
                        // The temporary file is already in use
                        ShowMsg("Could not view the file. You may already be viewing this file.");
                    }
                }
            }
        }

        private void DisplayNewList(List<FileTemplate> templates) // - Updates the list of templates and displays the new list of templates
        {
            DGTemplates.Items.Clear();

            // Add the provided list of templates to the list-view
            foreach (FileTemplate t in templates)
            {
                DGTemplates.Items.Add(t);
            }

            DisableButtons(); // Selection of list item is erased in this process so disable the dependent buttons
        }

        private void ResetDataGrid() // - Reloads the list with an updated list of all the templates in the main directory
        {
            ALL_TEMPLATE_FILES = CollectAllFiles(MAIN_DIR).OrderBy(x => x.Name).ToList(); // Sort the list alphabetically using the file names
            DisplayNewList(ALL_TEMPLATE_FILES);
        }

        private void DisableButtons() // - Disables the View Template and Download Template buttons
        {
            BtnViewTemplate.IsEnabled = false;
            BtnDLTemplate.IsEnabled = false;
        }

        private void EnableButtons() // - Enables the View Template and Download Template buttons (only enabled after a template has been selected from the list)
        {
            BtnViewTemplate.IsEnabled = true;
            BtnDLTemplate.IsEnabled = true;
        }

        private void ResetFilter() // - Clears text in the filter textbox
        {
            TBoxNameFilter.Text = "";
        }

        private void ResetDirectory() // - Resets the directory textbox to show the main folder's directory
        {
            TBoxDirectory.Text = "Directory: " + MAIN_DIR;
        }

        private void UpdateDirectory(string directory) // - Updates the directory textbox with the provided text
        {
            TBoxDirectory.Text = "Directory: " + directory;
        }

        private void DefaultAllControls() // - Resets the form controls to how they should appear at launch
        {
            ResetFilter();
            ResetDataGrid();
            ResetDirectory();
        }

        // To avoid repeating the same messagebox alert and procedure
        private void CommonError()
        {
            ShowMsg("Could not access the template specified. Refreshing List...");
            ResetDataGrid();
        }

        // For simplifying the messagebox usage in this application
        private void ShowMsg(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK);
        }
    }
}
