package com.mklt.uniconnect.Controllers;

import java.io.IOException;
import java.time.LocalDate;
import java.util.List;

import org.springframework.core.io.Resource;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.multipart.MultipartFile;
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

    public ProjectsController(ProjectsServices projectsServices,StudentRepo studentRepo) {
        this.projectsServices = projectsServices;
        this.studentRepo = studentRepo;
    }

@PostMapping("/createProject")
public Project createProject(@RequestBody Project project, @RequestParam("id") Long student_id){

Student student = studentRepo.findById(student_id).orElseThrow(() ->new ResponseStatusException(HttpStatus.BAD_REQUEST,"Student not found")); //first find student
//Creates a new project
String projectName = project.getProjectName();
String projectDescription = project.getProjectDescription();
LocalDate startDate = project.getStartDate();
LocalDate endDate = project.getEndDate();



//field validation
if (projectName.isEmpty()){
    throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Error Project Name cannot be empty!");
}else if (projectDescription.isEmpty()){
    throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Error Project Description cannot be empty!");
}else if (startDate == null){
    throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Error Project Start Date cannot be empty!");
}else if(endDate == null){
    throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Error Project End Date cannot be empty!");
}else{
    
    return projectsServices.createProject(student,projectName, projectDescription, startDate, endDate);
}

}

//add member-Group leader action
@PostMapping("/addMember")

public ProjectMembers addMember(@RequestParam ("projectId") Long projectId,@RequestParam ("email") String email ){
if (email.isEmpty()){
   throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Error email Start Date cannot be empty!"); 
}
return projectsServices.addMemberToProject(projectId, email);
}

//method to getProject Details
@PostMapping("/getProjectInfo")
public Project getProjectInfo(@RequestParam("projectId")Long projectId){
    return projectsServices.GetProjectDetails(projectId);
}


@GetMapping("/downloadProject/{projectId}/download")
public ResponseEntity<Resource> downloadProjectFile(@PathVariable Long projectId) {
    return projectsServices.downloadProjectFile(projectId);
}




//method to upload file
@PostMapping("/uploadProject")
public Project uploadProjectFile(@RequestParam("projectId")Long projectId, @RequestParam("projectFile")MultipartFile projectFile) throws IOException{
    return projectsServices.updateProjectFile(projectId,projectFile) ;
}

//Join by projectCode
@PostMapping("/joinByCode")
public ProjectMembers joinByProjectCode(@RequestParam("projectCode") Integer projectCode,@RequestParam ("email") String email) {
if (email.isEmpty()){
   throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Error Start Date cannot be empty!"); 
}else if (projectCode == null){
   throw new ResponseStatusException(HttpStatus.BAD_REQUEST,"Error Start Date cannot be empty!");
}else{
    return projectsServices.joinProject(projectCode, email);
}

}


//show student projects
    @PostMapping("/showStudentProjects")
    public List <Project> showStudentProjects(@RequestParam("studentId") Long studentId){
        return projectsServices.showStudentProjects(studentId);
    }

@PostMapping("/showStudentsInProject")
public List <ProjectMembers> showStudentsInProject(@RequestParam("projectId") Long projectId){
return projectsServices.showStudentsInProject(projectId);
}

//remove member from group 
@DeleteMapping ("/removeMemberFromgroup")
  public void removeStudentFromGroup(@RequestParam("studentEmail") String studentEmail, @RequestParam("projectId") Long projectId ){
    projectsServices.removeStudentFromGroup(studentEmail,projectId);
  }

  //disband group 
@DeleteMapping ("/disbandGroup")
  public void disbandGroup( @RequestParam("projectId") Long projectId ){
    projectsServices.disbandGroup(projectId);
  }

}








