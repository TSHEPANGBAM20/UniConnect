
package com.mklt.uniconnect.Entities;

import java.time.LocalDateTime;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.PrePersist;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

@Entity
public class ChatMessage {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long Id;

    @ManyToOne
    @JoinColumn(name = "chatID")
    @JsonIgnoreProperties({"messages", "project"})
    private Chat chat;

    @ManyToOne
    @JoinColumn(name = "user_id")
    @JsonIgnoreProperties({"password"})
    private Student sender;

    private String message;
    private LocalDateTime timeSent;

    // Constructors
    public ChatMessage(Chat chat, Student sender, String message) {
        this.chat = chat;
        this.sender = sender;
        this.message = message;
    }

    public ChatMessage() {
    }

    // Getters
    public Long getId() {
        return Id;
    }

    public Chat getChat() {
        return chat;
    }

    public Student getSender() {
        return sender;
    }

    // This exposes the sender's ID directly in the JSON response.
    public Long getSenderId() {
        return sender != null ? sender.getId() : null;
    }

    public String getMessage() {
        return message;
    }

    public LocalDateTime getTimeSent() {
        return timeSent;
    }

    // Setters
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
