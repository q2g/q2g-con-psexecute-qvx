namespace QlikConnectorPSExecute
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

        public bool IsValidPublicKey(string data, string sign, string algorithm = "SHA256withRSA")
        {
            try
            {
                var sig = Convert.FromBase64String(sign);
                ISigner signer = SignerUtilities.GetSigner(algorithm);
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

        public string SignWithPrivateKey(string data, bool write_algo_as_prefix=false,  bool use_indent=false, string algorithm="SHA256")
        {
            var rsa = RSA.Create() as RSACryptoServiceProvider;
            var rsaParameters = DotNetUtilities.ToRSAParameters(PrivateKey);
            rsa.ImportParameters(rsaParameters);

            var sha = new SHA256CryptoServiceProvider();
            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
            var id = CryptoConfig.MapNameToOID(algorithm);
            var sig = rsa.SignHash(hash, id);

            var prefix = String.Empty;
            if (write_algo_as_prefix)
                prefix = $"{algorithm}:\n";

            if (use_indent)
               return prefix + Convert.ToBase64String(sig, Base64FormattingOptions.InsertLineBreaks);

            return prefix + Convert.ToBase64String(sig);
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