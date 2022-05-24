using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BigBirdie.Models;
using BigBirdie.QuizzDB;
using Newtonsoft.Json;
using System.Reflection;

namespace BigBirdie.Controllers
{
    public class QuizzManagerController : Controller
    {
        private readonly QuizzDbContext _context;

        public QuizzManagerController()
        {
            _context = new QuizzDbContext();
        }

        // GET: QuizzManager
        public async Task<IActionResult> Index()
        {
            return _context.quizzItem != null ? 
                        View(await _context.quizzItem.ToListAsync()) :
                        Problem("Entity set 'QuizzDbContext.quizzItem'  is null.");
        }

        // GET: QuizzManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.quizzItem == null)
            {
                return NotFound();
            }

            var quizzItem = await _context.quizzItem
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quizzItem == null)
            {
                return NotFound();
            }

            return View(quizzItem);
        }

        // GET: QuizzManager/Create
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            //choix language à implémenter
            var serializer = new JsonSerializer();
            var OpenQuizzDBItem = new QuizItem();
            using (Stream s = file.OpenReadStream())
            using (StreamReader sr = new StreamReader(s))
            using (JsonTextReader js = new JsonTextReader(sr))
            {
                OpenQuizzDBItem = serializer.Deserialize<QuizItem>(js);


                foreach (PropertyInfo lang in OpenQuizzDBItem.CategorieNomSlogan.GetType().GetProperties())
                {
                    var cnsl = (CategorieNomSloganLang)lang.GetValue(OpenQuizzDBItem.CategorieNomSlogan);

                    var ql = (QuizzLang)typeof(Quizz).GetProperty(lang.Name).GetValue(OpenQuizzDBItem.Quizz);

                    //var cns = (CategorieNomSlogan)typeof(CategorieNomSlogan).GetProperty(lang.Name).GetValue(cnsl);

                    foreach (PropertyInfo diff in ql.GetType().GetProperties())
                    {
                        var li = (List<Item>)diff.GetValue(ql);

                        foreach (Item item in li)
                        {
                            if (!QuizzItemExists(item.Question))
                            {
                                _context.Add(new QuizzItem()
                                {
                                    Lang = lang.Name,
                                    Theme = cnsl.Nom,
                                    Question = item.Question,
                                    Rep = item.Reponse,
                                    prop1 = item.Propositions.ElementAtOrDefault(0),
                                    prop2 = item.Propositions.ElementAtOrDefault(1),
                                    prop3 = item.Propositions.ElementAtOrDefault(2),
                                    prop4 = item.Propositions.ElementAtOrDefault(3),
                                    Anecdote = item.Anecdote,
                                    Wiki = "",
                                    Niveau = OpenQuizzDBItem.Difficulte
                                });
                            }
                        }
                    }
                }


                /*
                foreach (QuizItem item in items)
                {
                    if (!QuizzItemExists(item.Question))
                    {
                        _context.Add(new QuizzItem()
                        {
                            Lang = "fr",
                            Theme = "",
                            Question = item.Question,
                            Rep = item.Reponse,
                            prop1 = item.Propositions.ElementAtOrDefault(0),
                            prop2 = item.Propositions.ElementAtOrDefault(1),
                            prop3 = item.Propositions.ElementAtOrDefault(2),
                            prop4 = item.Propositions.ElementAtOrDefault(3),
                            Anecdote = item.Anecdote,
                            Wiki = "",
                            Niveau = String.IsNullOrEmpty(item.Difficulte) ? "" : item.Difficulte
                        });
                    }
                }
                */
                //items = (List<QuizItem>)serializer.Deserialize(js);
            }
            //Process File & Do Operations
            await _context.SaveChangesAsync();
            return View();
        }

        // POST: QuizzManager/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Lang,Theme,Question,prop1,prop2,prop3,prop4,Rep,Niveau,Anecdote,Wiki")] QuizzItem quizzItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quizzItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quizzItem);
        }

        // GET: QuizzManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.quizzItem == null)
            {
                return NotFound();
            }

            var quizzItem = await _context.quizzItem.FindAsync(id);
            if (quizzItem == null)
            {
                return NotFound();
            }
            return View(quizzItem);
        }

        // POST: QuizzManager/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Lang,Theme,Question,prop1,prop2,prop3,prop4,Rep,Niveau,Anecdote,Wiki")] QuizzItem quizzItem)
        {
            if (id != quizzItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quizzItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizzItemExists(quizzItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quizzItem);
        }

        // GET: QuizzManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.quizzItem == null)
            {
                return NotFound();
            }

            var quizzItem = await _context.quizzItem
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quizzItem == null)
            {
                return NotFound();
            }

            return View(quizzItem);
        }

        // POST: QuizzManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.quizzItem == null)
            {
                return Problem("Entity set 'QuizzDbContext.quizzItem'  is null.");
            }
            var quizzItem = await _context.quizzItem.FindAsync(id);
            if (quizzItem != null)
            {
                _context.quizzItem.Remove(quizzItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuizzItemExists(int id)
        {
          return (_context.quizzItem?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool QuizzItemExists(string s)
        {
            return (_context.quizzItem?.Any(e => e.Question == s)).GetValueOrDefault();
        }
    }
}
