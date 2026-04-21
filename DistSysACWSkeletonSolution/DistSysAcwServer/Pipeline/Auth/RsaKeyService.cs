namespace DistSysAcwServer.Pipeline.Auth
{
    using System.Security.Cryptography;

    public static class RsaKeyService
    {
        private static readonly RSACryptoServiceProvider _rsa;

        static RsaKeyService()
        {
#pragma warning disable CA1416
            var cspParams = new CspParameters
            {
                KeyContainerName = "ServerSpecific_RSA_Key_Unique",
                Flags = CspProviderFlags.UseMachineKeyStore
            };

            // 1. Force delete any existing key in this container first
            using (var cleaner = new RSACryptoServiceProvider(cspParams))
            {
                cleaner.PersistKeyInCsp = false;
                cleaner.Clear(); // This wipes the container from the OS
            }

            // 2. Create the actual instance that threads will use
            // This will generate a brand new key because we just deleted the old one
            _rsa = new RSACryptoServiceProvider(2048, cspParams)
            {
                PersistKeyInCsp = true
            };
#pragma warning restore CA1416
        }

        public static string GetPublicKeyXml()
        {
            // This will no longer throw because _rsa is never cleared after initialization
            return _rsa.ToXmlString(false);
        }

        public static RSACryptoServiceProvider GetProvider() => _rsa;
    }
}