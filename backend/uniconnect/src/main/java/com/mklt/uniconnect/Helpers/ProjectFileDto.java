package com.mklt.uniconnect.Helpers;

import java.time.LocalDateTime;

import com.mklt.uniconnect.Entities.ProjectFile;

// What the app receives for each file. The field names match the app's ProjectFileModel.
public record ProjectFileDto(
        Long fileId,
        String fileName,
        String contentType,
        long sizeBytes,
        long uploadedById,
        String uploadedByName,
        LocalDateTime uploadedAt) {

    public static ProjectFileDto from(ProjectFile file) {
        String uploaderName = null;
        long uploaderId = 0;
        if (file.getUploadedBy() != null) {
            uploaderId = file.getUploadedBy().getId();
            String first = file.getUploadedBy().getName() == null ? "" : file.getUploadedBy().getName();
            String last = file.getUploadedBy().getSurname() == null ? "" : file.getUploadedBy().getSurname();
            uploaderName = (first + " " + last).trim();
        }
        return new ProjectFileDto(
                file.getId(),
                file.getFileName(),
                file.getContentType(),
                file.getSizeBytes(),
                uploaderId,
                uploaderName,
                file.getUploadedAt());
    }
}
