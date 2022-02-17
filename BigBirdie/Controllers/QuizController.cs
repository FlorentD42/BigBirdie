using BigBirdie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BigBirdie.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly QuizService QuizService;

        public QuizController(QuizService quizService)
        {
            this.QuizService = quizService;
        }

        public IActionResult Index(string id = "")
        {
            if (this.QuizService == null)
                return RedirectToAction("Index", "Home");

            if (!this.QuizService.AddUser(id, HttpContext.User.Identity?.Name ?? string.Empty))
                return RedirectToAction("Index", "Home");

            return View((object)id);
        }

        [HttpGet]
        public IActionResult Join(string id = "")
        {
            if (this.QuizService == null)
                return RedirectToAction("Index", "Home");

            if (!this.QuizService.SessionExists(id))
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", new {id = id});
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create()
        {
            if (this.QuizService == null) 
                return RedirectToAction("Index", "Home");

            // génère un id de groupe
            string id;
            do {
                id = this.RandomString(5);
            } while (!this.QuizService.AddSession(id, HttpContext.User.Identity?.Name ?? string.Empty));

            return RedirectToAction("Index", new { id = id });
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor((chars.Length - 1) * flt));
                letter = chars[shift];
                builder.Append(letter);
            }
            return builder.ToString();
        }
    }
}
