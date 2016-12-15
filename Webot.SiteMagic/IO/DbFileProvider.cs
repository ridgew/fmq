using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Configuration;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Web.Util;
using System.Web.Hosting;
//http://scottgu.com/talks/whidbey/tipstricks.zip

namespace Webot.SiteMagic.IO
{
    public class FileRecordData
    {
        public int ID;
        public int ParentID;
        public bool IsDirectory;
        public string Name;
        public string Content;
        public DateTime LastChanged;
    }

    internal class DbAccessLayer
    {
        static OleDbConnection _myConn;
        static OleDbCommand _getFileInfoCmd;
        static OleDbParameter _getFileInfoParam;
        static OleDbCommand _getChildrenCmd;
        static OleDbParameter _getChildrenParam;
        static OleDbCommand _getChildCmd;
        static OleDbParameter _getChildParamParentID;
        static OleDbParameter _getChildParamChildName;
        static int _rootID;

        internal DbAccessLayer()
        {
            // Create the various database objects
            _myConn = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0;Data Source=|DataDirectory|FileSystem.mdb;Persist Security Info=True;");
            _myConn.Open();

            _getFileInfoCmd = new OleDbCommand("select * from FileSystem where ID=@ID", _myConn);
            _getFileInfoParam = _getFileInfoCmd.Parameters.Add("@ID", OleDbType.Integer);

            _getChildrenCmd = new OleDbCommand("select * from FileSystem where ParentID=@ParentID", _myConn);
            _getChildrenParam = _getChildrenCmd.Parameters.Add("@ParentID", OleDbType.Integer);

            _getChildCmd = new OleDbCommand("select * from FileSystem where ParentID=@ParentID and Name=@Name", _myConn);
            _getChildParamParentID = _getChildCmd.Parameters.Add("@ParentID", OleDbType.Integer);
            _getChildParamChildName = _getChildCmd.Parameters.Add("@Name", OleDbType.BSTR);

            _rootID = GetRootID();
        }

        internal FileRecordData GetFileRecord(int ID)
        {
            lock (_myConn)
            {

                _getFileInfoParam.Value = ID;

                using (OleDbDataReader reader = _getFileInfoCmd.ExecuteReader())
                {
                    reader.Read();
                    return GetRecordData(reader);
                }
            }
        }

        private FileRecordData GetChildFileRecord(int ParentID, string name)
        {
            lock (_myConn)
            {
                _getChildParamParentID.Value = ParentID;
                _getChildParamChildName.Value = name;

                using (OleDbDataReader reader = _getChildCmd.ExecuteReader())
                {

                    // Child doesn't exist
                    if (!reader.HasRows)
                        return null;

                    reader.Read();
                    return GetRecordData(reader);
                }
            }
        }

        internal OleDbDataReader GetChildrenRecords(int parentID)
        {
            lock (_myConn)
            {
                _getChildrenParam.Value = parentID;
                return _getChildrenCmd.ExecuteReader();
            }
        }

        internal FileRecordData GetFile(string virtualPath)
        {

            int startIndex = virtualPath.IndexOf('/', 1);
            virtualPath = virtualPath.Substring(startIndex + 1);

            string[] parts = virtualPath.Split('/');

            int currentID = _rootID;

            FileRecordData frd = null;

            foreach (string part in parts)
            {

                frd = GetChildFileRecord(currentID, part);

                if (frd == null)
                    return null;

                currentID = frd.ID;
            }

            return frd;
        }

        internal FileRecordData GetRecordData(IDataRecord record)
        {
            FileRecordData frd = new FileRecordData();
            frd.ID = (int)record["ID"];
            frd.ParentID = (int)record["ParentID"];
            frd.IsDirectory = (bool)record["IsDirectory"];
            frd.Name = record["Name"] as String;
            frd.Content = record["Content"] as String;
            frd.LastChanged = (DateTime)record["LastChanged"];

            return frd;
        }

