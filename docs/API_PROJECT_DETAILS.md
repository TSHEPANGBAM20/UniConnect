# API contract: Project Details, Members and Join-by-Code

For the **backend pair**. Written by the frontend side; every endpoint below is also commented in
`frontend/UniConnect.MAUI/Services/ApiService.cs` (look for the `// ENDPOINT:` lines).

**Convention going forward:** every ApiService method gets an `// ENDPOINT:` comment with the HTTP method + path,
whether the backend side is `EXISTS`, `NEW` or `MISMATCH`, the request body/params, and the error codes.

## Roles and permissions (the UI hides buttons, but the BACKEND must enforce this)

Roles already exist: `ProjectRoles { LEADER, MEMBER }`. There is no token yet, so the frontend sends the logged-in
student's id as `requesterId` / `studentId`. The backend must look up that student's role **in that project**
(`project_members`) and reject anything not allowed with **403**.

| Action | Team Lead (LEADER) | Team Member (MEMBER) |
|---|---|---|
| View project details, code and members | yes | yes |
| Add a member by email | yes | **403** |
| Remove another member | yes (not themselves) | **403** |
| Leave the project (remove self) | **400** (must disband) | yes |
| Disband (delete) the project | yes | **403** |
| Join with a code | n/a | any student with a valid code |
| View / download / upload project files | yes | yes |
| Delete a project file | yes (any file) | only files they uploaded, otherwise **403** |

## Endpoints

### 1. GET `/projects/{projectId}/details?studentId={me}`  - NEW
Section 1 of the screen. `myRole` is the role of `studentId` in this project.
```json
200 { "projectId": 7, "projectName": "UniConnect", "projectDescription": "...",
      "startDate": "2026-09-14", "endDate": "2026-10-30", "status": "INPROGRESS",
      "projectFileName": "UniConnect_Brief.pdf", "projectCode": 482913, "myRole": "LEADER" }
```
Errors: `404` project not found, `403` studentId is not a member. Use a DTO: the `Project` entity has no `getStatus()`,
serializes its id as `id` (from `getID()`), and `getProjectCode()` returns `int` (NPE if null).

### 2. GET `/projects/{projectId}/members`  - NEW
Section 2. **Never include the password hash.**
```json
200 [ { "studentId": 1, "name": "Tshepang", "surname": "Bambo", "email": "...", "role": "LEADER" },
      { "studentId": 3, "name": "Koketso",  "surname": "Fakude", "email": "...", "role": "MEMBER" } ]
```

### 3. POST `/projects/addMember?projectId=&email=&requesterId=`  - EXISTS, needs changes
Today it has **no leader check** (any caller can add anyone). Add `requesterId` and require LEADER (`403`).
Return `404` for an unknown email and `409` if already a member (currently a 500 RuntimeException).

### 4. DELETE `/projects/{projectId}/members/{studentId}?requesterId=`  - NEW
Allowed when (requester is LEADER and `studentId` is not the leader) **or** (`studentId == requesterId` and role is MEMBER).
The leader can never be removed or leave: `400 { "message": "The Team Lead can't leave. Disband the project instead." }`.

### 5. DELETE `/projects/{projectId}?requesterId=`  - NEW (service `deleteProject` exists, no endpoint)
LEADER only. Must delete the project's `project_members` rows (and its tasks / chat) first, or cascade, otherwise the
foreign keys will reject the delete.

### 6. POST `/projects/joinByCode?projectCode=&email=`  - EXISTS, has bugs
Used by "Join with Code" on the Projects screen, then the app opens that project. The frontend accepts either
`{ "projectId": 7 }` (preferred) or the current `{ "project": { "id": 7 } }`.
- **No project with that code:** `findByProjectCode` returns `null`, then `existsByStudentAndProject(student, null)`.
  Return `404 { "message": "No project found for that code" }` instead.
- **Unknown email:** `findByStudentEmail` returns `null`. Return `404`.
- **Already a member:** return `409` (currently a 500).
- **Do not return the `ProjectMembers` entity.** It nests the whole `Student`, including the password hash. Return a DTO.

## Project files (documents and pictures) - section 2 of the screen

The Project Details screen has a **Project files** section between the details and the members. Any member can upload a
document or picture; everyone in the project can open them. This is **separate** from the existing
`POST /projects/uploadProject`, which stores a single "project file" on the `Project` row (a new upload replaces it).
Keep that endpoint for the main brief; these new endpoints hold many files per project.

