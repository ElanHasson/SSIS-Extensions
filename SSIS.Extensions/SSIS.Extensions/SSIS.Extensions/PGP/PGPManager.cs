
namespace SSIS.Extensions.PGPTask
{
    public static class PGPManager
    {
        /// <summary>
        /// Decrypts the specified input file.
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="privateKeyFile">The private key file.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <param name="outputFile">The output file.</param>
        /// <param name="overwriteTarget">if set to <c>true</c> [overwrite target].</param>
        /// <param name="removeSource">if set to <c>true</c> [remove source].</param>
        public static void Decrypt(string inputFile, string privateKeyFile, string passPhrase, string outputFile, bool overwriteTarget, bool removeSource)
        {
            Common.ValidateFile(inputFile);
            Common.ValidateFile(privateKeyFile);
            Common.ValidateOverwrite(overwriteTarget, outputFile);
            PGPEncryptDecrypt.Decrypt(inputFile, privateKeyFile, passPhrase, outputFile);
            Common.RemoveFile(removeSource, inputFile);
        }

        /// <summary>
        /// Encrypts the specified input file.
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="publicKeyFile">The public key file.</param>
        /// <param name="outputFile">The output file.</param>
        /// <param name="overwriteTarget">if set to <c>true</c> [overwrite target].</param>
        /// <param name="removeSource">if set to <c>true</c> [remove source].</param>
        /// <param name="armor">if set to <c>true</c> [armor].</param>
        public static void Encrypt(string inputFile, string publicKeyFile, string outputFile, bool overwriteTarget, bool removeSource, bool armor)
        {
            Common.ValidateFile(inputFile);
            Common.ValidateFile(publicKeyFile);
            Common.ValidateOverwrite(overwriteTarget, outputFile);
            PGPEncryptDecrypt.EncryptFile(inputFile, outputFile, publicKeyFile, armor, true);
            Common.RemoveFile(removeSource, inputFile);
        }

        /// <summary>
        /// Encrypts the and sign.
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="publicKeyFile">The public key file.</param>
        /// <param name="privateKeyFile">The private key file.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <param name="outputFile">The output file.</param>
        /// <param name="overwriteTarget">if set to <c>true</c> [overwrite target].</param>
        /// <param name="removeSource">if set to <c>true</c> [remove source].</param>
        /// <param name="armor">if set to <c>true</c> [armor].</param>
        public static void EncryptAndSign(string inputFile, string publicKeyFile, string privateKeyFile, string passPhrase, string outputFile, bool overwriteTarget, bool removeSource, bool armor)
        {
            Common.ValidateFile(inputFile);
            Common.ValidateFile(publicKeyFile);
            Common.ValidateFile(privateKeyFile);
            Common.ValidateOverwrite(overwriteTarget, outputFile);
            PGPEncryptDecrypt.EncryptAndSign(inputFile, outputFile, publicKeyFile, privateKeyFile, passPhrase, armor);
            Common.RemoveFile(removeSource, inputFile);
        }
    }
}
