#region License
/*
Copyright (c) 2017 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

namespace q2gconpsexecuteqvx
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    #endregion

    public class ScriptCode
    {
        #region Logger
        private static PseLogger logger = PseLogger.CreateLogger();
        #endregion

        #region Variables & Properties
        public List<string> Parameters { get; private set; }
        public string Code { get; private set; }
        public string ScriptWithSign { get; private set; }
        public string TableName { get; private set; }

        public string OriginalScript { get; set; }
        private string ExecuteName { get; set; } = "PSEXECUTE";
        private string Algorithm { get; set; } = "SHA256";
        #endregion

        #region Constructor & Load
        private ScriptCode(string script)
        {
            OriginalScript = script;
        }
        #endregion

        #region Static Methods
        public static ScriptCode Create(string script, CryptoManager manager)
        {
            try
            {
                var resultScript = new ScriptCode(script);
                if (resultScript.Read(manager))
                {
                    return resultScript;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("The script could not be created.", ex);
            }
        }

        public static ScriptCode Parse(string script)
        {
            try
            {
                var resultScript = new ScriptCode(script);
                if(resultScript.Read(null))
                {
                    return resultScript;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("The script is not valid.", ex);
            }
        }
        #endregion

        #region Methods
        private bool Read(CryptoManager manager)
        {
            try
            {
                if (String.IsNullOrEmpty(OriginalScript))
                {
                    logger.Warn("The script is empty.");
                    return false;
                }

                if (!OriginalScript.Trim().StartsWith(ExecuteName))
                {
                    logger.Warn($"The command {ExecuteName} was not found.");
                    return false;
                }

                var text = OriginalScript.Trim().Replace("\r\n", "\n");
                Code = Regex.Replace(text, $"{ExecuteName}[^\n]*\n", "", RegexOptions.Singleline).Trim();
                if (Code.IndexOf(Algorithm) > -1)
                    Code = Code.Substring(0, Code.IndexOf(Algorithm)).Trim();

                if (manager != null)
                    CreateSignature(manager, OriginalScript);

                //Read parameters with JSON
                var args = Regex.Match(OriginalScript, $"{ExecuteName}\\((\\[[^\\]]+\\])\\)", RegexOptions.Singleline).Groups[1].Value;

                try
                {
                    Parameters = JsonConvert.DeserializeObject<List<string>>(args);
                }
                catch(Exception ex)
                {
                    logger.Error(ex, "The script arguments could not be read in JSON.");
                }

                if (Parameters == null)
                    Parameters = new List<string>();

                //Generate name for qlik table
                TableName = Regex.Match(text, $"({ExecuteName}[^\\)]*\\))", RegexOptions.Singleline).Groups[1].Value;
                if (String.IsNullOrEmpty(TableName))
                    TableName = ExecuteName;

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The script could not be read.");
                return false;
            }
        }

        private void CreateSignature(CryptoManager manager, string script)
        {
            //Generate signature
            var signature = manager.SignWithPrivateKey(Code, false, true, Algorithm);
            var fullSignature = $"{Algorithm}:\r\n{signature}";

            if (Code.IndexOf(Algorithm) > -1)
                Code = Code.Substring(0, Code.IndexOf(Algorithm)).Trim();

            var originalWithoutSign = script.Replace("\r\n", "\n");
            var sbreak = String.Empty;
            if (originalWithoutSign.IndexOf(Algorithm) > -1)
            {
                originalWithoutSign = originalWithoutSign.Substring(0, originalWithoutSign.IndexOf(Algorithm));
                if (originalWithoutSign.EndsWith("\n"))
                    ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}{fullSignature}";
                else
                    ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}\r\n{fullSignature}";
            }
            else
            {
                ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}\r\n{fullSignature}";
            }
        }

        public string GetSignature()
        {
            try
            {
                var text = OriginalScript.Replace("\r\n", "\n");
                if (!text.EndsWith(";"))
                    text = $"{text};";

                var signature = Regex.Match(text, $"{Algorithm}:\n([^;]*);", RegexOptions.Singleline).Groups[1].Value;
                return signature.Replace("\n", "\r\n");
            }
            catch(Exception ex)
            {
                logger.Error(ex, "The signature could not be read.");
                return null;
            }
        }
        #endregion
    }
}