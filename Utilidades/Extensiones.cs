using System;

namespace PLCServicio.Utilidades
{
    public static class Extensiones
    {
        /// <summary>
        /// Convierte un array de bytes a float (IEEE 754)
        /// </summary>
        public static float BytesToFloat(this byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
            {
                throw new ArgumentException("Array de bytes insuficiente para convertir a float");
            }

            // Invertir bytes si es necesario (depende del orden del PLC)
            var temp = new byte[4];
            Array.Copy(bytes, startIndex, temp, 0, 4);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToSingle(temp, 0);
        }

        /// <summary>
        /// Convierte un array de bytes a int (32 bits)
        /// </summary>
        public static int BytesToInt32(this byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 4)
            {
                throw new ArgumentException("Array de bytes insuficiente para convertir a int32");
            }

            var temp = new byte[4];
            Array.Copy(bytes, startIndex, temp, 0, 4);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToInt32(temp, 0);
        }

        /// <summary>
        /// Convierte un array de bytes a short (16 bits)
        /// </summary>
        public static short BytesToInt16(this byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length < startIndex + 2)
            {
                throw new ArgumentException("Array de bytes insuficiente para convertir a int16");
            }

            var temp = new byte[2];
            Array.Copy(bytes, startIndex, temp, 0, 2);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToInt16(temp, 0);
        }

        /// <summary>
        /// Convierte un array de bytes a boolean
        /// </summary>
        public static bool BytesToBoolean(this byte[] bytes, int byteIndex = 0, int bitIndex = 0)
        {
            if (bytes == null || bytes.Length <= byteIndex)
            {
                throw new ArgumentException("Array de bytes insuficiente para convertir a boolean");
            }

            if (bitIndex < 0 || bitIndex > 7)
            {
                throw new ArgumentException("El índice del bit debe estar entre 0 y 7");
            }

            return (bytes[byteIndex] & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// Convierte float a array de bytes
        /// </summary>
        public static byte[] FloatToBytes(this float valor)
        {
            var bytes = BitConverter.GetBytes(valor);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Convierte int a array de bytes
        /// </summary>
        public static byte[] Int32ToBytes(this int valor)
        {
            var bytes = BitConverter.GetBytes(valor);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Valida si una dirección IP es válida
        /// </summary>
        public static bool EsIpValida(this string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            var partes = ip.Split('.');
            if (partes.Length != 4)
                return false;

            foreach (var parte in partes)
            {
                if (!byte.TryParse(parte, out byte numero))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Redondea un double a un número específico de decimales
        /// </summary>
        public static double RedondearA(this double valor, int decimales)
        {
            return Math.Round(valor, decimales, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Redondea un float a un número específico de decimales
        /// </summary>
        public static float RedondearA(this float valor, int decimales)
        {
            return (float)Math.Round(valor, decimales, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Limita un valor entre un mínimo y un máximo
        /// </summary>
        public static T Limitar<T>(this T valor, T minimo, T maximo) where T : IComparable<T>
        {
            if (valor.CompareTo(minimo) < 0)
                return minimo;
            
            if (valor.CompareTo(maximo) > 0)
                return maximo;
            
            return valor;
        }
    }
}
