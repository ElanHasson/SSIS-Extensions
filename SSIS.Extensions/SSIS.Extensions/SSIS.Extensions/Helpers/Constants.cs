
namespace SSIS.Extensions
{
    /// <summary>
    /// Specifies constants values used to tie variables between the SSIS UI and runtime.
    /// </summary>
    public static class CONSTANTS
    {
        /* PGP */
        public const string PGPSOURCEFILE = "sourceFile";
        public const string PGPTARGETFILE = "targetFile";
        public const string PGPPUBLICKEY = "publicKey";
        public const string PGPPRIVATEKEY = "privateKey";
        public const string PGPPASSPHRASE = "passPhrase";
        public const string PGPFILEACTION = "fileAction";
        public const string PGPREMOVESOURCE = "removeSource";
        public const string PGPOVERWRITETARGET = "overwriteTarget";
        public const string PGPCOMPONENTEVENTS = "componentEvents";
        public const string PGPARMORED = "isArmored";
        //public const string PGPLOGLEVEL = "logLevel";

        /* SFTP */
        public const string SFTPLOCALFILE = "localFile";
        public const string SFTPREMOTEFILE = "remoteFile";
        public const string SFTPFILEACTION = "fileAction";
        public const string SFTPFILEINFO = "fileInfo";
        public const string SFTPOVERWRITEDEST = "overwriteDest";
        public const string SFTPREMOVESOURCE = "removeSource";
        public const string SFTPISRECURSIVE = "isRecursive";
        public const string SFTPFILEFILTER = "fileFilter";
        //public const string SFTPRETRIES = "reTries";
        public const string SFTPHOST = "hostName";
        public const string SFTPPORT = "portNumber";
        public const string SFTPUSER = "userName";
        public const string SFTPPASSWORD = "passWord";
        public const string SFTPSTOPONFAILURE = "stopOnFailure";
        public const string SFTPREMOTEFILELISTVAR = "remoteFileListVariable";
        public const string SFTPLOGLEVEL = "logLevel";

        /* Zip - Unzip*/
        public const string ZIPFILEACTION = "fileAction";
        public const string ZIPCOMPRESSIONTYPE = "compressionType";
        public const string ZIPCOMPRESSIONLEVEL = "zipCompressionLevel";
        public const string TARCOMPRESSIONLEVEL = "tarCompressionLevel";
        public const string ZIPPASSWORD = "zipPassword";
        public const string ZIPENCRYPTIONTYPE = "encryptionType";
        public const string ZIPSOURCE = "sourceFile";
        public const string ZIPREMOVESOURCE = "removeSource";
        public const string ZIPRECURSIVE = "recursive";
        public const string ZIPTARGET = "targetFile";
        public const string ZIPOVERWRITE = "overwriteTarget";
        public const string ZIPFILEFILTER = "fileFilter";
        public const string ZIPLOGLEVEL = "logLevel";
    }
}
