package com.mklt.uniconnect.Controllers;

import java.util.List;
import java.util.Map;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.mklt.uniconnect.Entities.ChatMessage;
import com.mklt.uniconnect.Services.ChatServices;

@RestController 
@RequestMapping ("/chat") 
public class ChatController {

    private ChatServices chatService;
    
    public ChatController( ChatServices chatService) {
        this.chatService= chatService;
    }

    @PostMapping("/chatMessages")
    public List<ChatMessage> getMessages(@RequestParam("projectId") Long projectId) {
        return chatService.getMessages(projectId);
    }

    // Save a message to the project's group chat
    @PostMapping("/sendMessage")
    public ChatMessage sendMessage(
            @RequestParam("projectId") Long projectId,
            @RequestParam("senderId") Long senderId,
            @RequestBody Map<String, String> request) {

        String messageText = request.get("message");

        return chatService.sendMessage(projectId, senderId, messageText);
    }
}