### 7. GET `/projects/{projectId}/files?studentId={me}`  - NEW
Any member (403 otherwise). Newest first.
```json
200 [ { "fileId": 12, "fileName": "Login_wireframe.png", "contentType": "image/png", "sizeBytes": 1310720,
        "uploadedById": 2, "uploadedByName": "Lebogang Malatsi", "uploadedAt": "2026-10-17T09:30:00" } ]
```

### 8. POST `/projects/{projectId}/files?uploaderId={me}`  - NEW
`multipart/form-data` with **one part named `file`**. Any member (403 otherwise).
```json
201 { "fileId": 12, "fileName": "Login_wireframe.png", "contentType": "image/png", "sizeBytes": 1310720,
      "uploadedById": 2, "uploadedByName": "Lebogang Malatsi", "uploadedAt": "2026-10-17T09:30:00" }
```
- **Size:** max **10 MB**. Spring's default is only **1 MB**, so add to `application.properties`:
  `spring.servlet.multipart.max-file-size=10MB` and `spring.servlet.multipart.max-request-size=10MB`.
  Too big -> `413`.
- **Type:** allow only `.pdf .doc .docx .ppt .pptx .xls .xlsx .txt .zip .png .jpg .jpeg` (the app offers the same list, but
  the backend must check as well). Anything else -> `415`. Never allow executables or scripts.
- **File name:** never trust the client name. Strip any path (`../`) and store as `uploads/projects/{projectId}/files/{uuid}_{safeName}`.
  (The existing `updateProjectFile` uses `getOriginalFilename()` directly, which allows path traversal. Fix it the same way.)
- Save the metadata in a new table (see below). Do not store file bytes in the database.

### 9. GET `/projects/{projectId}/files/{fileId}/download?studentId={me}`  - NEW
Returns the raw bytes with `Content-Type` and `Content-Disposition: attachment; filename="..."`.
Any member (403 otherwise); `404` if the row or the file on disk is missing.

### 10. DELETE `/projects/{projectId}/files/{fileId}?requesterId={me}`  - NEW
Allowed for the student who uploaded the file **or** the project's LEADER. Anyone else -> `403`.
Delete the file from disk as well as the row. When a project is disbanded (endpoint 5) delete its files too.

### Suggested table and Spring outline
```java
@Entity @Table(name = "project_files")
public class ProjectFile {
    @Id @GeneratedValue(strategy = GenerationType.IDENTITY) private Long id;
    @ManyToOne private Project project;
    @ManyToOne private Student uploadedBy;
    private String fileName;      // original name shown in the app
    private String storedPath;    // where it lives on disk (never sent to the client)
    private String contentType;
    private long sizeBytes;
    private LocalDateTime uploadedAt;
}

// in ProjectsController (@RequestMapping("/projects"))
@GetMapping("/{projectId}/files")
public List<ProjectFileDto> listFiles(@PathVariable Long projectId, @RequestParam Long studentId) { ... }

@PostMapping("/{projectId}/files")
public ResponseEntity<ProjectFileDto> upload(@PathVariable Long projectId, @RequestParam Long uploaderId,
                                             @RequestParam("file") MultipartFile file) throws IOException { ... } // 201

@GetMapping("/{projectId}/files/{fileId}/download")
public ResponseEntity<Resource> download(@PathVariable Long projectId, @PathVariable Long fileId,
                                         @RequestParam Long studentId) { ... }

@DeleteMapping("/{projectId}/files/{fileId}")
public ResponseEntity<Void> deleteFile(@PathVariable Long projectId, @PathVariable Long fileId,
                                       @RequestParam Long requesterId) { ... } // 204
```
The DTO returns `fileId, fileName, contentType, sizeBytes, uploadedById, uploadedByName, uploadedAt`, never `storedPath`.

## Other mismatches the frontend hit while building this

| Frontend needs | Backend today |
|---|---|
| Project list item `{ projectId, projectName, endDate, role }` | `POST /projects/showStudentProjects?studentId=` returns the raw `Project` (`id`, no role) |
| Login returns the student's id | `Student.getId()` exists, so `id` is returned. The password hash is returned too |
| Error body `{ "message": "..." }` with proper status codes | `RuntimeException` becomes a 500 with no message. A `@ControllerAdvice` fixes this |
| Task / chat paths | See the `MISMATCH` and `NOT BUILT` comments in `ApiService.cs` |

Project **status** values used by the UI: `INPROGRESS`, `COMPLETED` (exposed via the details DTO).
Project **code** is the existing 6-digit `projectCode` (100000-999999).
