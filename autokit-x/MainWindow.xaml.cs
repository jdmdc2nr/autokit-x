using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace autokit_x
{


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Subscribe to the Exit event
            Application.Current.Exit += App_Exit;


        }


        private void App_Exit(object sender, ExitEventArgs e)
        {
            // Delete the folder on application exit
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "autokit-x");
            if (Directory.Exists(folderPath))
            {
                try
                {
                    Directory.Delete(folderPath, true);
                }
                catch (Exception ex)
                {
                    // Handle deletion error if necessary
                    Console.WriteLine($"Error deleting folder: {ex.Message}");
                }
            }
        }





        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Filter files with the .ACD extension
                var acdFiles = files.Where(file => Path.GetExtension(file)?.Equals(".ACD", StringComparison.OrdinalIgnoreCase) == true);

                if (acdFiles.Count() > 0)
                {
                    string filePath = acdFiles.First();

                    // Update the filename TextBlock
                    filenameTextBlock.Text = $"{Path.GetFileName(filePath)}";

                    // Check if the file exists
                    if (File.Exists(filePath))
                    {
                        SearchForV(filePath);
                    }
                }
                else
                {
                    // Filter files with the .MER extension
                    var merFiles = files.Where(file => Path.GetExtension(file)?.Equals(".MER", StringComparison.OrdinalIgnoreCase) == true);

                    if (merFiles.Count() > 0)
                    {
                        string filePath = merFiles.First();
                        // Update the filename TextBlock
                        filenameTextBlock.Text = $"{Path.GetFileName(filePath)}";

                        // Check if the file exists
                        if (File.Exists(filePath))
                        {
                            UnzipMerFile(filePath);
                        }
                    }
                    else
                    {
                        resultTextBlock.Text = "Please drop a file with the .ACD or .MER extension.";
                    }
                }
            }
        }

        private void DeleteFolder(string folderPath)
        {
            try
            {
                Directory.Delete(folderPath, true);
            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error deleting folder: {ex.Message}";
            }
        }

        private void UnzipMerFile(string filePath)
        {
            try
            {
                // Specify the destination folder
                string destinationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "autokit-x");

                // Check if the destination folder exists
                if (Directory.Exists(destinationFolder))
                {
                    // Delete the destination folder
                    Directory.Delete(destinationFolder, true);
                }

                // Create the destination folder
                Directory.CreateDirectory(destinationFolder);

                // Construct the arguments for 7-Zip command line
                string arguments = $"x \"{filePath}\" -o\"{destinationFolder}\"";

                // Set up process start information
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\7-zip\7z.exe", // Path to 7-Zip executable
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                // Start the 7-Zip process
                using (Process process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        resultTextBlock.Text = "File extracted successfully.";

                        // Read the last 2 bytes of VERSION_INFORMATION file
                        ReadSecondByteAndDisplay(destinationFolder);

                        // Delete the extracted folder
                        Directory.Delete(destinationFolder, true);
                    }
                    else
                    {
                        resultTextBlock.Text = "Extraction failed.";
                    }
                }
            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        private void SearchForV(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        int character;
                        while ((character = streamReader.Read()) != -1)
                        {
                            // Check for 'V'
                            if (character == 'V')
                            {
                                // Read the next two characters
                                char[] buffer = new char[2];
                                int bytesRead = streamReader.Read(buffer, 0, 2);
                                if (bytesRead == 2)
                                {
                                    string result = new string(buffer);
                                    resultTextBlock.Text = $"{result}";
                                    return;
                                }
                            }
                        }
                        resultTextBlock.Text = "Version not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        private void ReadSecondByteAndDisplay(string folderPath)
        {
            try
            {
                string versionInfoFilePath = Path.Combine(folderPath, "VERSION_INFORMATION");

                // Check if the file exists
                if (File.Exists(versionInfoFilePath))
                {
                    // Read the last 4 bytes of the file
                    byte[] buffer = new byte[3];
                    using (FileStream fs = new FileStream(versionInfoFilePath, FileMode.Open, FileAccess.Read))
                    {
                        fs.Seek(-3, SeekOrigin.End);
                        fs.Read(buffer, 0, 3);
                    }

                    // Display the value in the resultTextBlock
                    resultTextBlock.Text = $" {buffer[1]}.{buffer[2]}";

                    // Convert bytes to int8
                    sbyte value = (sbyte)buffer[1];

                }
                else
                {
                    resultTextBlock.Text = "VERSION_INFORMATION file not found.";
                }



            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error: {ex.Message}";
            }
        }









    }
}




