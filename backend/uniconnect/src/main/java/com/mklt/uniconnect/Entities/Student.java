
package com.mklt.uniconnect.Entities;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "UniConnectStudents")
public class Student {

    @Id
    @GeneratedValue
    private Long id;

    private String student_name;
    private String student_surname;

    @Column(unique = true)
    private String studentEmail;

    private String password;

    // Empty constructor required by JPA
    public Student() {
    }

    // Constructor used when registering a student
    public Student(String name, String surname, String studentEmail, String password) {
        this.student_name = name;
        this.student_surname = surname;
        this.studentEmail = studentEmail;
        this.password = password;
    }

    // Setters
    public void setName(String name) {
        this.student_name = name;
    }

    public void setSurname(String surname) {
        this.student_surname = surname;
    }

    public void setEmail(String email) {
        this.studentEmail = email;
    }

    public void setPassword(String password) {
        this.password = password;
    }

    // Getters
    public String getName() {
        return student_name;
    }

    public String getSurname() {
        return student_surname;
    }

    public String getEmail() {
        return studentEmail;
    }

    public String getPassword() {
        return password;
    }

    public Long getId() {
        return id;
    }
}