        private int GetRootID()
        {
            IDataReader reader = GetChildrenRecords(0);

            try
            {
                reader.Read();
                return (int)reader["ID"];
            }
            finally
            {
                reader.Close();
            }
        }

    }

    public class DbFileProvider : VirtualPathProvider
    {

        private DbAccessLayer _db;

        public DbFileProvider()
        {
            _db = new DbAccessLayer();
        }

        // Using weird casing to test case insensitivity
        public static void AppInitialize()
        {
            DbFileProvider fileProvider = new DbFileProvider();
            HostingEnvironment.RegisterVirtualPathProvider(fileProvider);
        }


        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();

            FileRecordData frd = _db.GetFile(virtualPath);

            if (frd != null)
            {
                hashCodeCombiner.AddObject(frd.LastChanged.Ticks);
            }

            return hashCodeCombiner.CombinedHashString;
        }

        /*
        public override string GetFileHash(IEnumerable virtualPaths, string cacheHash) {

            HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();

            foreach (string virtualPath in virtualPaths) {
                FileRecordData frd = _db.GetFile(virtualPath);
                
                if (frd != null) {
                    hashCodeCombiner.AddObject(frd.LastChanged.Ticks);
                }
            }

            return hashCodeCombiner.CombinedHashString;
        }
        */

        public override bool FileExists(string virtualPath)
        {

            // Check if it's a database file
            FileRecordData frd = _db.GetFile(virtualPath);
            if (frd != null)
            {
                return !frd.IsDirectory;
            }

            return false;
            //return Previous.FileExists(virtualPath);
        }

        public override bool DirectoryExists(string virtualDir)
        {

            // Check if it's a database directory
            FileRecordData frd = _db.GetFile(virtualDir);
            if (frd != null)
            {
                return frd.IsDirectory;
            }

            return false;
        }

        public override VirtualFile GetFile(string virtualPath)
        {

            FileRecordData frd = _db.GetFile(virtualPath);

            if (frd != null)
            {
                if (frd.IsDirectory)
                    return null;

                return new DbVirtualFile(virtualPath, frd);
            }

            return null;
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {

            FileRecordData frd = _db.GetFile(virtualDir);

            if (frd != null)
            {
                if (!frd.IsDirectory)
                    return null;

                return new DbVirtualDirectory(_db, virtualDir, frd);
            }

            return Previous.GetDirectory(virtualDir);
        }
    }

    internal class DbVirtualFile : VirtualFile
    {

        private FileRecordData _frd;

        public DbVirtualFile(string virtualPath, FileRecordData frd) : base(virtualPath)
        {
            _frd = frd;
        }

        public override Stream Open()
        {
            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.Unicode);

            writer.Write(_frd.Content);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }

    internal class DbVirtualDirectory : VirtualDirectory
    {

        private DbAccessLayer _db;
        internal DbAccessLayer Db { get { return _db; } }
        private FileRecordData _frd;
        internal FileRecordData Frd { get { return _frd; } }

        public DbVirtualDirectory(DbAccessLayer db, string virtualPath, FileRecordData frd) : base(virtualPath)
        {
            _db = db;
            _frd = frd;
        }

        public override IEnumerable Directories
        {
            get
            {
                return new DbVirtualPathCollection(
                    this, RequestedEntryType.Directories);
            }
        }

        public override IEnumerable Files
        {
            get
            {
                return new DbVirtualPathCollection(
                    this, RequestedEntryType.Files);
            }
        }

        public override IEnumerable Children
        {
            get
            {
                return new DbVirtualPathCollection(
                    this, RequestedEntryType.All);
            }
        }
    }


    internal enum RequestedEntryType
    {
        Files,
        Directories,
        All
    }

    internal class DbVirtualPathCollection : IEnumerable
    {

        private DbVirtualDirectory _dbVirtualDir;
        private RequestedEntryType _requestedEntryType;

        internal DbVirtualPathCollection(DbVirtualDirectory dbVirtualDir, RequestedEntryType requestedEntryType)
        {
            _dbVirtualDir = dbVirtualDir;
            _requestedEntryType = requestedEntryType;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DbVirtualPathEnumerator(_dbVirtualDir, _requestedEntryType);
        }
    }

