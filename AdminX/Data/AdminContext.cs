﻿using Microsoft.EntityFrameworkCore;
using AdminX.Models;


namespace AdminX.Data
{
    public class AdminContext : DbContext //The CLinicalContext class is the data context for all clinical related data.
    {
        public AdminContext(DbContextOptions<AdminContext> options) : base(options) { }

        public DbSet<ActivityType> ActivityType { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<ListStatusAdmin> ListStatusAdmin { get; set; }
        public DbSet<ListDisease> ListDiseases { get; set; }

    }
}
