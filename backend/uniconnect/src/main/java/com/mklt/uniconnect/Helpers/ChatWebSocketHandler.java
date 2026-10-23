package com.mklt.uniconnect.Helpers;

import org.springframework.stereotype.Component;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

import java.io.IOException;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

@Component
public class ChatWebSocketHandler extends TextWebSocketHandler{

    private final Map<Long, Set<WebSocketSession>> projectSessions =new ConcurrentHashMap<>();
    
    @Override
     public void afterConnectionEstablished(WebSocketSession session) {

        Long projectId = getProjectId(session);

        if (projectId == null) {
            try {
                session.close(CloseStatus.BAD_DATA);
            } catch (IOException e) {
                // Connection is already unusable.
            }
            return;
        }

        projectSessions
                .computeIfAbsent(projectId, id -> ConcurrentHashMap.newKeySet())
                .add(session);
    }


    
    public void broadcastToProject(Long projectId, String message) {

        Set<WebSocketSession> sessions = projectSessions.get(projectId);

        if (sessions == null || sessions.isEmpty()) {
            return;
        }

        for (WebSocketSession session : sessions) {

            if (!session.isOpen()) {
                continue;
            }

            try {
                synchronized (session) {
                    if (session.isOpen()) {
                        session.sendMessage(new TextMessage(message));
                    }
                }
            } catch (IOException e) {
                try {
                    session.close(CloseStatus.SERVER_ERROR);
                } catch (IOException ignored) {
                    // Ignore cleanup failure.
                }
            }
        }
    }

      private Long getProjectId(WebSocketSession session) {

        String path = session.getUri().getPath();

        try {
            return Long.parseLong(
                    path.substring(path.lastIndexOf('/') + 1));
        } catch (NumberFormatException | NullPointerException e) {
            return null;
        }
    }



}
