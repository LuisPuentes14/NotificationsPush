using System;
using System.Security.Cryptography;
using System.Text;

public class SHA256Encryption
{
    public static string EncryptWithSHA256(string input)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Convertir la cadena de entrada en un array de bytes y calcular el hash.
            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convertir el array de bytes del hash a una cadena en hexadecimal.
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Retornar la cadena en hexadecimal.
            return sBuilder.ToString();
        }
    }
}
