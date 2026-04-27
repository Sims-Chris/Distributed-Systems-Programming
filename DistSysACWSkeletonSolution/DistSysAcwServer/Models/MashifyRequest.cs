// Models/MashifyRequest.cs
namespace DistSysAcwServer.Models
{
    public class MashifyRequest
    {
        public string EncryptedString { get; set; }
        public string EncryptedSymKey { get; set; }
        public string EncryptedIV { get; set; }
    }
}