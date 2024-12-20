﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HomeBudget.Data;
using HomeBudget.Models;
using Microsoft.AspNetCore.Authorization;
using Azure.Identity;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace HomeBudget.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: User
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [AllowAnonymous]
        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserName,FirstName,LastName,Email,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "This name is already taken.");
                }
                else if (existingEmail != null)
                {
                    ModelState.AddModelError(string.Empty, "This email is already used.");
                }
                else
                {
                    user.Password = UserHelper.HashSHA256(user.Password);
                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    if (user.Email == "admin@gmail.com")
                    {
                        user.IsAdmin = true;
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    var workCategory = new Category
                    {
                        CategoryName = "Work",
                        Icon = "&#128184;",
                        Type = "Income",
                        UserId = user.Id
                    };
                    _context.Add(workCategory);

                    await _context.SaveChangesAsync();
                    await SendWelcomeEmail(user.Email, user.FirstName);

                    TempData["ToastrMessage"] = "User has been created successfully";
                    TempData["ToastrType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(user);
        }

        private async Task SendWelcomeEmail(string email, string firstName)
        {
            var smtpSettings = _configuration.GetSection("Smtp").Get<SmtpSettings>();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSettings.SenderName, smtpSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Welcome to Home Budget";

            // Body text
            var body = new TextPart("html")
            {
                Text = $"Hello {firstName},<br/><br/>Your account has been created successfully. Welcome to our system!<br/><br/>Best regards,<br/>Home Budget Team"
            };

            // Load the image and create an attachment
            var attachment = new MimePart("image", "jpeg")
            {
                Content = new MimeContent(System.IO.File.OpenRead("wwwroot/images/HOMEBUDGET.jpg")),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "welcome.jpg"
            };

            // Create a multipart/mixed container to hold the message text and the attachment
            var multipart = new Multipart("mixed")
            {
                body,
                attachment
            };

            message.Body = multipart;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpSettings.Server, smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserName,FirstName,LastName,Email,Password")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
