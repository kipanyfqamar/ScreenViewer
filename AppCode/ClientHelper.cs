using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using ScreenViewer.Data;
using ScreenViewer.Models;

namespace ScreenViewer
{
    public static class ClientHelper
    {
        private static ScreenPlayClientsEntities db = new ScreenPlayClientsEntities();
        public static string GetClientIdByUserID(string userId)
        {
            return db.ScriptClientUsers.Where(x => x.UserID == userId).FirstOrDefault().ScriptClientID;
        }

        public static bool ValidateUserIdAndClientId(string userId, string clientId)
        {
            return db.ScriptClientUsers.Any(x => x.UserID == userId && x.ScriptClientID == clientId);
        }

        public static List<UserResource> GetAllUserByClientID(string clientId)
        {
            var users = (from cu in db.ScriptClientUsers
                         join u in db.AspNetUsers on cu.UserID equals u.Id
                         where cu.ScriptClientID == clientId
                         select new UserResource
                         {
                             UserValue = cu.UserID,
                             UserName = u.FirstName + " " + u.LastName,
                             UserColor = ""
                         }).ToList<UserResource>();

            return users;

        }

        public static string GetUserNameByID(string Id)
        {
            var user = db.AspNetUsers.Where(x=> x.Id == Id).First();
            if (user != null)
                return string.Format("{0} {1}", user.FirstName, user.LastName);
            else
                return string.Empty;
        }

    }

    public static class ScriptHelper
    {
        private static ScreenPlayEntities db = new ScreenPlayEntities();

        public static List<TaskTypeResource> GetTaskTypeByClientID(string clientId)
        {

            List<TaskTypeResource> resource = (from cu in db.ScriptTasks

                                               where cu.ClientID == clientId
                                               select new TaskTypeResource
                                               {
                                                   TaskName = cu.TaskType,
                                                   TaskValue = cu.TaskType,
                                                   TaskColor = cu.TaskColor
                                               }).Distinct().ToList<TaskTypeResource>();

            return resource;

        }
    }
}