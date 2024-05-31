
using System.Runtime.Serialization;
namespace WcfServiceBook
{
    [DataContract]
    public class FileTransferRequest
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public byte[] Content { get; set; }
    }
}
