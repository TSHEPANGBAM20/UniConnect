package com.mklt.uniconnect.Repositories;

import org.springframework.data.jpa.repository.JpaRepository;

import com.mklt.uniconnect.Entities.Student;


public interface StudentRepo extends JpaRepository<Student, Long> {
//all custom queries will be done here
//Student findByEmail(String email); 
//we want it to find a student in the student table with the email(for login)
boolean existsByStudentEmail(String student_email);
}
