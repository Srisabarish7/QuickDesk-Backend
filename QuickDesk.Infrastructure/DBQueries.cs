using QuickDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickDesk.Infrastructure
{
    public class DBQueries
    {
        #region Stored Procedures
        public const string AddUser = "usp_Add_User";
        public const string CreateJob = "usp_Job_Create";
        public const string GetNotes = "usp_GetNotes";
        public const string GetJobs = "usp_GetJobDetails";
        #endregion

        #region Queries
        public const string CheckUserExists = "SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WITH(NOLOCK) WHERE UserName = @UserName) THEN 1 ELSE 0 END AS UserName, CASE WHEN EXISTS (SELECT 1 FROM Users WITH(NOLOCK) WHERE Email = @Email) THEN 1 ELSE 0 END AS Email";

        public const string CheckValidUser = "SELECT UserName, PasswordHash FROM Users WITH(NOLOCK) WHERE UserName = @UserName";
        public const string GetUserByUsername = "SELECT UserId, UserName, FirstName, LastName, Email, IsActive FROM Users WITH(NOLOCK) WHERE UserName = @UserName";
        public const string CheckUserExist = "SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WITH(NOLOCK) WHERE UserId = @UserId) THEN 1 ELSE 0 END AS UserExists";

        public const string GetJobById = """           
            SELECT 
                jb.JobId,             
                jt.JobName, 
                us.Username, 
                js.JobStatus, 
                jb.CreatedAt, 
                jb.ScheduledAt, 
                jb.Progress, 
                jb.StartedAt, 
                jb.CompletedAt, 
                jb.ErrorMessage, 
                el.Recipient, 
                el.Subject, 
                el.Title, 
                el.Description 
            FROM Jobs jb WITH(NOLOCK)
            INNER JOIN JobTypes jt WITH(NOLOCK) ON jt.JobTypeId = jb.JobTypeId
            INNER JOIN JobStatus js WITH(NOLOCK) ON js.JobStatusId = jb.JobStatusId
            INNER JOIN Users us WITH(NOLOCK) ON us.UserId = jb.UserId
            LEFT JOIN EmailLogs el WITH(NOLOCK) ON el.JobId = jb.JobId
            WHERE jb.UserId = @UserId AND jb.JobId = @JobId            
            """;
        #endregion
    }
}
