using System;
using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace GS.MFH.Common.Extensions
{
    public static class GuidExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <remarks>this came from: https://www.stevejgordon.co.uk/using-high-performance-dotnetcore-csharp-techniques-to-base64-encode-a-guid</remarks>
        public static string EncodeBase64StringCast(this Guid guid)
        {
            const char dash = '-';
            const byte forwardSlashByte = 0x2F;
            const byte plusByte = 0x2B;
            const char underscore = '_';

            Span<byte> guidBytes = stackalloc byte[16];
            Span<byte> encodedBytes = stackalloc byte[24];

            MemoryMarshal.TryWrite(guidBytes, ref guid);
            Base64.EncodeToUtf8(guidBytes, encodedBytes, out _, out _);
            Span<char> chars = stackalloc char[22];
            for (var i =0; i< 22; i++)
            {
                switch (encodedBytes[i])
                {
                    case forwardSlashByte:
                        chars[i] = dash;
                        break;

                    case plusByte:
                        chars[i] = underscore;
                        break;

                    default:
                        chars[i] = (char) encodedBytes[i];
                        break;
                }

            }

            return new string(chars.ToArray());

        }
        
    }
}
