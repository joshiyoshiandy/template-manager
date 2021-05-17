//: Template Manager

/* 
  Title: NewTemplateWindow.xaml.cs
  Date: January 6, 2019
  Author: Andy Joshi
  Version: 2.0
  Copyright: 2018 Andy Joshi
*/

// System namespaces
using System.IO;
using System.Windows;
using System.Diagnostics;

// External dll namespaces (OpenXML)
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;

// Other namespaces
using Microsoft.Win32;

namespace TemplateManager
{
    public partial class NewTemplateWindow : Window
    {
        // MASTER_PATH is needed here too but it is received from the MainWindow class
        // This is done so that MASTER_PATH is only required to be changed in one location of the application
        private readonly string MASTER_PATH = "";

        public NewTemplateWindow(string masterPath) // - Handles startup procedure for when the NewTemplate window is launched
        {
            InitializeComponent();
            ChkMSExcel.IsChecked = true; // Check the excel option by default
            MASTER_PATH = masterPath; // Set the MASTER_PATH for use outside this scope
        }

        // Event for when the Save button is clicked
        private void BtnSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            string extension = ".xlsx";
            if (ChkMSWord.IsChecked == true) { extension = ".docx"; }

            // Open a save dialog window that shows (filters) files of the same extension as the one desired
            // Direct the user to the main path because templates can only be saved in there
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "(*" + extension + "*)|*" + extension + "*",
                FilterIndex = 2,
                InitialDirectory = MASTER_PATH + '\\',
                RestoreDirectory = true
            };

            if (sfd.ShowDialog() == true)
            {
                // Control to ensure that new templates are saved in the main Templates folder
                // Check to see if the user's save path contains the main path
                if (!Path.GetFullPath(sfd.FileName).Contains(MASTER_PATH))
                {
                    // The user is trying to save the file outside of the main directory
                    MessageBox.Show("You have selected a location outside of the intended directory." +
                                    "\nPlease select a location within the " + MASTER_PATH + " directory.",
                                    "Error", MessageBoxButton.OK);
                }
                else
                {
                    string savePath = sfd.FileName; // Get the user-specified file path

                    // Break up the user-specified file path using the '.' character for file extension analysis
                    string[] s = sfd.FileName.Split('.');

                    if ("." + s[s.Length - 1] != extension) // Check if the user's file name already contains the appropriate file extension
                    {
                        savePath += extension; // User did not include a file extension in their name so add the correct one
                    }
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath); // User may be trying overwrite an existing file so delete any file with that name if it exists
                    }

                    // Decide which type of doc to create depending on radio button selection
                    if (ChkMSWord.IsChecked == true) { CreateWordDocument(savePath); }
                    else { CreateExcelDocument(savePath); }

                    // Launch the file created
                    Process.Start(savePath);

                    // Close the form
                    Close();
                }
            }
        }

        private void CreateExcelDocument(string path) // - Creates and saves a blank excel workbook to a specified path (standard procedure)
        {
            // By default, AutoSave = true, Editable = true, and Type = xlsx
            SpreadsheetDocument excelDoc = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook); // Initialize a spreadsheet document

            // Add a workbook part
            WorkbookPart workbookpart = excelDoc.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a worksheet part
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheets sheets = excelDoc.WorkbookPart.Workbook.AppendChild(new Sheets()); // Add new sheets to workbook

            // Append a new sheet to sheets
            Sheet sheet = new Sheet()
            {
                Id = excelDoc.WorkbookPart.
                GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            // Save and close the workbook
            workbookpart.Workbook.Save();
            excelDoc.Close();
        }

        private void CreateWordDocument(string path) // - Creates and saves a blank word document to a specified path (standard procedure)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document)) // Initialize a word document
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart(); // Add a main part

                // Create the doc structure
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Save and close the word document
                wordDoc.Save();
                wordDoc.Close();
            }
        }
    }
}