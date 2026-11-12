
package com.mklt.uniconnect.Controllers;

import java.time.LocalDate;
import java.util.List;

import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ResponseStatusException;

import com.mklt.uniconnect.Entities.Project;
import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Helpers.ProjectMembers;
import com.mklt.uniconnect.Repositories.StudentRepo;
import com.mklt.uniconnect.Services.ProjectsServices;

@RestController
@RequestMapping("/projects")
public class ProjectsController {

    private ProjectsServices projectsServices;
    private StudentRepo studentRepo;

    public ProjectsController(
            ProjectsServices projectsServices,
            StudentRepo studentRepo) {

        this.projectsServices = projectsServices;
        this.studentRepo = studentRepo;
    }

    // Create project
    @PostMapping("/createProject")
    public Project createProject(
            @RequestBody Project project,
            @RequestParam("id") Long studentId) {

        Student student = studentRepo.findById(studentId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Student not found"));

        String projectName = project.getProjectName();
        String projectDescription = project.getProjectDescription();
        LocalDate startDate = project.getStartDate();
        LocalDate endDate = project.getEndDate();

        if (projectName == null || projectName.trim().isEmpty()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Project name cannot be empty!");
        }

        if (projectDescription == null ||
                projectDescription.trim().isEmpty()) {

            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Project description cannot be empty!");
        }

        if (startDate == null || endDate == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Project dates cannot be empty!");
        }

        return projectsServices.createProject(
                student,
                projectName,
                projectDescription,
                startDate,
                endDate);
    }

    // Update project - Team Lead only
    @PostMapping("/updateProject")
    public Project updateProject(
            @RequestParam("projectId") Long projectId,
            @RequestParam("studentId") Long studentId,
            @RequestBody Project updatedProject) {

        return projectsServices.updateProject(
                projectId,
                studentId,
                updatedProject.getProjectName(),
                updatedProject.getProjectDescription(),
                updatedProject.getStartDate(),
                updatedProject.getEndDate(),
                updatedProject.getStatus());
    }

    // Add member
    @PostMapping("/addMember")
    public ProjectMembers addMember(
            @RequestParam("projectId") Long projectId,
            @RequestParam("email") String email) {

        if (email == null || email.trim().isEmpty()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Email cannot be empty!");
        }

        return projectsServices.addMemberToProject(projectId, email);
    }

    // Get project details
    @PostMapping("/getProjectInfo")
    public Project getProjectInfo(
            @RequestParam("projectId") Long projectId) {

        return projectsServices.GetProjectDetails(projectId);
    }

    // Project files are handled by ProjectFileController

    // Join by project code
    @PostMapping("/joinByCode")
    public ProjectMembers joinByProjectCode(
            @RequestParam("projectCode") Integer projectCode,
            @RequestParam("email") String email) {

        if (email == null || email.trim().isEmpty()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Email cannot be empty!");
        }

        if (projectCode == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Project code cannot be empty!");
        }

        return projectsServices.joinProject(projectCode, email);
    }

    // Show student projects
    @PostMapping("/showStudentProjects")
    public List<Project> showStudentProjects(
            @RequestParam("studentId") Long studentId) {

        return projectsServices.showStudentProjects(studentId);
    }

    // Show project members
    @PostMapping("/showStudentsInProject")
    public List<ProjectMembers> showStudentsInProject(
            @RequestParam("projectId") Long projectId) {

        return projectsServices.showStudentsInProject(projectId);
    }

    // Remove member
    @DeleteMapping("/removeMemberFromgroup")
    public void removeStudentFromGroup(
            @RequestParam("studentEmail") String studentEmail,
            @RequestParam("projectId") Long projectId) {

        projectsServices.removeStudentFromGroup(
                studentEmail,
                projectId);
    }

    // Disband project
    @DeleteMapping("/disbandGroup")
    public void disbandGroup(
            @RequestParam("projectId") Long projectId) {

        projectsServices.disbandGroup(projectId);
    }
}
