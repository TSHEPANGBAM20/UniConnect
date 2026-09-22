package com.mklt.uniconnect.Entities;

import jakarta.persistence.Entity;

@Entity 
public class Student {
private String student_name;
private String student_number;

    public Student() {
    }

     public Student(String name, String number) {
        this.student_name = name;
        this.student_number = number;
    }

    public void setName(String name){
        this.student_name = name;
    }

     public void setNumber(String number){
        this.student_number = number;
    }

    public String getName(){
        return student_name;
    }

    public String getNumber (){
        return student_number;
    }

}
