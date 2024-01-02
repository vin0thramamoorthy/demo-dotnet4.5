using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http;

namespace Todo.Web.Controllers
{
    [RoutePrefix("Tasks")]
    public class TasksController : ApiController
    {
	    readonly string _filePath = HostingEnvironment.MapPath("~/App_data/");

        [Route("{email}")]
        public IHttpActionResult PostTask(string email, Task task) => withTasks(email, tasks =>
        {
            task.Id = tasks.Any() 
                    ? tasks.Max(t => t.Id) + 1
                    : 1;
            tasks.Add(task);
            return Ok(task);
        });
        [Route("{email}")]
        public IHttpActionResult PutTask(string email, Task task) => withTasks(email, tasks =>
        {
            tasks.Remove(tasks.FirstOrDefault(t => t.Id == task.Id));
            tasks.Add(task);
            return Ok(task);
        });
        [Route("{email}/{id:int}")]
        public IHttpActionResult DeleteTask(string email, int id) => withTasks(email, tasks =>
        {
            tasks.Remove(tasks.FirstOrDefault(t => t.Id == id));
            return Ok();
        });
        [Route("{email}")]
        public IHttpActionResult GetAllTasks(string email) => Ok(getTasks(email));
        [Route("{email}/{id:int}")]
        public IHttpActionResult GeTaskById(string email, int id) => Ok(getTasks(email).FirstOrDefault(t => t.Id == id));
        [Route("{email}/done")]
        public IHttpActionResult GetCompletedTasks(string email) => 
			Ok(getTasks(email).Where(t => t.Done));

        [Route("{email}/pending")]
        public IHttpActionResult GetPendingTasks(string email) => 
			Ok(getTasks(email).Where(t => !t.Done));

        [Route("{email}/overdue")]
        public IHttpActionResult GetOverDueTasks(string email) => 
			Ok(getTasks(email).Where(t => !t.Done && t.DueDate < DateTime.Now));

        string getPath(string email) => _filePath + "/tasks_" + email + ".json";
        List<Task> getTasks(string email)
        {
            var path = getPath(email);
            return !File.Exists(path) 
					? new List<Task>() 
					: JsonConvert.DeserializeObject<List<Task>>(File.ReadAllText(getPath(email)));
        }
        IHttpActionResult withTasks(string email, Func<List<Task>, IHttpActionResult> callback)
        {
            var path = getPath(email);
            var tasks = getTasks(email);
            var result = callback(tasks);
            File.WriteAllText(path, JsonConvert.SerializeObject(tasks));
            return result;
        }
    }

    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public bool Done { get; set; }
    }

    [RoutePrefix("Accounts")]
    public class AccountController : ApiController
    {
        [Route("")]
        public IHttpActionResult GetEmails()
        {
            return Ok(Directory.GetFiles(HostingEnvironment.MapPath("~/App_data/"))
                               .Select(Path.GetFileName)
                               .Select(f => f.Replace("tasks_", "")
                                             .Replace(".json", "")));
        }
    }

	[RoutePrefix("Configuration")]
	public class ConfigurationController : ApiController
	{
		[Route("{key}")]
		public IHttpActionResult GetConfigValue(string key) => Ok(ConfigurationManager.AppSettings[key]);
	}
}