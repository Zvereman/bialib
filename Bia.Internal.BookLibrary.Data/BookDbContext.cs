using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    // todo если не будет работать поиск по объектам в DownloadHistory или Reviews или RentHistory то добавить внешние ключи в OnModelCreating
    /// <summary>
    /// Book library database context.
    /// </summary>
    public class BookDbContext : DbContext
    {

        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
        { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<DownloadHistory> DownloadHistories { get; set; }
        public DbSet<UploadHistory> UploadHistories { get; set; }
        public DbSet<Arrangement> Arrangements { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Ebook> Ebooks { get; set; }
        public DbSet<PaperBook> PaperBooks { get; set; }
        public DbSet<RentHistory> RentHistories { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<UserNotify> UserNotifies { get; set; }
        public DbSet<AdminNotify> AdminNotifies { get; set; }
        public DbSet<IgnoredUser> IgnoredUsers { get; set; }
        public DbSet<RequestBook> RequestBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Reviews)
                .WithOne(r => r.Book)
                .HasForeignKey(r => r.BookId);

            modelBuilder.Entity<Book>()
                .HasMany(p => p.Authors)
                .WithOne(i => i.Book);

            modelBuilder.Entity<Book>()
                .HasMany(p => p.ReqestBooks)
                .WithOne(i => i.Book);

            modelBuilder.Entity<PaperBook>()
                .HasMany(p => p.RentHistory)
                .WithOne(i => i.TakenBook);

            modelBuilder.Entity<Ebook>()
                .HasMany(p => p.DownloadHistory)
                .WithOne(i => i.DownloadedBook);

            modelBuilder.Entity<AppUser>()
                .HasMany(p => p.DownloadHistory)
                .WithOne(i => i.User);

            modelBuilder.Entity<AppUser>()
                .HasMany(p => p.RentHistory)
                .WithOne(i => i.User);

            modelBuilder.Entity<AppUser>()
                .HasMany(i => i.BooksTaken)
                .WithOne(p => p.TakenByUser);

            modelBuilder.Entity<AppUser>()
                .HasMany(i => i.Reviews)
                .WithOne(p => p.RatedBy);

            modelBuilder.Entity<AppUser>()
                .HasMany(p => p.ReqestBooks)
                .WithOne(i => i.AppUser);

            modelBuilder.Entity<AppUser>()
                .HasMany(p => p.UserNotifies)
                .WithOne(u => u.AppUser);

            modelBuilder.Entity<Admin>()
                .HasMany(p => p.AdminNotifies)
                .WithOne(a => a.Admin);

            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorUid });

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(a => a.Authors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(ba => ba.AuthorUid);

            modelBuilder.Entity<BookCategory>()
                .HasKey(c => new { c.BookId, c.CategoryId });

            modelBuilder.Entity<BookCategory>()
                .HasOne(ba => ba.Book)
                .WithMany(a => a.Cathegories)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookCategory>()
                .HasOne(ba => ba.Category)
                .WithMany(a => a.Books)
                .HasForeignKey(ba => ba.CategoryId);

            modelBuilder.Entity<UserNotify>()
                .HasKey(rb => new { rb.Uid, rb.BookId });

            modelBuilder.Entity<UserNotify>()
                .HasOne(un => un.AppUser)
                .WithMany(un => un.UserNotifies)
                .HasForeignKey(un => un.Uid);

            modelBuilder.Entity<RequestBook>()
                .HasKey(rb => new {rb.BookId, rb.AppUserUid });

            modelBuilder.Entity<RequestBook>()
                .HasOne(ba => ba.Book)
                .WithMany(a => a.ReqestBooks)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<RequestBook>()
                .HasOne(ba => ba.AppUser)
                .WithMany(a => a.ReqestBooks)
                .HasForeignKey(ba => ba.AppUserUid);

            //todo https://stackoverflow.com/questions/54985032/ef-core-one-to-one-or-zero-relationship
            modelBuilder.Entity<AppUser>()
                .HasOne(q => q.Ignored)
                .WithOne(q => q.AppUser)
                .HasForeignKey<IgnoredUser>(q => q.AppUserUid);

            modelBuilder.Entity<AppUser>()
                .HasOne(q => q.Arrangement)
                .WithOne(q => q.AppUser)
                .HasForeignKey<Arrangement>(q => q.AppUserUid);

            modelBuilder.Entity<UploadHistory>()
                .HasKey(c => new { c.AppUserUid, c.BookId });

            //Fix readonly prop Discriminator
            modelBuilder.Entity<AppUser>().Property(b => b.Discriminator).Metadata.AfterSaveBehavior = PropertySaveBehavior.Save;
        }
    }
}
