package com.mklt.uniconnect.Helpers;

import org.springframework.context.annotation.Configuration;
import org.springframework.web.socket.config.annotation.EnableWebSocket;
import org.springframework.web.socket.config.annotation.WebSocketConfigurer;
import org.springframework.web.socket.config.annotation.WebSocketHandlerRegistry;

@Configuration 
@EnableWebSocket
public class WebSocketConfig implements WebSocketConfigurer {

    private ChatWebSocketHandler chatWebSHandler;

    public WebSocketConfig(ChatWebSocketHandler chatWebSHandler) {
        this.chatWebSHandler = chatWebSHandler;
    }

    

      @Override
    public void registerWebSocketHandlers(WebSocketHandlerRegistry registry) {
        registry.addHandler(chatWebSHandler, "/ws/chat/{projectId}").setAllowedOriginPatterns("*");
    }


    
}



