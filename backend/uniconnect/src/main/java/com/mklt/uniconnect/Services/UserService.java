package com.mklt.uniconnect.Services;

import org.springframework.stereotype.Service;
import com.mklt.uniconnect.Entities.Student;

@Service 
public class UserService {

    public String createUser(String name,String number){
        Student newStudent = new Student(name,number); // a user object is created
        return "User registered successfully";
    }
}
