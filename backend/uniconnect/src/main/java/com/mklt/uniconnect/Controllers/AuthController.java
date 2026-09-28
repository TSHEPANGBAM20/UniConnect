package com.mklt.uniconnect.Controllers;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.mklt.uniconnect.Services.StudentServices;


@RestController 
@RequestMapping ("/auth")
public class AuthController {

    //the only job of the controller methods is to invoke the services
    private StudentServices studentService;
    public AuthController(StudentServices studentService) {
        this.studentService = studentService;
    }

    @PostMapping("/register")
    public String registerUser (@RequestParam("name") String name, @RequestParam("surname") String surname, @RequestParam("number") String number,@RequestParam("email") String email, @RequestParam ("password") String password){
        return studentService.createUser(name, surname, number, email, password); 
    }

    //login
    @PostMapping("/login")
    public String loginUser(@RequestParam("email") String email, @RequestParam("password") String password) {
        return studentService.loginUser(email, password);
    }





}
