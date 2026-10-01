using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Infrastructure
{
    public class DBQueries
    {
        #region Stored Procedures
        public const string AddUser = "usp_Add_User";
        #endregion

        #region Queries
        public const string CheckUserExists = "SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WITH(NOLOCK) WHERE UserName = @UserName) THEN 1 ELSE 0 END AS UserName, CASE WHEN EXISTS (SELECT 1 FROM Users WITH(NOLOCK) WHERE Email = @Email) THEN 1 ELSE 0 END AS Email";
        #endregion
    }
}
