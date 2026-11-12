
package com.mklt.uniconnect.Entities;

import java.time.LocalDate;

import com.mklt.uniconnect.Helpers.ProjectStatus;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;

@Entity
public class Project {

    @Id
    @GeneratedValue
    private Long projectID;

    @Column(unique = true)
    private String projectName;

    private String projectDescription;
    private LocalDate startDate;
    private LocalDate endDate;
    private String projectFile;

    @Enumerated(EnumType.STRING)
    private ProjectStatus status;

    @Column(unique = true)
    private Integer projectCode;

    // Setters

    public void setProjectDescription(String description) {
        this.projectDescription = description;
    }

    public void setProjectName(String name) {
        this.projectName = name;
    }

    public void setStartDate(LocalDate startDate) {
        this.startDate = startDate;
    }

    public void setEndDate(LocalDate endDate) {
        this.endDate = endDate;
    }

    public void setProjectFile(String file) {
        this.projectFile = file;
    }

    // Added setter for project status
    public void setStatus(ProjectStatus status) {
        this.status = status;
    }

    // Getters

    public Long getID() {
        return projectID;
    }

    public String getProjectDescription() {
        return projectDescription;
    }

    public String getProjectName() {
        return projectName;
    }

    public LocalDate getStartDate() {
        return startDate;
    }

    public LocalDate getEndDate() {
        return endDate;
    }

    public String getProjectFile() {
        return projectFile;
    }

    public int getProjectCode() {
        return projectCode;
    }

    public ProjectStatus getStatus() {
        return status;
    }

    // Constructors

    public Project() {
        // Empty constructor for JPA
    }

    public Project(
            String projectName,
            String projectDescription,
            LocalDate startDate,
            LocalDate endDate,
            ProjectStatus status,
            Integer projectCode) {

        this.projectName = projectName;
        this.projectDescription = projectDescription;
        this.startDate = startDate;
        this.endDate = endDate;
        this.status = status;
        this.projectCode = projectCode;
    }
}
