package com.mklt.uniconnect.Controllers;

import java.io.IOException;
import java.util.List;

import org.springframework.core.io.Resource;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.multipart.MultipartFile;

import com.mklt.uniconnect.Helpers.ProjectFileDto;
import com.mklt.uniconnect.Services.ProjectFileServices;

// Project files: /projects/{projectId}/files
@RestController
@RequestMapping("/projects/{projectId}/files")
public class ProjectFileController {

    private final ProjectFileServices projectFileServices;

    public ProjectFileController(ProjectFileServices projectFileServices) {
        this.projectFileServices = projectFileServices;
    }

    // GET /projects/{projectId}/files?studentId=  -> every file in the project, newest first
    @GetMapping
    public List<ProjectFileDto> listFiles(@PathVariable Long projectId,
                                          @RequestParam("studentId") Long studentId) {
        return projectFileServices.listFiles(projectId, studentId);
    }

    // POST /projects/{projectId}/files?studentId=  (form field "projectFile") -> the new file record
    @PostMapping
    public ProjectFileDto uploadFile(@PathVariable Long projectId,
                                     @RequestParam("studentId") Long studentId,
                                     @RequestParam("projectFile") MultipartFile projectFile) throws IOException {
        return projectFileServices.uploadFile(projectId, studentId, projectFile);
    }

    // GET /projects/{projectId}/files/{fileId}/download?studentId=
    @GetMapping("/{fileId}/download")
    public ResponseEntity<Resource> downloadFile(@PathVariable Long projectId,
                                                 @PathVariable Long fileId,
                                                 @RequestParam("studentId") Long studentId) {
        return projectFileServices.downloadFile(projectId, fileId, studentId);
    }

    // DELETE /projects/{projectId}/files/{fileId}?requesterId=  -> uploader or Team Lead only
    @DeleteMapping("/{fileId}")
    public void deleteFile(@PathVariable Long projectId,
                           @PathVariable Long fileId,
                           @RequestParam("requesterId") Long requesterId) {
        projectFileServices.deleteFile(projectId, fileId, requesterId);
    }
}
