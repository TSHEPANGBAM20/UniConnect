
package com.mklt.uniconnect.Controllers;

import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ResponseStatusException;

import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Services.StudentServices;

@RestController
@RequestMapping("/auth")
public class AuthController {

    private StudentServices studentService;

    public AuthController(StudentServices studentService) {
        this.studentService = studentService;
    }

    // Register a student
    @PostMapping("/register")
    public Student registerUser(@RequestBody Student student) {

        String name = student.getName();
        String surname = student.getSurname();
        String email = student.getEmail();
        String password = student.getPassword();

        if (name == null || name.isBlank()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Name field cannot be empty. Please fill it in."
            );
        } else if (surname == null || surname.isBlank()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Surname field cannot be empty. Please fill it in."
            );
        } else if (email == null || email.isBlank()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Email field cannot be empty. Please fill it in."
            );
        } else if (password == null || password.isBlank()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Password field cannot be empty. Please fill it in."
            );
        } else if (password.length() <= 4) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Password cannot be less than 4 characters."
            );
        }

        return studentService.createUser(name, surname, email, password);
    }

    // Login
    @PostMapping("/login")
    public Student loginUser(@RequestBody Student student) {

        String email = student.getEmail();
        String password = student.getPassword();

        if (email == null || email.isBlank()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Email field cannot be empty. Please fill it in."
            );
        } else if (password == null || password.isBlank()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Password field cannot be empty. Please fill it in."
            );
        }

        return studentService.loginUser(email, password);
    }

    // Update student name, surname and email
    @PutMapping("/updateEmailOrName")
    public Student updateEmailOrName(
            @RequestParam("id") Long id,
            @RequestParam("updatedName") String updatedName,
            @RequestParam("updatedSurname") String updatedSurname,
            @RequestParam("updatedEmail") String updatedEmail) {

        return studentService.updateStudentNameAndEmail(
                id,
                updatedName,
                updatedSurname,
                updatedEmail
        );
    }
}
