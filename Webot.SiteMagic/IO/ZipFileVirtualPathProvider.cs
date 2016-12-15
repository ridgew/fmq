using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Hosting;

using ICSharpCode.SharpZipLib.Zip;
using System.Collections;
using System.IO;

namespace Webot.SiteMagic.IO
{
    public class ZipFileVirtualPathProvider : System.Web.Hosting.VirtualPathProvider
    {
        ZipFile _zipFile;

        public ZipFileVirtualPathProvider(string zipFilename)
            : base()
        {
            _zipFile = new ZipFile(zipFilename);
        }

        ~ZipFileVirtualPathProvider()
        {
            _zipFile.Close();
        }

        public override bool FileExists(string virtualPath)
        {
            string zipPath = VPPUtil.ConvertVirtualPathToZipPath(virtualPath, true);
            ZipEntry zipEntry = _zipFile.GetEntry(zipPath);

            if (zipEntry != null)
            {
                return !zipEntry.IsDirectory;
            }
            else
            {
                // Here you may want to return Previous.FileExists(virtualPath) instead of false
                // if you want to give the previously registered provider a process to serve the file
                return false;
            }
        }

        public override bool DirectoryExists(string virtualDir)
        {
            string zipPath = VPPUtil.ConvertVirtualPathToZipPath(virtualDir, false);
            ZipEntry zipEntry = _zipFile.GetEntry(zipPath);

            if (zipEntry != null)
            {
                return zipEntry.IsDirectory;
            }
            else
            {
                // Here you may want to return Previous.DirectoryExists(virtualDir) instead of false
                // if you want to give the previously registered provider a chance to process the directory
                return false;
            }
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return new ZipVirtualFile(virtualPath, _zipFile);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            return new ZipVirtualDirectory(virtualDir, _zipFile);
        }

        public override string GetFileHash(string virtualPath, System.Collections.IEnumerable virtualPathDependencies)
        {
            return null;
        }

        public override System.Web.Caching.CacheDependency GetCacheDependency(String virtualPath, System.Collections.IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return null;
        }
    }

    internal static class VPPUtil
    {
        internal static string ConvertVirtualPathToZipPath(String virtualPath, bool isFile)
        {
            if (virtualPath[0] == '/')
            {
                if (!isFile)
                    return virtualPath.Substring(1) + "/";
                else
                    return virtualPath.Substring(1);
            }
            else
                return virtualPath;
        }

        internal static string ConvertZipPathToVirtualPath(String zipPath)
        {
            return "/" + zipPath;
        }
    }

    internal enum VirtualPathType
    {
        Files, Directories, All
    }

    class ZipVirtualDirectory : VirtualDirectory
    {
        ZipFile _zipFile;

        public ZipVirtualDirectory(String virtualDir, ZipFile file)
            : base(virtualDir)
        {
            _zipFile = file;
        }

        public override System.Collections.IEnumerable Children
        {
            get
            {
                return new ZipVirtualPathCollection(base.VirtualPath, VirtualPathType.All, _zipFile);
            }
        }

        public override System.Collections.IEnumerable Directories
        {
            get
            {
                return new ZipVirtualPathCollection(base.VirtualPath, VirtualPathType.Directories, _zipFile);
            }
        }

        public override System.Collections.IEnumerable Files
        {
            get
            {
                return new ZipVirtualPathCollection(base.VirtualPath, VirtualPathType.Files, _zipFile);
            }
        }
    }

    class ZipVirtualFile : VirtualFile
    {
        ZipFile _zipFile;

        public ZipVirtualFile(String virtualPath, ZipFile zipFile)
            : base(virtualPath)
        {
            _zipFile = zipFile;
        }

        public override System.IO.Stream Open()
        {
            ZipEntry entry = _zipFile.GetEntry(VPPUtil.ConvertVirtualPathToZipPath(base.VirtualPath, true));
            using (Stream st = _zipFile.GetInputStream(entry))
            {
                MemoryStream ms = new MemoryStream();
                ms.SetLength(entry.Size);
                byte[] buf = new byte[2048];
                while (true)
                {
                    int r = st.Read(buf, 0, 2048);
                    if (r == 0)
                        break;
                    ms.Write(buf, 0, r);
                }
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }
    }

    class ZipVirtualPathCollection : MarshalByRefObject, IEnumerable
    {
        ZipFile _zipFile;
        ArrayList _paths;
        String _virtualPath;
        VirtualPathType _requestType;

        public ZipVirtualPathCollection(String virtualPath, VirtualPathType requestType, ZipFile zipFile)
        {
            _paths = new ArrayList();
            _virtualPath = virtualPath;
            _requestType = requestType;
            _zipFile = zipFile;

            PerformEnumeration();
        }

        private void PerformEnumeration()
        {
            String zipPath = VPPUtil.ConvertVirtualPathToZipPath(_virtualPath, false);

            if (zipPath[zipPath.Length - 1] != '/')
            {
                ZipEntry entry = _zipFile.GetEntry(zipPath);
                if (entry != null)
                    _paths.Add(new ZipVirtualFile(zipPath, _zipFile));
                return;
            }
            else
            {
                foreach (ZipEntry entry in _zipFile)
                {
                    Console.WriteLine(entry.Name);
                    if (entry.Name == zipPath)
                        continue;
                    if (entry.Name.StartsWith(zipPath))
                    {
                        // if we're looking for files and current entry is a directory, skip it
                        if (_requestType == VirtualPathType.Files && entry.IsDirectory)
                            continue;
                        // if we're looking for directories and current entry its not one, skip it
                        if (_requestType == VirtualPathType.Directories && !entry.IsDirectory)
                            continue;

                        int pos = entry.Name.IndexOf('/', zipPath.Length);
                        if (pos != -1)
                        {
                            if (entry.Name.Length > pos + 1)
                                continue;
                        }
                        //    continue;
                        if (entry.IsDirectory)
                            _paths.Add(new ZipVirtualDirectory(VPPUtil.ConvertZipPathToVirtualPath(entry.Name), _zipFile));
                        else
                            _paths.Add(new ZipVirtualFile(VPPUtil.ConvertZipPathToVirtualPath(entry.Name), _zipFile));
                    }
                }
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _paths.GetEnumerator();
        }

        #endregion
    }
}
