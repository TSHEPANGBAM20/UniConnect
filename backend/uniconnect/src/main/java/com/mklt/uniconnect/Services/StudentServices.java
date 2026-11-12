
package com.mklt.uniconnect.Services;

import org.springframework.http.HttpStatus;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;

import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Repositories.StudentRepo;

@Service
public class StudentServices {

    private StudentRepo studentRepo;
    private BCryptPasswordEncoder passwordEncoder;

    public StudentServices(StudentRepo studentRepo, BCryptPasswordEncoder passwordEncoder) {
        this.studentRepo = studentRepo;
        this.passwordEncoder = passwordEncoder;
    }

    // Register a new student
    public Student createUser(String name, String surname, String email, String password) {

        boolean exists = studentRepo.existsByStudentEmail(email);

        if (exists) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "User already exists"
            );
        }

        String hashedPassword = passwordEncoder.encode(password);

        Student student = new Student(name, surname, email, hashedPassword);

        studentRepo.save(student);

        return student;
    }

    // Login student
    public Student loginUser(String email, String password) {

        Student student = studentRepo.findByStudentEmail(email);

        if (student == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Student does not exist"
            );
        }

        String hashedPassword = student.getPassword();

        boolean passwordCheck = passwordEncoder.matches(password, hashedPassword);

        if (!passwordCheck) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Password Incorrect! Try again"
            );
        }

        return student;
    }

    // Update student name, surname and email
    public Student updateStudentNameAndEmail(
            Long id,
            String updatedName,
            String updatedSurname,
            String updatedEmail) {

        // Find student
        Student student = studentRepo.findById(id)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.NOT_FOUND,
                        "Student not found"
                ));

        // Update first name
        if (updatedName != null && !updatedName.isBlank()) {
            student.setName(updatedName);
        }

        // Update surname
        if (updatedSurname != null && !updatedSurname.isBlank()) {
            student.setSurname(updatedSurname);
        }

        // Update email
        if (updatedEmail != null && !updatedEmail.isBlank()) {

            boolean existsByEmail =
                    studentRepo.existsByStudentEmailAndIdNot(updatedEmail, id);

            if (existsByEmail) {
                throw new ResponseStatusException(
                        HttpStatus.CONFLICT,
                        "Student with email already exists."
                );
            }

            student.setEmail(updatedEmail);
        }

        // Save and return updated student
        return studentRepo.save(student);
    }
}
