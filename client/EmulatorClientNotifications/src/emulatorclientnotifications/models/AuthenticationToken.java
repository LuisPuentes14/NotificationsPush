/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package emulatorclientnotifications.models;

/**
 *
 * @author aleja
 */
public class AuthenticationToken {
    private String token;
    private String minutesExpiresToken;

    public String getToken() {
        return token;
    }

    public void setToken(String token) {
        this.token = token;
    }

    public String getMinutesExpiresToken() {
        return minutesExpiresToken;
    }

    public void setMinutesExpiresToken(String minutesExpiresToken) {
        this.minutesExpiresToken = minutesExpiresToken;
    }
    
    
}
