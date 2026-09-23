package com.mklt.uniconnect.Entities;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

 
@Entity  //all variables will be translated to table columns
@Table (name = "ucStudents") //the table in the database where the information is supposed to go
public class Student {

@Id //says that the variable is a primary key
@GeneratedValue //for auto increment
private Long id;

private String student_name;
private String student_surname;
@Column (unique = true) //the variable/column will be unique 
private String student_number;

@Column (unique = true)
private String studentEmail;

private String password;

    //an empty constructor = allows a version of object creation where parameters aren't passed
    //in this case its so that the repository handles it for me
    public Student() {
    }


     public Student(String name, String surname, String number, String studentEmail,String password) {
        this.student_name = name;
        this.student_surname = surname;
        this.student_number = number;
        this.studentEmail = studentEmail;
        this.password = password;
        
    }

    //setters
    //setters are used to change information on an object

    public void setName(String name){
        this.student_name = name;
    }

    public void setSurname(String surname){
        this.student_surname = surname;
    }

    public void setEmail(String email){
        this.studentEmail= email;
    }

     public void setNumber(String number){
        this.student_number = number;
    }

    public void setPassword(String password){
        this.password = password;
    }

    //getters
    //getters are used dusing run time to get the particular value of a certain private variable of an entity
    public String getName(){
        return student_name;
    }

     public String getSurname(){
        return student_surname;
    }

    public String getNumber (){
        return student_number;
    }

    public String getEmail(){
        return studentEmail;
    }

    public String getPassword(){
        return password;
    }

}
