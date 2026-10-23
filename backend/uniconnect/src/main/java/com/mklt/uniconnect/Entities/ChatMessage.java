package com.mklt.uniconnect.Entities;

import java.time.LocalDateTime;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.PrePersist;

@Entity
public class ChatMessage {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long Id;

    @ManyToOne
    @JoinColumn(name = "chatID") 
    private Chat chat;

    @ManyToOne
    @JoinColumn(name = "user_id")
    private Student sender;

    private String message;
    private LocalDateTime timeSent;

    //condtructors

    public ChatMessage(Chat chat, Student sender, String message) {
        this.chat = chat;
        this.sender = sender;
        this.message = message;
    }

    public ChatMessage() {
    }

    //getters

    public Long getId() {
        return Id;
    }

    public Chat getChat() {
        return chat;
    }

    public Student getSender() {
        return sender;
    }

    public String getMessage() {
        return message;
    }

    

    //setters
    
    public void setMessage(String newMessage) {
        this.message = newMessage;
    }

    @PrePersist
public void setTimeSent() {
    if (timeSent == null) {
        timeSent = LocalDateTime.now();
    }
}

    
    

    



}
