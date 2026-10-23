package com.mklt.uniconnect.Services;

import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.transaction.annotation.Transactional;

import com.mklt.uniconnect.Entities.Chat;
import com.mklt.uniconnect.Entities.ChatMessage;
import com.mklt.uniconnect.Entities.Project;
import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Repositories.ChatMessageRepo;
import com.mklt.uniconnect.Repositories.ChatRepo;
import com.mklt.uniconnect.Repositories.ProjectsRepo;
import com.mklt.uniconnect.Repositories.StudentRepo;
import java.util.List;

@Service
public class ChatServices {

    private ChatRepo chatRepo;
    private ChatMessageRepo chatMessageRepo;
    private ProjectsRepo projectRepo;
    private StudentRepo studentRepo;

    public ChatServices(ChatRepo chatRepo, ChatMessageRepo chatMessageRepo, ProjectsRepo projectRepo, StudentRepo studentRepo) {
        this.chatRepo = chatRepo;
        this.chatMessageRepo = chatMessageRepo;
        this.projectRepo = projectRepo;
        this.studentRepo = studentRepo;
    }

    

    
    @Transactional

    public Chat getChat(Long projectId){

    return chatRepo.findChatByProjectId(projectId).orElseGet(() -> {
    Project project = projectRepo.findById(projectId).orElseThrow(() -> new ResponseStatusException(HttpStatus.NOT_FOUND, "Project not found"));
    return chatRepo.save(new Chat(project));
    });
       
    }

    @Transactional

    public ChatMessage sendMessage(Long projectId, Long senderId,String messageText){
      if (messageText.isEmpty() || messageText.isBlank()){
        throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Text not sent");
      }

      Chat chat = getChat(projectId);

      Student sender = studentRepo.findById(senderId).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST,"Student not found"));

      ChatMessage newMessage = new ChatMessage (chat,sender,messageText);

      return chatMessageRepo.save(newMessage);
    }

    @Transactional
    public List<ChatMessage> getMessages(Long projectId){
        Chat chat = chatRepo.findChatByProjectId(projectId).orElse(null);

        if (chat == null){
            return List.of();
        } // returns the chat still

        return chatMessageRepo.findByChat_IdOrderByTimeSentAsc(chat.getID()); //returns messages 
    }
    }

