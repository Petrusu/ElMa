using System;
using System.Collections.Generic;
using ElMaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ElMaAPI.Context;

public partial class AryvulsiContext : DbContext
{
    public AryvulsiContext()
    {
    }

    public AryvulsiContext(DbContextOptions<AryvulsiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Bbk> Bbks { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookAuthor> BookAuthors { get; set; }

    public virtual DbSet<BookEditor> BookEditors { get; set; }

    public virtual DbSet<BookTheme> BookThemes { get; set; }

    public virtual DbSet<Editor> Editors { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Publicationplase> Publicationplases { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=balarama.db.elephantsql.com;Database=aryvulsi;Username=aryvulsi;password=cOsPdJ9RXqYGaicLSf_DoYih0PPc2LC4");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("btree_gin")
            .HasPostgresExtension("btree_gist")
            .HasPostgresExtension("citext")
            .HasPostgresExtension("cube")
            .HasPostgresExtension("dblink")
            .HasPostgresExtension("dict_int")
            .HasPostgresExtension("dict_xsyn")
            .HasPostgresExtension("earthdistance")
            .HasPostgresExtension("fuzzystrmatch")
            .HasPostgresExtension("hstore")
            .HasPostgresExtension("intarray")
            .HasPostgresExtension("ltree")
            .HasPostgresExtension("pg_stat_statements")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("pgrowlocks")
            .HasPostgresExtension("pgstattuple")
            .HasPostgresExtension("tablefunc")
            .HasPostgresExtension("unaccent")
            .HasPostgresExtension("uuid-ossp")
            .HasPostgresExtension("xml2");

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorsId).HasName("authors_pkey");

            entity.ToTable("authors");

            entity.Property(e => e.AuthorsId).HasColumnName("authors_id");
            entity.Property(e => e.Authorsname)
                .HasMaxLength(255)
                .HasColumnName("authorsname");
        });

        modelBuilder.Entity<Bbk>(entity =>
        {
            entity.HasKey(e => e.BbkId).HasName("bbk_pkey");

            entity.ToTable("bbk");

            entity.HasIndex(e => e.BbkCode, "bbk_bbk_code_key").IsUnique();

            entity.Property(e => e.BbkId).HasColumnName("bbk_id");
            entity.Property(e => e.BbkCode)
                .HasMaxLength(10)
                .HasColumnName("bbk_code");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("books_pkey");

            entity.ToTable("books");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Annotation).HasColumnName("annotation");
            entity.Property(e => e.BbkCode).HasColumnName("bbk_code");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.PlaceOfPublication).HasColumnName("place_of_publication");
            entity.Property(e => e.Publisher).HasColumnName("publisher");
            entity.Property(e => e.SeriesName)
                .HasMaxLength(255)
                .HasColumnName("series_name");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.YearOfPublication).HasColumnName("year_of_publication");

            entity.HasOne(d => d.BbkCodeNavigation).WithMany(p => p.Books)
                .HasForeignKey(d => d.BbkCode)
                .HasConstraintName("books_bbk_code_fkey");

            entity.HasOne(d => d.PlaceOfPublicationNavigation).WithMany(p => p.Books)
                .HasForeignKey(d => d.PlaceOfPublication)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("books_place_of_publication_fkey");

            entity.HasOne(d => d.PublisherNavigation).WithMany(p => p.Books)
                .HasForeignKey(d => d.Publisher)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("books_publisher_fkey");
        });

        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("book_authors");

            entity.Property(e => e.AuthorsId).HasColumnName("authors_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");

            entity.HasOne(d => d.Authors).WithMany()
                .HasForeignKey(d => d.AuthorsId)
                .HasConstraintName("book_authors_authors_id_fkey");

            entity.HasOne(d => d.Book).WithMany()
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("book_authors_book_id_fkey");
        });

        modelBuilder.Entity<BookEditor>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("book_editor");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.EditorsId).HasColumnName("editors_id");

            entity.HasOne(d => d.Book).WithMany()
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("book_editor_book_id_fkey");

            entity.HasOne(d => d.Editors).WithMany()
                .HasForeignKey(d => d.EditorsId)
                .HasConstraintName("book_editor_editors_id_fkey");
        });

        modelBuilder.Entity<BookTheme>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("book_themes");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.ThemesId).HasColumnName("themes_id");

            entity.HasOne(d => d.Book).WithMany()
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("book_themes_book_id_fkey");

            entity.HasOne(d => d.Themes).WithMany()
                .HasForeignKey(d => d.ThemesId)
                .HasConstraintName("book_themes_themes_id_fkey");
        });

        modelBuilder.Entity<Editor>(entity =>
        {
            entity.HasKey(e => e.EditorsId).HasName("editors_pkey");

            entity.ToTable("editors");

            entity.Property(e => e.EditorsId).HasColumnName("editors_id");
            entity.Property(e => e.Editorname)
                .HasMaxLength(255)
                .HasColumnName("editorname");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("favorites");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Book).WithMany()
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("favorites_book_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("favorites_user_id_fkey");
        });

        modelBuilder.Entity<Publicationplase>(entity =>
        {
            entity.HasKey(e => e.PublicationplaseId).HasName("publicationplase_pkey");

            entity.ToTable("publicationplase");

            entity.Property(e => e.PublicationplaseId).HasColumnName("publicationplase_id");
            entity.Property(e => e.Publicationplasesname)
                .HasMaxLength(255)
                .HasColumnName("publicationplasesname");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublishersId).HasName("publishers_pkey");

            entity.ToTable("publishers");

            entity.Property(e => e.PublishersId).HasColumnName("publishers_id");
            entity.Property(e => e.Publishersname)
                .HasMaxLength(255)
                .HasColumnName("publishersname");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.ThemesId).HasName("themes_pkey");

            entity.ToTable("themes");

            entity.Property(e => e.ThemesId).HasColumnName("themes_id");
            entity.Property(e => e.Themesname)
                .HasMaxLength(255)
                .HasColumnName("themesname");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Username)
                .HasMaxLength(16)
                .HasColumnName("username");
            entity.Property(e => e.Userpassword)
                .HasMaxLength(255)
                .HasColumnName("userpassword");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
