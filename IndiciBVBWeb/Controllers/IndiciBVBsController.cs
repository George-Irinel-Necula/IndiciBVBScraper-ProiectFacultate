using IndiciBVBWeb.Data;
using IndiciBVBWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndiciBVBWeb.Controllers
{
    public class IndiciBVBsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IndiciBVBsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> UpdateIndiciBVB()
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArgument("--headless=new");
                using (var driver = new ChromeDriver(options))
                {
                    driver.Navigate().GoToUrl("https://m.bvb.ro/TradingAndStatistics/Trading/MarketsToday");
                    IList<IWebElement> rows = driver.FindElement(By.CssSelector("#gv.small-table")).FindElements(By.CssSelector("tbody tr"));
                    foreach (var row in rows)
                    {
                        IWebElement nume = row.FindElement(By.CssSelector("td[align='left'] a"));
                        IList<IWebElement> values = row.FindElements(By.CssSelector("td[align='right']"));
                        double valoareUnitara = Convert.ToDouble(values[0].Text.Replace(".", "").Replace(",", "."));
                        double crestereValoare = Convert.ToDouble(values[1].Text.Replace(".", "").Replace(",", "."));
                        double crestereProcent = Convert.ToDouble(values[2].Text.Replace("%", "").Replace(",", "."));
                        DateTime date = DateTime.Now;
                        string sursa = "BVB";
                        IndiciBVB obiect_Db = new IndiciBVB() { Nume = nume.Text, ValoareUnitara = valoareUnitara, CastigValoare = crestereValoare, CastigProcent = crestereProcent, Data = date,Sursa=sursa };
                        _context.IndiciBVB.Add(obiect_Db);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare Indici BVB: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> UpdateIndiciBVB2()
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArgument("--headless=new");
                using (var driver = new ChromeDriver(options))
                {
                    driver.Navigate().GoToUrl("https://www.primet.ro/informatii-piata-raport-zilnic");
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                    var indiciDiv = driver.FindElement(By.CssSelector("div.box-indici.bg-bleumarin"));

                    IList<IWebElement> rows = indiciDiv
                        .FindElement(By.CssSelector("tbody"))
                        .FindElements(By.CssSelector("tr"));

                    foreach (var row in rows.Skip(1))
                    {
                        IWebElement nume = row.FindElement(By.CssSelector("td:nth-child(1)"));
                        IList<IWebElement> values = row.FindElements(By.CssSelector("td.text-right"));


                        if (values.Count >= 3)
                        {
                            double valoareUnitara = Convert.ToDouble(values[0].Text.Replace(".", "").Replace(",", "."));
                            double crestereProcent = Convert.ToDouble(values[1].Text.Replace("%", "").Replace(",", "."));
                            double crestereValoare = Math.Round(valoareUnitara * (crestereProcent / 100), 2);

                            IndiciBVB obiect_Db = new IndiciBVB()
                            {
                                Nume = nume.Text,
                                ValoareUnitara = valoareUnitara,
                                CastigValoare = crestereValoare,
                                CastigProcent = crestereProcent,
                                Data = DateTime.Now,
                                Sursa = "Primet"
                            };

                            _context.IndiciBVB.Add(obiect_Db);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare Indici BVB: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: IndiciBVBs
        public async Task<IActionResult> Index()
        {
            return View(await _context.IndiciBVB.OrderByDescending(d => d.Data).ThenBy(n => n.Nume).ToListAsync());
        }

        // GET: IndiciBVBs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indiciBVB = await _context.IndiciBVB
                .FirstOrDefaultAsync(m => m.Id == id);
            if (indiciBVB == null)
            {
                return NotFound();
            }

            return View(indiciBVB);
        }

        // GET: IndiciBVBs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: IndiciBVBs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nume,ValoareUnitara,CastigValoare,CastigProcent,Data,Sursa")] IndiciBVB indiciBVB)
        {
            if (ModelState.IsValid)
            {
                _context.Add(indiciBVB);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(indiciBVB);
        }

        // GET: IndiciBVBs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indiciBVB = await _context.IndiciBVB.FindAsync(id);
            if (indiciBVB == null)
            {
                return NotFound();
            }
            return View(indiciBVB);
        }

        // POST: IndiciBVBs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nume,ValoareUnitara,CastigValoare,CastigProcent,Data,Sursa")] IndiciBVB indiciBVB)
        {
            if (id != indiciBVB.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(indiciBVB);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IndiciBVBExists(indiciBVB.Id))
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
            return View(indiciBVB);
        }

        // GET: IndiciBVBs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var indiciBVB = await _context.IndiciBVB
                .FirstOrDefaultAsync(m => m.Id == id);
            if (indiciBVB == null)
            {
                return NotFound();
            }

            return View(indiciBVB);
        }

        // POST: IndiciBVBs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var indiciBVB = await _context.IndiciBVB.FindAsync(id);
            if (indiciBVB != null)
            {
                _context.IndiciBVB.Remove(indiciBVB);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IndiciBVBExists(int id)
        {
            return _context.IndiciBVB.Any(e => e.Id == id);
        }
    }
}
