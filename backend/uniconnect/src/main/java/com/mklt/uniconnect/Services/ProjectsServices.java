package com.mklt.uniconnect.Services;

import java.io.IOException;
import java.nio.file.Paths;
import java.time.LocalDate;

import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import com.mklt.uniconnect.Entities.Project;
import com.mklt.uniconnect.Repositories.ProjectsRepo;


@Service 
public class ProjectsServices {

    private ProjectsRepo projectsRepo;

    public ProjectsServices(ProjectsRepo projectsRepo) {
        this.projectsRepo = projectsRepo;
    }

    //method to create project

    public Project createProject(String projectName,String projectDescription, LocalDate startDate, LocalDate endDate){
        //checks if a project of that name exists
        boolean projectExists = projectsRepo.existsByProjectName(projectName);

        if (projectExists){
            throw new RuntimeException("A project of that name already exists.");
        }else{
            //create a new project and save it to db
            Project newProject = new Project(projectName, projectDescription, startDate, endDate);

            //save to db
            projectsRepo.save(newProject);

            return newProject;

        }
        
    }

    //method to edit the file upload

    public Project updateProjectFile(Long id, MultipartFile projectFile) throws IOException{
        Project project = projectsRepo.findById(id).orElseThrow(() -> new RuntimeException("Project not found"));
        id = project.getID();

        //getting the actual file name
        String fileName = projectFile.getOriginalFilename();
        String filePath = "uploads/projects/" + id +"/" + fileName; //constructing the file path

        //set the project file
        project.setProjectFile(filePath); //setting file path in the entity

        //write a file to folder
        projectFile.transferTo(Paths.get(filePath)); //file goes to folder

        //save project
        projectsRepo.save(project);
        return project;
    }

    //method to delete the project
    public void deleteProject(Long id){
        Project project = projectsRepo.findById(id).orElseThrow(() -> new RuntimeException("Project Not found"));

        //delete the project
        projectsRepo.deleteById(id);
    }  
}