    internal class DbVirtualPathEnumerator : IEnumerator, IDisposable
    {

        private DbVirtualDirectory _dbVirtualDir;
        private RequestedEntryType _requestedEntryType;
        private IDataReader _dataReader;
        private FileRecordData _childRecord;

        internal DbVirtualPathEnumerator(DbVirtualDirectory dbVirtualDir, RequestedEntryType requestedEntryType)
        {
            _dbVirtualDir = dbVirtualDir;
            _requestedEntryType = requestedEntryType;
            _dataReader = _dbVirtualDir.Db.GetChildrenRecords(_dbVirtualDir.Frd.ID);
        }

        // Dispose the file enumerator
        void IDisposable.Dispose()
        {
            if (_dataReader != null)
            {
                _dataReader.Dispose();
                _dataReader = null;
            }
        }

        bool IEnumerator.MoveNext()
        {

            for (; ; )
            {

                // No more data: we're done
                if (!_dataReader.Read())
                    return false;

                _childRecord = _dbVirtualDir.Db.GetRecordData(_dataReader);

                // If it's the wrong entry type, ignore it
                if (_childRecord.IsDirectory && _requestedEntryType == RequestedEntryType.Files ||
                    !_childRecord.IsDirectory && _requestedEntryType == RequestedEntryType.Directories)
                {
                    continue;
                }

                // We found a usable child
                return true;
            }
        }

        internal VirtualFileBase Current
        {
            get
            {
                string childVirtualPath = _dbVirtualDir.VirtualPath + "/" + _childRecord.Name;

                return new DbVirtualFile(childVirtualPath, _childRecord);
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        void IEnumerator.Reset()
        {
            // We don't support reset, though it would be easy to add if needed
            throw new InvalidOperationException();
        }
    }

    /*
 * Class used to combine several hashcodes into a single hashcode
 */
    internal class HashCodeCombiner
    {

        // Start with a seed (obtained from JRoxe's implementation of String.GetHashCode)
        private long _combinedHash = 5381;

        internal HashCodeCombiner()
        {
        }

        internal HashCodeCombiner(HashCodeCombiner hcc)
        {
            HashCodeCombiner ret = new HashCodeCombiner();
            ret._combinedHash = hcc._combinedHash;
        }

        internal void AddInt(int n)
        {
            _combinedHash = ((_combinedHash << 5) + _combinedHash) ^ n;
        }

        internal void AddObject(int n)
        {
            AddInt(n);
        }

        internal void AddObject(long l)
        {
            AddInt(l.GetHashCode());
        }

        internal void AddObject(bool b)
        {
            AddInt(b.GetHashCode());
        }

        internal void AddObject(string s)
        {
            if (s != null)
                AddInt(s.GetHashCode());
        }

        internal void AddObject(object o)
        {
            if (o != null)
                AddInt(o.GetHashCode());
        }

        internal void AddCaseInsensitiveString(string s)
        {
            if (s != null)
                AddInt( StringComparer.InvariantCultureIgnoreCase.GetHashCode(s));
        }

        internal void AddDateTime(DateTime dt)
        {
            AddInt(dt.GetHashCode());
        }

        private void AddFileSize(long fileSize)
        {
            AddInt(fileSize.GetHashCode());
        }

        internal void AddFile(string fileName)
        {
            AddInt(fileName.GetHashCode());
            FileInfo file = new FileInfo(fileName);
            AddDateTime(file.CreationTimeUtc);
            AddDateTime(file.LastWriteTimeUtc);
            AddFileSize(file.Length);
        }

        internal long CombinedHash { get { return _combinedHash; } }
        internal int CombinedHash32 { get { return _combinedHash.GetHashCode(); } }

        internal string CombinedHashString
        {
            get
            {
                return _combinedHash.ToString("x", CultureInfo.InvariantCulture);
            }
        }
    }

}