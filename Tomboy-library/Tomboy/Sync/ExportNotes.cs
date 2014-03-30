//
// ExportNotes.cs
//
// Author:
//       Rashid Khan <rashood.khan@gmail.com>
//
// Copyright (c) 2014 Rashid Khan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;

namespace Tomboy
{
    public class ExportNotes
    {
        public ExportNotes()
        {

        }

        public static void Export(string rootDirectory)
        {
            ProcessDirectory(rootDirectory);
        }

        private static void ProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = System.IO.Directory.GetFiles(targetDirectory);

            foreach (string filename in fileEntries)
                ProcessFile(filename);

            string [] subdirectoryEntries = System.IO.Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        private static void ProcessFile(string path) 
        {
            var storage_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy");
            var file_name = Path.GetFileName(path);
            var dest_file = Path.Combine(storage_path, file_name);
            File.Copy(path, dest_file, true);
            Console.WriteLine("Copied File '{0}'.", path);       
        }
    }
}

