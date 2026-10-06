package com.mklt.uniconnect.Helpers;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;


//The configuration annot, used to create configured beans i.e beans without an orthodox class injection
@Configuration 
public class PasswordConfig {
  @Bean 
    public BCryptPasswordEncoder passwordEncoder(){
      return new BCryptPasswordEncoder();
    }//manages methods that create objects of a class
}
