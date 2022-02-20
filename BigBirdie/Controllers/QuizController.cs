using BigBirdie.Hubs;
using BigBirdie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

        [HttpGet]
        public IActionResult Join(string id = "")
        {
            if (!this.QuizService.SessionExists(id))
                return RedirectToAction("Index", "Home");

            return View((object)id);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Join()
        {
            if (this.QuizService == null) 
                return RedirectToAction("Index", "Home");

            // génère un id de groupe
            string id;
            do {
                id = this.RandomString(5);
            } while (!this.QuizService.AddSession(id, HttpContext.User.Identity?.Name ?? string.Empty));

            return RedirectToAction("Join", new { id });
        }

        /// <summary>
        /// Génère une chaîne de texte aléatoire
        /// </summary>
        /// <param name="length">taille de la chaîne</param>
        /// <returns></returns>
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
