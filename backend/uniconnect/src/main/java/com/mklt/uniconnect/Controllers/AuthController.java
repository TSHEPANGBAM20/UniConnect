package com.mklt.uniconnect.Controllers;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

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
    public String registerUser (@RequestParam("name") String name, @RequestParam("surname") String surname, @RequestParam("number") String number,@RequestParam("email") String email, @RequestParam ("password") String password){
        if (name.isEmpty()){
            return "Name field cannot be empty.Please fill it in.";
        }else if (surname.isEmpty()){
            return "Surname field cannot be empty.Please fill it in.";
        }else if (number.isEmpty()){
            return "Number field cannot be empty.Please fill it in.";
        }else if (email.isEmpty()){
            return "Email field cannot be empty.Please fill it in.";
        } else if (password.isEmpty()){
            return "Password field cannot be empty.Please fill it in.";
        }else if (password.length() < 4){
            return "Your Password cannot be less than 3 characters. Please Try again";
        }else{
             return studentService.createUser(name, surname, number, email, password); 
        }
        
       
    }

    //login
    @PostMapping("/login")
    public String loginUser(@RequestParam("email") String email, @RequestParam("password") String password) {
        if (email.isEmpty()){
            return "Email field cannot be empty. Please fill it in.";
        }else if (password.isEmpty()){
            return "Email field cannot be empty. Please fill it in.";
        }else{
        return studentService.loginUser(email, password);
        }
    }





}
