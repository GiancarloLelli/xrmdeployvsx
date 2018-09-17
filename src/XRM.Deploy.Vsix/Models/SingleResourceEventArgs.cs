using System;

namespace XRM.Deploy.Vsix.Models
{
    internal class SingleResourceEventArgs : EventArgs
    {
        public string File { get; set; }

        public SingleResourceEventArgs(string file)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            File = file;
        }
    }
}
