
using System.ComponentModel;

namespace SSIS.Extensions
{
    public enum PGPFileAction
    {
        [Description("Encrypt")]
        Encrypt,

        [Description("Encrypt & Sign")]
        EncryptAndSign,

        [Description("Decrypt")]
        Decrypt
    }

    public enum SFTPFileAction
    {
        [Description("Send File(s)")]
        Send,

        [Description("Send Mutiple Files")]
        SendMultiple,

        [Description("Receive File(s)")]
        Receive,

        [Description("Receive Mutiple Files")]
        ReceiveMultiple,

        [Description("Remote File List")]
        List
    }

    public enum ZipFileAction
    {
        Compress,
        Decompress
    }

    public enum CompressionType
    {
        Zip,
        Tar
    }

    public enum ZipCompressionLevel
    {
        Ultra=9,
        Maximum=6,
        Normal=4,
        Minimum=2,
        None=0
    }

    public enum TarCompressionLevel
    {
        BZip2,
        GZip,
        None
    }

    public enum LogLevel
    {
        Verbose = 3,
        Minimal = 2,
        None = 1
    }
}
