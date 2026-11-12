
package com.mklt.uniconnect.Services;

import java.time.LocalDate;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.server.ResponseStatusException;

import com.mklt.uniconnect.Entities.Chat;
import com.mklt.uniconnect.Entities.Project;
import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Helpers.ProjectMembers;
import com.mklt.uniconnect.Helpers.ProjectRoles;
import com.mklt.uniconnect.Helpers.ProjectStatus;
import com.mklt.uniconnect.Repositories.ChatMessageRepo;
import com.mklt.uniconnect.Repositories.ChatRepo;
import com.mklt.uniconnect.Repositories.ProjectMembersRepo;
import com.mklt.uniconnect.Repositories.ProjectsRepo;
import com.mklt.uniconnect.Repositories.StudentRepo;

@Service
public class ProjectsServices {

    private ProjectsRepo projectsRepo;
    private ProjectMembersRepo projectMembersRepo;
    private StudentRepo studentRepo;
    private ChatRepo chatRepo;
    private ChatMessageRepo chatMessageRepo;
    private ProjectFileServices projectFileServices;

    public ProjectsServices(
            ProjectsRepo projectsRepo,
            ProjectMembersRepo projectMembersRepo,
            StudentRepo studentRepo,
            ChatRepo chatRepo,
            ChatMessageRepo chatMessageRepo,
            ProjectFileServices projectFileServices) {

        this.projectsRepo = projectsRepo;
        this.projectMembersRepo = projectMembersRepo;
        this.studentRepo = studentRepo;
        this.chatRepo = chatRepo;
        this.chatMessageRepo = chatMessageRepo;
        this.projectFileServices = projectFileServices;
    }

