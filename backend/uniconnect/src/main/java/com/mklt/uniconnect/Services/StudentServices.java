package com.mklt.uniconnect.Services;

import java.util.List;

import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;

import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Repositories.StudentRepo;


@Service 
public class StudentServices {

  //Service classess facillitate business logic or steps that do a particular task, and also invoke Repo methods
private StudentRepo studentRepo;
private BCryptPasswordEncoder passwordEncoder;


    //Whenever we want to use methods or an object without creating a new one globally, we have to add that Object as a parameter in teh constructor of the class it is used in.
    public StudentServices(StudentRepo studentRepo, BCryptPasswordEncoder passwordEncoder) {
      this.studentRepo=studentRepo;
      this.passwordEncoder = passwordEncoder;
    }
    

  


    public Student createUser(String name, String surname, String email,String password ){
       //first check whether a user of the same student email exists by repo
         boolean exists = studentRepo.existsByStudentEmail(email);

       if (exists){
            //User does not exist message
    throw new RuntimeException("User already exists");   
}
       else{
        //hashing password
        String hashedPassword = passwordEncoder.encode(password);
        //create student Object
        Student student = new Student(name, surname,email, hashedPassword);
        studentRepo.save(student); //saves student to database
        return student;
       }

    }
    public Student loginUser(String email, String password) {
      List<Student> studentList =studentRepo.findByStudentEmail(email);

      if (studentList.isEmpty()){
        throw new RuntimeException("Student does not exist"); 
      }

      Student student = studentList.get(0);

      //getting the hashed password
      String hashedPassword = student.getPassword(); //store saved password (hashed)
      boolean passwordCheck = passwordEncoder.matches(password,hashedPassword);//check if password input and hashed match

      if (!passwordCheck) {
         throw new RuntimeException("Password Incorrect! Try again"); 
      }
      return student;
    }
    
}
