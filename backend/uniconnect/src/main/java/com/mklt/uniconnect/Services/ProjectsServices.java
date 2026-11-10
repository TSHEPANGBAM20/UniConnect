package com.mklt.uniconnect.Services;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.core.io.FileSystemResource;
import org.springframework.core.io.Resource;
import org.springframework.http.ContentDisposition;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.multipart.MultipartFile;
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

    public ProjectsServices(ProjectsRepo projectsRepo,ProjectMembersRepo projectMembersRepo,StudentRepo studentRepo,ChatRepo chatRepo,ChatMessageRepo chatMessageRepo,ProjectFileServices projectFileServices) {
        this.projectsRepo = projectsRepo;
        this.projectMembersRepo = projectMembersRepo;
        this.studentRepo = studentRepo;
        this.chatRepo = chatRepo;
        this.chatMessageRepo = chatMessageRepo;
        this.projectFileServices = projectFileServices;

        
    }

    //method to create project

    @Transactional 
    public Project createProject(Student student , String projectName,String projectDescription, LocalDate startDate, LocalDate endDate){
        //checks if a project of that name exists
        boolean projectExists = projectsRepo.existsByProjectName(projectName);

        if (projectExists){
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"A project of that name already exists.");
        }else{
            //generate a projectCode
            Integer projectCode = generateProjectCode();
           

           
            //create a new project and save it to db
            Project newProject = new Project(projectName, projectDescription, startDate, endDate,ProjectStatus.INPROGRESS,projectCode);
            //save to db
            projectsRepo.save(newProject);

            //save new member
             ProjectMembers newMember = new ProjectMembers(student, newProject, ProjectRoles.LEADER);
             projectMembersRepo.save(newMember);

            return newProject;

        }
        
    }

    //project files (upload, download, delete) now live in ProjectFileServices

    //method shows projects
    public Project GetProjectDetails(Long projectId){
        Project project = projectsRepo.findById(projectId).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST,"Project not found"));
        return project;
    }

    //method to delete the project
    public void deleteProject(Long id){
        Project project = projectsRepo.findById(id).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST,"Project Not found"));

        //delete the project (and its uploaded files)
        projectFileServices.deleteAllForProject(project);
        projectsRepo.deleteById(project.getID());
    } 

    public ProjectMembers addMemberToProject(Long projectId,String email){
        //find student by email
        Student student = studentRepo.findByStudentEmail(email); // check if the user exists
       

         if (student == null){
        throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Student does not exist"); 
        }

        //find the project
        Project project = projectsRepo.findById(projectId).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST,"Project Not found"));
         //check if student is a member
         boolean isMember = projectMembersRepo.existsByStudentAndProject(student, project);
         if (isMember){
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"User already in project.");
         }else{
            //add create new member with member role
        ProjectMembers newMember = new ProjectMembers(student, project,ProjectRoles.MEMBER);

        //save new member
        projectMembersRepo.save(newMember);

        return newMember;

         }
     
    }

    public ProjectMembers joinProject(Integer projectCode, String email){
        Student student = studentRepo.findByStudentEmail(email);
        Project project = projectsRepo.findByProjectCode(projectCode);

         boolean isMember = projectMembersRepo.existsByStudentAndProject(student, project);
         if (isMember){
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"User already in project.");
         }else{
             ProjectMembers newMember = new ProjectMembers(student, project,ProjectRoles.MEMBER);
         projectMembersRepo.save(newMember);
         
         return newMember;
         }
    
    }


    public Integer generateProjectCode(){
            Random random = new Random();
            int randomCode;
            boolean projectExistsByCode;

            //generate a random number
            do {
                randomCode = random.nextInt(100000,999999);
                //check if it exists
                projectExistsByCode = projectsRepo.existsByProjectCode(randomCode);

            }while(projectExistsByCode);

            return randomCode;
    }

    //showing all projects related to particular user
    public List<Project> showStudentProjects(Long studentId){
        //find the group project to which they are a member of 
        Student student = studentRepo.findById(studentId).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST,"Student Not found"));
        List<ProjectMembers> memberships = projectMembersRepo.findAllByStudent(student); //finds list of all memberships linked to member


        List<Project> projects = new ArrayList<>(); // stires projects
        
        // we loop through the project memberships
        //on the left
        for (ProjectMembers membership: memberships ) {
            
            //store project
            Project project = membership.getProject();

            //add to projects list
            projects.add(project);

        }


        return projects;
    }

    //method to show all project members

    public List<ProjectMembers> showStudentsInProject (Long project_id){
        Project project = projectsRepo.findById(project_id).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST,"Project Not found"));
        List<ProjectMembers> memberList = projectMembersRepo.findAllByProject(project);  //find all membership by project

        return memberList ;

    }

    //method to remove student from membership
    public void removeStudentFromGroup(String studentEmail,Long projectId){
        //find the student
        Student student = studentRepo.findByStudentEmail(studentEmail);
        
        //find project
        Project currentProject = projectsRepo.findById(projectId).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST,"Project Not found"));

        //Remove member by student object and projet in group 
        ProjectMembers member = projectMembersRepo.findByStudentAndProject(student,currentProject);
        projectMembersRepo.deleteById(member.getId());

    }

        @Transactional
        public void disbandGroup(Long projectId) {

            // Find project
             Project currentProject = projectsRepo.findById(projectId).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST, "Project Not found"));
            // Delete memberships
            projectMembersRepo.deleteAllByProject(currentProject);
            // Find project's chat
            Chat currentChat = chatRepo.findByProject(currentProject);

    // Delete messages before deleting chat
    if (currentChat != null) {
        chatMessageRepo.deleteAllByChat(currentChat);

        // Delete chat
        chatRepo.delete(currentChat);
    }

    // Delete the project's uploaded files, then the project
    projectFileServices.deleteAllForProject(currentProject);
    projectsRepo.deleteById(currentProject.getID());
}



    
    
    
    
}