    // Create project
    @Transactional
    public Project createProject(
            Student student,
            String projectName,
            String projectDescription,
            LocalDate startDate,
            LocalDate endDate) {

        if (projectsRepo.existsByProjectName(projectName)) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "A project of that name already exists.");
        }

        Integer projectCode = generateProjectCode();

        Project newProject = new Project(
                projectName,
                projectDescription,
                startDate,
                endDate,
                ProjectStatus.INPROGRESS,
                projectCode);

        projectsRepo.save(newProject);

        ProjectMembers newMember = new ProjectMembers(
                student,
                newProject,
                ProjectRoles.LEADER);

        projectMembersRepo.save(newMember);

        return newProject;
    }

    // Update project details - Team Lead only
    @Transactional
    public Project updateProject(
            Long projectId,
            Long studentId,
            String projectName,
            String projectDescription,
            LocalDate startDate,
            LocalDate endDate,
            ProjectStatus status) {

        // Find the project
        Project project = projectsRepo.findById(projectId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.NOT_FOUND,
                        "Project not found."));

        // Find the student requesting the update
        Student student = studentRepo.findById(studentId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.NOT_FOUND,
                        "Student not found."));

        // Check the student's membership and role
        ProjectMembers membership =
                projectMembersRepo.findByStudentAndProject(student, project);

        if (membership == null ||
                membership.getRole() != ProjectRoles.LEADER) {

            throw new ResponseStatusException(
                    HttpStatus.FORBIDDEN,
                    "Only the Team Lead can edit project details.");
        }

        // Validate project name
        if (projectName == null || projectName.trim().isEmpty()) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Project name cannot be empty.");
        }

        // Validate description
        if (projectDescription == null ||
                projectDescription.trim().isEmpty()) {

            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Project description cannot be empty.");
        }

        // Validate dates
        if (startDate == null || endDate == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Start date and end date are required.");
        }

        if (endDate.isBefore(startDate)) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "End date cannot be before the start date.");
        }

        // Validate status
        if (status == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Project status is required.");
        }

        String cleanedProjectName = projectName.trim();

        // Prevent duplicate project names
        if (!cleanedProjectName.equalsIgnoreCase(project.getProjectName())
                && projectsRepo.existsByProjectName(cleanedProjectName)) {

            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "A project of that name already exists.");
        }

        // Update project information
        project.setProjectName(cleanedProjectName);
        project.setProjectDescription(projectDescription.trim());
        project.setStartDate(startDate);
        project.setEndDate(endDate);
        project.setStatus(status);

        return projectsRepo.save(project);
    }

    // Get project details
    public Project GetProjectDetails(Long projectId) {

        return projectsRepo.findById(projectId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Project not found"));
    }

    // Delete project
    public void deleteProject(Long id) {

        Project project = projectsRepo.findById(id)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Project not found"));

        projectFileServices.deleteAllForProject(project);
        projectsRepo.deleteById(project.getID());
    }

    // Add member to project
    public ProjectMembers addMemberToProject(Long projectId, String email) {

        Student student = studentRepo.findByStudentEmail(email);

        if (student == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Student does not exist");
        }

        Project project = projectsRepo.findById(projectId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Project not found"));

        boolean isMember =
                projectMembersRepo.existsByStudentAndProject(student, project);

        if (isMember) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "User already in project.");
        }

        ProjectMembers newMember = new ProjectMembers(
                student,
                project,
                ProjectRoles.MEMBER);

        projectMembersRepo.save(newMember);

        return newMember;
    }

    // Join project using project code
    public ProjectMembers joinProject(Integer projectCode, String email) {

        Student student = studentRepo.findByStudentEmail(email);
        Project project = projectsRepo.findByProjectCode(projectCode);

        if (student == null || project == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Student or project not found.");
        }

        boolean isMember =
                projectMembersRepo.existsByStudentAndProject(student, project);

        if (isMember) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "User already in project.");
        }

        ProjectMembers newMember = new ProjectMembers(
                student,
                project,
                ProjectRoles.MEMBER);

        projectMembersRepo.save(newMember);

        return newMember;
    }

    // Generate unique project code
    public Integer generateProjectCode() {

        Random random = new Random();
        int randomCode;
        boolean projectExistsByCode;

        do {
            randomCode = random.nextInt(100000, 999999);
            projectExistsByCode =
                    projectsRepo.existsByProjectCode(randomCode);

        } while (projectExistsByCode);

        return randomCode;
    }

    // Show all projects belonging to a student
    public List<Project> showStudentProjects(Long studentId) {

        Student student = studentRepo.findById(studentId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Student not found"));

        List<ProjectMembers> memberships =
                projectMembersRepo.findAllByStudent(student);

        List<Project> projects = new ArrayList<>();

        for (ProjectMembers membership : memberships) {
            projects.add(membership.getProject());
        }

        return projects;
    }

    // Show all members in a project
    public List<ProjectMembers> showStudentsInProject(Long projectId) {

        Project project = projectsRepo.findById(projectId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Project not found"));

        return projectMembersRepo.findAllByProject(project);
    }

    // Remove student from project
    public void removeStudentFromGroup(
            String studentEmail,
            Long projectId) {

        Student student = studentRepo.findByStudentEmail(studentEmail);

        if (student == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Student not found.");
        }

        Project currentProject = projectsRepo.findById(projectId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Project not found"));

        ProjectMembers member =
                projectMembersRepo.findByStudentAndProject(
                        student, currentProject);

        if (member == null) {
            throw new ResponseStatusException(
                    HttpStatus.BAD_REQUEST,
                    "Student is not a member of this project.");
        }

        projectMembersRepo.deleteById(member.getId());
    }

    // Disband project
    @Transactional
    public void disbandGroup(Long projectId) {

        Project currentProject = projectsRepo.findById(projectId)
                .orElseThrow(() -> new ResponseStatusException(
                        HttpStatus.BAD_REQUEST,
                        "Project not found"));

        projectMembersRepo.deleteAllByProject(currentProject);

        Chat currentChat = chatRepo.findByProject(currentProject);

        if (currentChat != null) {
            chatMessageRepo.deleteAllByChat(currentChat);
            chatRepo.delete(currentChat);
        }

        projectFileServices.deleteAllForProject(currentProject);

        projectsRepo.deleteById(currentProject.getID());
    }
}
