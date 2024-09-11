using System;
using Firefly.Box;
using ENV.Data.DataProvider;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class FloatMSBasicNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;

        public FloatMSBasicNumberStorage(int size)
        {
            _size = size;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            var bytes = loader.GetByteArray();

            if (bytes[bytes.Length - 1] == 0) return 0;

            var ieee = new byte[bytes.Length];
            byte sign = 0x00;
            int ieee_exp = 0x0000;
            int i;

            sign = (byte)(bytes[bytes.Length - 2] & 0x80);

            ieee[bytes.Length - 1] |= sign;

            ieee_exp = bytes[bytes.Length - 1] - 128 - 1 + 1023;

            ieee[bytes.Length - 1] |= (byte)(ieee_exp >> 4);

            ieee[bytes.Length - 2] |= (byte)(ieee_exp << 4);

            for (i = bytes.Length - 2; i > 0; i--)
            {
                bytes[i] <<= 1;
                bytes[i] |= (byte)(bytes[i - 1] >> 7);
            }
            bytes[0] <<= 1;

            for (i = bytes.Length - 2; i > 0; i--)
            {
                ieee[i] |= (byte)(bytes[i] >> 4);
                ieee[i - 1] |= (byte)(bytes[i] << 4);
            }
            ieee[0] |= (byte)(bytes[0] >> 4);

            var doubleBytes = new byte[8];
            Array.Copy(ieee, 0, doubleBytes, 8 - _size, _size);

            return BitConverter.ToDouble(doubleBytes, 0);
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
            {
                saver.SaveNull();
                return;
            }

            var bytes = new byte[_size];

            var ieee = new byte[_size];
            Array.Copy(BitConverter.GetBytes((double)value), 8 - _size, ieee, 0, _size);

            byte sign = 0x00;
            byte any_on = 0x00;
            uint msbin_exp = 0x0000;
            int i;

            for (i = 0; i < _size; i++) any_on |= ieee[i];
            if (any_on != 0)
            {
                sign = (byte)(ieee[_size - 1] & 0x80);
                bytes[_size - 2] |= sign;
                msbin_exp = (uint)(ieee[_size - 1] & 0x7f) * 0x10;
                msbin_exp += (uint)(ieee[_size - 2] >> 4);

                //if (msbin_exp - 0x3ff > 0x80) return 1;

                bytes[_size - 1] = (byte)(msbin_exp - 0x3ff + 0x80 + 1);

                ieee[_size - 2] &= 0x0f;

                for (i = _size - 2; i > 0; i--)
                {
                    bytes[i] |= (byte)(ieee[i] << 3);
                    bytes[i] |= (byte)(ieee[i - 1] >> 5);
                }

                bytes[0] |= (byte)(ieee[0] << 3);
            }

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(bytes, value.ToDecimal(), 9);
            else
                saver.SaveByteArray(bytes);
        }
    }
}
