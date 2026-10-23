# UniConnect — API Contract (Draft v0.1)

This is the agreed contract between the **C# / .NET MAUI frontend** and the **Java / Spring Boot backend**.
Nobody changes an endpoint shape without flagging it to the whole team — treat this file as the source of truth, and update it in the same PR as any backend change that affects it.

- Base URL (local dev): `http://localhost:8080/api/v1`
- Format: JSON over HTTPS (HTTP in local dev)
- Auth: `Authorization: Bearer <jwt>` header on every request except `/auth/register` and `/auth/login`
- Dates: ISO 8601 (`"2026-09-22T14:00:00Z"`)

---

## Auth

### POST /auth/register
FR: User Registration and Login

Request:
```json
{
  "name": "Tshepang",
  "surname": "Bambo",
  "email": "EDUV4855006@vossie.net",
  "password": "plaintext-sent-over-https"
}
```
Response `201`:
```json
{ "userId": 1, "email": "EDUV4855006@vossie.net" }
```

### POST /auth/login
Request:
```json
{ "email": "EDUV4855006@vossie.net", "password": "..." }
```
Response `200`:
```json
{ "token": "jwt-here", "userId": 1, "name": "Tshepang" }
```
Errors: `401` invalid credentials.

---

## Projects

### POST /projects
FR: Project Creation and Management. Creator is auto-assigned role `TEAM_LEAD`.

Request:
```json
{
  "projectName": "UniConnect",
  "projectDescription": "Final year project",
  "startDate": "2026-09-01",
  "endDate": "2026-11-30"
}
```
Response `201`:
```json
{ "projectId": 10, "projectName": "UniConnect", "role": "TEAM_LEAD" }
```

### GET /projects
Returns all projects the logged-in user belongs to.
```json
[
  { "projectId": 10, "projectName": "UniConnect", "role": "TEAM_LEAD" }
]
```

### GET /projects/{projectId}
Full project detail: description, dates, file link, member list.

### PUT /projects/{projectId}
Update project details. `TEAM_LEAD` only.

### DELETE /projects/{projectId}
Disband project. `TEAM_LEAD` only.

### POST /projects/{projectId}/members
FR: Role assignation. `TEAM_LEAD` only.
```json
{ "email": "member@vossie.net", "role": "TEAM_MEMBER" }
```

### DELETE /projects/{projectId}/members/{userId}
Remove a team member. `TEAM_LEAD` only.

### POST /projects/{projectId}/file
Multipart upload — FR: Project file sharing/attachments. Returns a file URL stored against the project.

---

## Tasks

### POST /projects/{projectId}/tasks
FR: Task creation and Management, Task allocation. `TEAM_LEAD` only for assigning to others.
```json
{
  "taskTitle": "Design login screen",
  "taskDescription": "...",
  "priority": "HIGH",
  "assignedTo": 3,
  "dueDate": "2026-09-28"
}
```
Response `201` includes `taskId`, `status: "TODO"`.

### GET /projects/{projectId}/tasks
Returns all tasks for the project (used for the Task Board / Mock Up 3).

### GET /tasks/{taskId}
Single task detail.

### PUT /tasks/{taskId}/status
FR: Task creation and Management (mark complete).
```json
{ "status": "IN_PROGRESS" }
```
Valid values: `TODO`, `IN_PROGRESS`, `DONE`.

### DELETE /tasks/{taskId}
Remove a task entirely.

---

## Calendar

### GET /projects/{projectId}/calendar
FR: Calendar for deadlines. Returns task due dates + project milestones/deadlines in one list for the calendar screen.
```json
[
  { "type": "TASK", "refId": 5, "title": "Design login screen", "dueDate": "2026-09-28" },
  { "type": "PROJECT_DEADLINE", "refId": 10, "title": "UniConnect", "dueDate": "2026-11-30" }
]
```

---

## Chat

### GET /projects/{projectId}/chat
FR: Project based chat. Returns the chat thread (list of messages) for the project.

### POST /projects/{projectId}/chat/messages
```json
{ "message": "@Lebo I have a problem with task 3" }
```
Response `201` includes `messageId`, `senderId`, `timestamp`.

Note: for real-time delivery (NFR: Reliability — real-time communication), plan to add a WebSocket endpoint (`/ws/chat/{projectId}`) once basic REST chat works — don't block MVP on this.

---

## Notifications

### GET /notifications
FR: Deadline notifications. Returns unread notifications for the logged-in user across all their projects.
```json
[
  { "notificationId": 1, "message": "Task 'Design login screen' is due in 2 days", "isRead": false }
]
```

### PUT /notifications/{id}/read
Marks a notification as read.

---

## Standard error shape (all endpoints)

```json
{ "error": "VALIDATION_ERROR", "message": "email is required" }
```
Common status codes: `400` bad input, `401` not authenticated, `403` not authorized for this action (e.g. non-lead trying to remove a member), `404` not found.

---

## Open questions for the team to confirm before backend work starts

1. Password hashing algorithm (bcrypt recommended) — Java pair to confirm library.
2. JWT expiry time and refresh strategy.
3. Whether `/chat` stays plain REST (poll-based) for MVP or needs WebSockets from day one — recommend REST first, upgrade later if time allows.
4. File upload size limit and storage location (local disk vs cloud bucket) for `/projects/{id}/file`.
