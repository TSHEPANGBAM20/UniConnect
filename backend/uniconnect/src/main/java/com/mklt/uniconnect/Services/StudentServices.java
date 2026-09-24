package com.mklt.uniconnect.Services;

import org.springframework.stereotype.Service;

import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Repositories.StudentRepo;

import java.util.List;


@Service 
public class StudentServices {

  //Service classess facillitate business logic or steps that do a particular task, and also invoke Repo methods
private StudentRepo studentRepo;
    //Whenever we want to use methods or an object without creating a new one globally, we have to add that Object as a parameter in teh constructor of the class it is used in.
    public StudentServices(StudentRepo studentRepo) {
      this.studentRepo=studentRepo;
    }





    public String createUser(String name, String surname, String number, String email,String password ){
       //first check whether a user of the same student email exists by repo
         boolean exists = studentRepo.existsByStudentEmail(email);

       if (exists){
            //User does not exist message
            return "User Already Exists";
       }
       else{
        //create student Object
        Student student = new Student(name, surname, number, email, password);
        studentRepo.save(student); //saves student to database
        return "Welcome " + student.getName() + "!";
       }

    }
    public String loginUser(String email, String password) {
      List<Student> studentList =studentRepo.findByStudentEmail(email);

      if (studentList.isEmpty()){
        return "User Does Not Exist";
      }

      Student student = studentList.get(0);

      if (!student.getPassword().equals(password)) {
        return "Incorrect Password";
      }
      return "Welcome back, " + student.getName() +"!";
    }
    
}
