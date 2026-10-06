package com.mklt.uniconnect.Controllers;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Services.StudentServices;



@RestController 
@RequestMapping ("/auth") //Base path for auth endpoint 
public class AuthController {

    //the only job of the controller methods is to invoke the services
    private StudentServices studentService;
    public AuthController(StudentServices studentService) {
        this.studentService = studentService;
    }

    @PostMapping("/register")
    public Student registerUser (@RequestBody Student student){

        //Variables
        String name = student.getName();
        String surname = student.getSurname();
        String email = student.getEmail();
        String password = student.getPassword();

        if (name.isEmpty()){
           throw new RuntimeException("Name field cannot be empty.Please fill it in.");
        }else if (surname.isEmpty()){
           throw new RuntimeException("Surname field cannot be empty.Please fill it in.");
        }else if (email.isEmpty()){
             throw new RuntimeException("Email field cannot be empty.Please fill it in.");
        } else if (password.isEmpty()){
             throw new RuntimeException("Password field cannot be empty.Please fill it in.");
        }else if (password.length() <= 4){
             throw new RuntimeException("Password cannot be less than 4 characters.");
        }else{
             return studentService.createUser(name, surname,email, password); 
        }
        
       
    }

    //1. Data is collected into usermodel
    //1. Sent to  is collected to path as json body via

    //login
    @PostMapping("/login")
    public Student loginUser(@RequestBody Student student) {
        String email = student.getEmail();
        String password = student.getPassword();
        
        if (email.isEmpty()){
             throw new RuntimeException("Email field cannot be empty.Please fill it in.");
        }else if (password.isEmpty()){
             throw new RuntimeException("Password field cannot be empty.Please fill it in.");
        }else{
        return studentService.loginUser(email, password);
        }
    }





}
