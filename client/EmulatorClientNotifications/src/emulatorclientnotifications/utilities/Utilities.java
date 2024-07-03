/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package emulatorclientnotifications.utilities;

import java.security.SecureRandom;
import java.util.Base64;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

/**
 *
 * @author aleja
 */
public class Utilities {

   
    public static SecretKeySpec getKeyFromBase64String(String keyString) throws Exception {
        byte[] decodedKey = Base64.getDecoder().decode(keyString);
        return new SecretKeySpec(decodedKey, "AES");
    }

    public static IvParameterSpec getIvFromBase64String(String ivString) throws Exception {
        byte[] decodedIv = Base64.getDecoder().decode(ivString);
        return new IvParameterSpec(decodedIv);
    }

    public static String encrypt(String algorithm, String input, SecretKeySpec key, IvParameterSpec iv) throws Exception {
        Cipher cipher = Cipher.getInstance(algorithm);
        cipher.init(Cipher.ENCRYPT_MODE, key, iv);
        byte[] cipherText = cipher.doFinal(input.getBytes());
        return Base64.getEncoder().encodeToString(cipherText);
    }

    public static String decrypt(String algorithm, String cipherText, SecretKeySpec key, IvParameterSpec iv) throws Exception {
        Cipher cipher = Cipher.getInstance(algorithm);
        cipher.init(Cipher.DECRYPT_MODE, key, iv);
        byte[] plainText = cipher.doFinal(Base64.getDecoder().decode(cipherText));
        return new String(plainText);
    }

}
