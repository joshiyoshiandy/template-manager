using System.IO;

namespace TemplateManager
{
    class FileTemplate
    {
        // Fields
        private string name;
        private string folder;
        private string created;
        private string filePath;
        private string extension;
        private string directory;
        private string modifiedLast;
        private string filterByName;

        // Properties
        public string Name
        {
            get { return name; }
            set { name = Name; }
        }
        public string Folder
        {
            get { return folder; }
            set { folder = Folder; }
        }
        public string Created
        {
            get { return created; }
            set { created = Created; }
        }
        public string FilePath
        {
            get { return filePath; }
            set { filePath = FilePath; }
        }
        public string Extension
        {
            get { return extension; }
            set { extension = Extension; }
        }
        public string Directory
        {
            get { return directory; }
            set { directory = Directory; }
        }
        public string ModifiedLast
        {
            get { return modifiedLast; }
            set { modifiedLast = ModifiedLast; }
        }

        public string FilterByName
        {
            get { return filterByName; }
            set { filterByName = FilterByName; }
        }

        public FileTemplate(string path) // - Constructor
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                name = fileInfo.Name;
                filePath = fileInfo.FullName;
                extension = fileInfo.Extension;
                directory = fileInfo.DirectoryName;
                filterByName = name.ToLower();

                // Determine the parent folder of the file
                string[] s = filePath.Split('\\');
                folder = s[s.Length - 2];

                // Convert to a string format for use purposes
                created = fileInfo.CreationTime.ToString("dd/MM/yy");
                modifiedLast = File.GetLastWriteTime(path).ToString("dd/MM/yy");
            }
        }

    }
}
