﻿using BUDZETdomowy.Models;
using Microsoft.EntityFrameworkCore;

namespace BUDZETdomowy.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected ApplicationDbContext() { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
