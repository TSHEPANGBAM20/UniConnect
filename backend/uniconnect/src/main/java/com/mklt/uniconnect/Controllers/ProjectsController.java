package com.mklt.uniconnect.Controllers;

import java.io.IOException;
import java.time.LocalDate;
import java.util.List;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.multipart.MultipartFile;

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

Student student = studentRepo.findById(student_id).orElseThrow(() ->new RuntimeException("Student not found")); //first find student
//Creates a new project
String projectName = project.getProjectName();
String projectDescription = project.getProjectDescription();
LocalDate startDate = project.getStartDate();
LocalDate endDate = project.getEndDate();



//field validation
if (projectName.isEmpty()){
    throw new RuntimeException("Error Project Name cannot be empty!");
}else if (projectDescription.isEmpty()){
    throw new RuntimeException("Error Project Description cannot be empty!");
}else if (startDate == null){
    throw new RuntimeException("Error Project Start Date cannot be empty!");
}else if(endDate == null){
    throw new RuntimeException("Error Project End Date cannot be empty!");
}else{
    
    return projectsServices.createProject(student,projectName, projectDescription, startDate, endDate);
}

}

//add member-Group leader action
@PostMapping("/addMember")

public ProjectMembers addMember(@RequestParam ("projectId") Long projectId,@RequestParam ("email") String email ){
if (email.isEmpty()){
   throw new RuntimeException("Error email Start Date cannot be empty!"); 
}
return projectsServices.addMemberToProject(projectId, email);
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
   throw new RuntimeException("Error email Start Date cannot be empty!"); 
}else if (projectCode == null){
   throw new RuntimeException("Error email Start Date cannot be empty!");
}else{
    return projectsServices.joinProject(projectCode, email);
}

}


//show student projects
    @PostMapping("/showStudentProjects")
    public List <Project> showStudentProjects(@RequestParam("studentId") Long studentId){
        return projectsServices.showStudentProjects(studentId);
    }


}


