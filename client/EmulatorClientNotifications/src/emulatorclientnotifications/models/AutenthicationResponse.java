/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package emulatorclientnotifications.models;

/**
 *
 * @author aleja
 */
public class AutenthicationResponse {
    
     private String message;

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public AuthenticationToken getToken() {
        return value;
    }

    public void setToken(AuthenticationToken token) {
        this.value = token;
    }
     private String status;
     private AuthenticationToken value ;    
    
}
