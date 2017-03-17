namespace QlikConnect.Crypt
{
    #region Usings
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Crypto.Prng;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Security;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    #endregion

    public class CryptoManager
    {
        #region Constructor
        public CryptoManager()
        {
            Pair = GetKeyPair();
            PrivateKey = Pair.Private as RsaPrivateCrtKeyParameters;
            PublicKey = Pair.Public as RsaKeyParameters;
        }

        public CryptoManager(string private_key_path)
        {
            using (var reader = new StreamReader(private_key_path, Encoding.ASCII))
            {
                this.Init(reader);
            }
        }

        public CryptoManager(MemoryStream private_key_stream)
        {
            using (var reader = new StreamReader(private_key_stream, Encoding.ASCII))
            {
                this.Init(reader);
            }
        }

        private void Init(StreamReader reader)
        {
            var pemReader = new PemReader(reader);
            Pair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
            PrivateKey = Pair.Private as RsaPrivateCrtKeyParameters;
            PublicKey = Pair.Public as RsaKeyParameters;
        }
        #endregion

        #region Static Methods
        private static string FormatCode(string code)
        {
            code = code.Trim();
            code = code.Replace("\r\n", "\n");
            code = code.Replace("\t", "");
            code = code.Replace("\n\n", "\n");

            return code;
        }

        public static RsaKeyParameters ReadPublicKey(string public_key_path)
        {
            try
            {
                using (var reader = new StreamReader(public_key_path, Encoding.ASCII))
                {
                    var pemReader = new PemReader(reader);
                    return pemReader.ReadObject() as RsaKeyParameters;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"The public key {public_key_path} could not read.", ex);
            }
        }

        public static bool IsValidPublicKey(string data, string sign, RsaKeyParameters public_key)
        {
            try
            {
                data = FormatCode(data);

                var sig = Convert.FromBase64String(sign);
                ISigner signer = SignerUtilities.GetSigner("SHA1WithRSA");
                signer.Init(false, public_key);

                var msgBytes = Encoding.Default.GetBytes(data);
                signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
                return signer.VerifySignature(sig);
            }
            catch (Exception ex)
            {
                throw new Exception("The public key could not be properly verified.", ex);
            }
        }
        #endregion

        #region Methods
        private AsymmetricCipherKeyPair GetKeyPair()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var secureRandom = new SecureRandom(randomGenerator);
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom, 2048);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }

        public string SignWithPrivateKey(string data)
        {
            return SignWithPrivateKey(null, data);
        }

        public string SignWithPrivateKey(string prefix, string data)
        {
            data = FormatCode(data);

            var rsa = RSA.Create() as RSACryptoServiceProvider;
            var rsaParameters = DotNetUtilities.ToRSAParameters(PrivateKey);
            rsa.ImportParameters(rsaParameters);

            var sha = new SHA1CryptoServiceProvider();
            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));

            var sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));

            return prefix + Convert.ToBase64String(sig);
        }

        public bool IsValid(string data, string sign)
        {
            try
            {
                data = FormatCode(data);

                var sig = Convert.FromBase64String(sign);
                ISigner signer = SignerUtilities.GetSigner("SHA1WithRSA");
                signer.Init(false, PublicKey);

                var msgBytes = Encoding.Default.GetBytes(data);
                signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
                return signer.VerifySignature(sig);
            }
            catch (Exception ex)
            {
                throw new Exception("The public key could not be properly verified.", ex);
            }
        }

        private void SaveKey(string path, object key)
        {
            using (var writer = new StreamWriter(path, false, Encoding.ASCII))
            {
                var pemWriter = new PemWriter(writer);
                pemWriter.WriteObject(key);
                pemWriter.Writer.Flush();
            }
        }

        public void SavePrivateKey(string path)
        {
            SaveKey(path, PrivateKey);
        }

        public void SavePublicKey(string path)
        {
            SaveKey(path, PublicKey);
        }
        #endregion

        #region Variables & Properties
        private AsymmetricCipherKeyPair Pair { get; set; }
        public RsaPrivateCrtKeyParameters PrivateKey { get; private set; }
        public RsaKeyParameters PublicKey { get; private set; }
        #endregion
    }
}