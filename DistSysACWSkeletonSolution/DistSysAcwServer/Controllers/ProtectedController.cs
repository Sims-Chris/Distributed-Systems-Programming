using DistSysAcwServer.Models;
using DistSysAcwServer.Pipeline.Auth;
using DistSysAcwServer.Shared; // Ensure you include this namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace DistSysAcwServer.Controllers
{
    [Authorize(Roles = "Admin, User")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProtectedController : BaseController
    {
        public ProtectedController(Models.UserContext dbcontext, SharedError error)
            : base(dbcontext, error) { }

        [HttpGet("Hello")]
        public IActionResult Hello()
        {
            string username = User.Identity.Name;
            UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Protected/Hello");
            return Ok($"Hello {username}");
        }

        [HttpGet("SHA1")]
        public IActionResult Sha1([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Bad Request");
            }

            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] sourceBytes = Encoding.ASCII.GetBytes(message);
                byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");
                UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Protected/SHA1");
                return Ok(hash);
            }
        }

        [HttpGet("SHA256")]
        public IActionResult Sha256([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Bad Request");
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sourceBytes = Encoding.ASCII.GetBytes(message);
                byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");
                UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Protected/SHA256");
                return Ok(hash);
            }
        }

        [HttpGet("GetPublicKey")]
        public IActionResult GetPublicKey()
        {
            using (RSA rsa = RSA.Create())
            {
                string publicKeyXml = RsaKeyService.GetPublicKeyXml();
                UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Protected/GetPublicKey");
                return Ok(publicKeyXml);
            }
        }

        [HttpGet("Sign")]
        public IActionResult Sign([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Bad Request");
            }

            try
            {
                // Get the central RSA provider (which contains the private key)
                RSACryptoServiceProvider rsa = RsaKeyService.GetProvider();

                byte[] dataToSign = Encoding.ASCII.GetBytes(message);
                byte[] signature = rsa.SignData(dataToSign, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

                string hexSignature = BitConverter.ToString(signature);
                UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Protected/Sign");
                return Ok(hexSignature);
            }
            catch (Exception)
            {
                return BadRequest("Bad Request");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Mashify")]
        public IActionResult Mashify([FromBody] MashifyRequest request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.EncryptedString) ||
                string.IsNullOrEmpty(request.EncryptedSymKey) ||
                string.IsNullOrEmpty(request.EncryptedIV))
            {
                return BadRequest("Bad Request");
            }

            try
            {
                // Get the RSA provider for decryption
                RSACryptoServiceProvider rsa = RsaKeyService.GetProvider();

                // Helper to convert hex strings with dashes back to byte arrays
                byte[] encryptedStringBytes = HexStringToByteArray(request.EncryptedString);
                byte[] encryptedSymKeyBytes = HexStringToByteArray(request.EncryptedSymKey);
                byte[] encryptedIVBytes = HexStringToByteArray(request.EncryptedIV);

                // Decrypt all three parameters using RSA with OaepSHA1 padding
                byte[] decryptedStringBytes = rsa.Decrypt(encryptedStringBytes, RSAEncryptionPadding.OaepSHA1);
                byte[] decryptedSymKey = rsa.Decrypt(encryptedSymKeyBytes, RSAEncryptionPadding.OaepSHA1);
                byte[] decryptedIV = rsa.Decrypt(encryptedIVBytes, RSAEncryptionPadding.OaepSHA1);

                string originalString = Encoding.ASCII.GetString(decryptedStringBytes);

                // Mashify the string: Vowels to 'X' and Reverse
                string mashifiedString = MashifyLogic(originalString);

                // Encrypt the mashified string using the client's symmetric (AES) key and IV
                byte[] mashifiedBytes = Encoding.ASCII.GetBytes(mashifiedString);
                byte[] encryptedMashifiedBytes;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = decryptedSymKey;
                    aes.IV = decryptedIV;
                    aes.Mode = CipherMode.CBC; // Standard AES mode
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        encryptedMashifiedBytes = encryptor.TransformFinalBlock(mashifiedBytes, 0, mashifiedBytes.Length);
                    }
                }

                // Log the activity (without compromising encrypted data)
                UserProvider.LogActivity(Request.Headers["ApiKey"], "User requested /api/Protected/Mashify");

                // Return the newly encrypted string as a hex string with dashes
                return Ok(BitConverter.ToString(encryptedMashifiedBytes));
            }
            catch (Exception)
            {
                return BadRequest("Bad Request"); // Requirement: 400 if error occurs
            }
        }

        // Helper method for the "Mashify" logic
        private string MashifyLogic(string input)
        {
            char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };

            // Convert all vowels to 'X'
            char[] chars = input.Select(c => vowels.Contains(c) ? 'X' : c).ToArray();

            // Reverse the string
            Array.Reverse(chars);

            return new string(chars);
        }

        // Helper to handle the hex format with dashes
        private byte[] HexStringToByteArray(string hex)
        {
            return hex.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
        }
    }
}
    