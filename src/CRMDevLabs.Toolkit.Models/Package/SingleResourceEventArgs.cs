using System;

namespace CRMDevLabs.Toolkit.Models.Package
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
