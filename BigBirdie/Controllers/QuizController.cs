using BigBirdie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BigBirdie.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly QuizService _quizService;
        public QuizController(QuizService quizService)
        {
            this._quizService = quizService;
        }

        public IActionResult Index(string id = "")
        {
            if (!this._quizService.Sessions.Contains(id))
                return RedirectToAction("Index", "Home");

            return View((object)id);
        }

        [HttpGet]
        public IActionResult Join(string id = "")
        {
            if (!this._quizService.Sessions.Contains(id))
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", new {id = id});
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create()
        {
            // génère un id de groupe
            string id = this.RandomString(5);

            if (this._quizService == null) 
                return RedirectToAction("Index", "Home");

            this._quizService.Sessions.Add(id);

            //return View("Index", id);
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
