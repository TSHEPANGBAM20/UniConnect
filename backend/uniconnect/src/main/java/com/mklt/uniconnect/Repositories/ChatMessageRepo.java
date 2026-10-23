package com.mklt.uniconnect.Repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import com.mklt.uniconnect.Entities.Chat;
import com.mklt.uniconnect.Entities.ChatMessage;
import java.util.List;

public interface ChatMessageRepo extends JpaRepository<ChatMessage, Long>{
 List<ChatMessage> findByChat_IdOrderByTimeSentAsc(Long chatId);
}
