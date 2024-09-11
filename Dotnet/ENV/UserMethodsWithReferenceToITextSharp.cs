using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV
{
    public partial class UserMethods
    {
        partial void DoCipherDecipher(DoCipherDecipherParams doCipherDecipherParams)
        {
            Org.BouncyCastle.Crypto.IBlockCipher cipher = null;
            Org.BouncyCastle.Crypto.ICipherParameters keyParameter = new Org.BouncyCastle.Crypto.Parameters.KeyParameter(doCipherDecipherParams.Key);


            switch ((int)doCipherDecipherParams.AlgorithmId)
            {
                case 1:
                    cipher = new Org.BouncyCastle.Crypto.Engines.BlowfishEngine();
                    break;
                case 2:
                    cipher = new Org.BouncyCastle.Crypto.Engines.Cast5Engine();
                    break;
                case 3:
                case 8:
                    cipher = new Org.BouncyCastle.Crypto.Engines.DesEngine();
                    keyParameter = new Org.BouncyCastle.Crypto.Parameters.KeyParameter(doCipherDecipherParams.Key, 0, 8);
                    break;
                case 5:
                    cipher = new Org.BouncyCastle.Crypto.Engines.RC2Engine();
                    keyParameter = new Org.BouncyCastle.Crypto.Parameters.RC2Parameters(doCipherDecipherParams.Key, 0x400);
                    break;
                case 6:
                    {
                        var c = new Org.BouncyCastle.Crypto.Engines.RC4Engine();
                        c.Init(!doCipherDecipherParams.Decipher, new Org.BouncyCastle.Crypto.Parameters.KeyParameter(doCipherDecipherParams.Key));
                        var r = new byte[doCipherDecipherParams.Input.Length];
                        c.ProcessBytes(doCipherDecipherParams.Input, 0, doCipherDecipherParams.Input.Length, r, 0);
                        doCipherDecipherParams.Result = r;
                        return;
                    }
                    break;
                case 7:
                    cipher = new Org.BouncyCastle.Crypto.Engines.RC532Engine();
                    keyParameter = new Org.BouncyCastle.Crypto.Parameters.RC5Parameters(doCipherDecipherParams.Key, 16);
                    break;
                case 9:

                    using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider())
                    {
                        rsa.FromXmlString(ENV.Data.TextColumn.FromByteArray(doCipherDecipherParams.Key));
                        if (doCipherDecipherParams.Decipher)
                            doCipherDecipherParams.Result = rsa.Decrypt(doCipherDecipherParams.Input, false);
                        else
                            doCipherDecipherParams.Result = rsa.Encrypt(doCipherDecipherParams.Input, false);
                    }
                    return;
                case 10:
                    cipher = new Org.BouncyCastle.Crypto.Engines.AesEngine();
                    break;
                default:
                    return;
            }

            var iv = new byte[cipher.GetBlockSize()];
            if (doCipherDecipherParams.Vector != null)
                Array.Copy(doCipherDecipherParams.Vector, iv, Math.Min(doCipherDecipherParams.Vector.Length, iv.Length));
            Org.BouncyCastle.Crypto.ICipherParameters keyWithIVParameter = new Org.BouncyCastle.Crypto.Parameters.ParametersWithIV(keyParameter, iv);

            switch ((doCipherDecipherParams.Mode ?? "").Trim().ToUpper(CultureInfo.InvariantCulture))
            {
                case "ECB":
                case "ECB3":
                    keyWithIVParameter = keyParameter;
                    break;
                case "CFB":
                    cipher = new Org.BouncyCastle.Crypto.Modes.CfbBlockCipher(cipher, cipher.GetBlockSize() * 8);
                    break;
                case "OFB":
                    cipher = new Org.BouncyCastle.Crypto.Modes.OfbBlockCipher(cipher, cipher.GetBlockSize() * 8);
                    break;
                default:
                    cipher = new Org.BouncyCastle.Crypto.Modes.CbcBlockCipher(cipher);
                    break;
            }
            cipher.Init(!doCipherDecipherParams.Decipher, keyWithIVParameter);
            var l = doCipherDecipherParams.Input.Length;

            if (!cipher.IsPartialBlockOkay && l % cipher.GetBlockSize() != 0)
            {
                l += cipher.GetBlockSize() - l % cipher.GetBlockSize();
                var tmp = new byte[l];
                Array.Copy(doCipherDecipherParams.Input, tmp, doCipherDecipherParams.Input.Length);
                doCipherDecipherParams.Input = tmp;
            }
            ProcessBlockCipher(new Org.BouncyCastle.Crypto.BufferedBlockCipher(cipher), doCipherDecipherParams, l);
        }

        

        static void ProcessBlockCipher(Org.BouncyCastle.Crypto.BufferedCipherBase bc, DoCipherDecipherParams doCipherDecipherParams, int blockSize)
        {
            var result = new byte[blockSize];
            var x = bc.ProcessBytes(doCipherDecipherParams.Input, result, 0);
            bc.DoFinal(result, x);
            doCipherDecipherParams.Result = result;
        }
    }
}
