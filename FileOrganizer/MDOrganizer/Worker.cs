using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace MDOrganizer
{
    class Entry
    {
        /// <summary>
        /// New file name of the md, Note that it is without extension
        /// </summary>
        public string NewFileName { get; private set; }
        
        /// <summary>
        /// Current file name of the md, Note that it is without extension
        /// </summary>
        public string CurrentFileName { get; private set; }

        /// <summary>
        /// Relative path where the new file would get stored
        /// </summary>
        public string RelativePath { get; private set;}

        public Entry(string fileName, string currentFileName, string relativePath) {
            this.NewFileName = fileName;
            this.CurrentFileName = currentFileName;
            this.RelativePath = relativePath;
        }
    }

    class Worker
    {
        private StreamReader csvReader = null;
        private string workerDir = null;
        private string targetDir = null;
        private string toc = null;
        private List<Entry> entryList = new List<Entry>();
        private StreamWriter mappedDataWriter = null;
        private StreamReader mappedDataReader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workerDir"></param>
        /// <param name="toc"></param>
        public Worker(string workerDir, string toc,string targetDir)
        {
            if (string.IsNullOrEmpty(targetDir))
            {
                Console.WriteLine("Error: target directory is empty, worker directory will be treated as targetdirectory");
                this.targetDir = this.workerDir;
            }
            else
            {
                this.targetDir = targetDir;
                Logger.Path = targetDir;
            }

            if (string.IsNullOrEmpty(workerDir))
            {
                Logger.Instance.LogError("Worker Directory can not be Null or Empty, Please provide the directory path where the mds file are present");
                return;
            }
            if (!Directory.Exists(workerDir))
            {
                Logger.Instance.LogError("Invalid Worker Directory Path.");
                return;
            }
            if (string.IsNullOrEmpty(toc) || !File.Exists(toc))
            {
                Logger.Instance.LogError("toc file is either NULL or Empty or doesn't exists");
                return;
            }
            this.workerDir = workerDir;
            this.toc = toc;
        }

        public void Start()
        {
            this.ParseTOC();
            this.FindAndReplace();
            //this.MoveFilesAround();
        }
        private void ParseTOC()
        {
            using (StreamReader csvReader = new StreamReader(this.toc)) {
                string line = null;
                while((line = csvReader.ReadLine())!= null) {
                    Logger.Instance.LogMessage("Parsing Line : " + line);
                    
                    //skip the heading
                    if (line.StartsWith("new name"))
                        continue;

                    //split in the tokens
                    string[] tokens = line.Split(',');
                    if (tokens.Length != 3)
                    {
                        Logger.Instance.LogError("Number of tokens is not equal to 3 in : " + line);
                        continue;
                    }

                    string relPath = tokens[2];
                    if (relPath.StartsWith(@"/") || relPath.StartsWith(@"\"))
                    {
                        relPath = relPath.Remove(0, 1);
                    }

                    if (tokens[1].StartsWith(@"/") || tokens[1].StartsWith(@"\"))
                    {
                        tokens[1] = tokens[1].Remove(0, 1);
                    }

                    Entry mddata = new Entry(tokens[1], tokens[0], relPath);
                    this.entryList.Add(mddata);
                }
                Logger.Instance.LogMessage("Parsing Completed");
            }
        }

        private void MoveFilesAround()
        {
            foreach (Entry e in this.entryList)
            {
                #region Creating source and dest dir
                string relPath = e.RelativePath;
                string destDir = this.targetDir;
                if (relPath != null)
                {
                    destDir = Path.Combine(destDir, relPath);
                    Directory.CreateDirectory(destDir);
                }
                string source = Path.Combine(this.workerDir, e.CurrentFileName + ".md");
                string dest = Path.Combine(destDir, e.NewFileName) + ".md";
                #endregion

                #region  Moving the file 
                Logger.Instance.LogMessage("Moving file " + source + "to " + dest);
                //copy th efile
                try
                {
                    if (File.Exists(source))
                    {
                        File.Copy(source, dest);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogError("An error has been encountered while moving file source: " + source + " to " + dest + "And the error is " + ex.Message);
                }
                #endregion
            }
        }

        private void FindAndReplace()
        {
            //foreach (Entry e in entryList)
            //{
                #region  Get the file path(whose stream need to be read

                string[] files = Directory.GetFiles(workerDir, "*.md", SearchOption.AllDirectories); //txtFolderPath.Text, "*ProfileHandler.cs", SearchOption.AllDirectories);
                //string filePath = Path.Combine(this.workerDir, e.CurrentFileName + ".md");

            foreach (string filePath in files) 
            { 
                if (!File.Exists(filePath))
                {
                    Logger.Instance.LogError("File : " + filePath + " doesn't exist");
                    continue;
                }
                Logger.Instance.LogMessage("Doing find replace in file : " + filePath);
                #endregion

                #region Read the file stream
                StreamReader reader = new StreamReader(filePath);
                string content = reader.ReadToEnd();
                reader.Close();
                #endregion

                #region Do find and replace for all entries in this opened stream
                foreach (Entry ee in this.entryList)
                {
                    // get the replace value it should be sth like dir1/dir2/filename
                    string replaceValue = null;
                    if (!string.IsNullOrEmpty(ee.RelativePath))
                    {
                        replaceValue = Path.Combine(ee.RelativePath, ee.NewFileName);
                    }
                    else
                    {
                        replaceValue = ee.NewFileName;
                    }

                    // find and replace
                    try
                    {
                       
                        replaceValue =  replaceValue.Replace(@"\", "/");

                        string currentFileName = ee.CurrentFileName.Replace(@"\", "/"); 

                        int count = filePath.Count(x => x == '\\'); //get ocurrances of / in the current file that we are in
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < count; i++)
                        {
                            sb.Append("../");
                        }


                        Logger.Instance.LogMessage("Finding and replacing the entry : " + ee.CurrentFileName + " with: " + sb.ToString() + replaceValue + ".md");

                        //only replace file names that end with .md.  Also, replace instances of \ with / in the new path for GFM
                        content = Regex.Replace(content, currentFileName, sb.ToString() + replaceValue + ".md", RegexOptions.None);

                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogError("Error while finding " + ee.CurrentFileName + "And replacing it with " + replaceValue + "in file " + filePath + "And error is " + ex.Message);
                    }
                }
                #endregion

                #region Write the content and close the stream
                StreamWriter writer = new StreamWriter(filePath);
                writer.Write(content);
                writer.Close();
                #endregion
            
            }
        }

        public void Finish()
        {
            Logger.Instance.Close();
        }
    }
}
