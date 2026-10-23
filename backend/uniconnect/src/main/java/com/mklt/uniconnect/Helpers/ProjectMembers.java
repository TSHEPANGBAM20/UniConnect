package com.mklt.uniconnect.Helpers;

import com.mklt.uniconnect.Entities.Project;
import com.mklt.uniconnect.Entities.Student;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.Table;

@Entity
@Table(name = "project_members")

public class ProjectMembers {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "member_id")
    private Long id;

    @ManyToOne
    @JoinColumn(name = "user_id")
    private Student student;

    @ManyToOne // a user can be in many groups 
    @JoinColumn(name = "projectID")
    private Project project;

    @Enumerated(EnumType.STRING) // Tells JPA to store the Enum as text (e.g., "LEADER")
    private ProjectRoles role;

    //constructors
    public ProjectMembers() {
    }

    public ProjectMembers(Student student, Project project, ProjectRoles role) {
        this.student = student;
        this.project = project;
        this.role = role;
    }

    //getters

    public Long getId() {
        return id;
    }

    public Student getStudent() {
        return student;
    }

    public Project getProject() {
        return project;
    }

    public ProjectRoles getRole() {
        return role;
    }
    
    



}
