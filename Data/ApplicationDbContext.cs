using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PAW_CATALOG_PROJ.Models;

namespace PAW_CATALOG_PROJ.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> // Inherits from IdentityDbContext to use Identity features
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> AppUsers { get; set; } = default!;
        public DbSet<Group> Groups { get; set; } = default!;
        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<Enrollment> Enrollments { get; set; } = default!;
        public DbSet<Grade> Grades { get; set; } = default!;
        public DbSet<Message> Messages { get; set; } = default!;



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
            base.OnModelCreating(modelBuilder);//important to call this to ensure the Identity tables are created correctly
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("varchar(50)").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasColumnType("varchar(100)").IsRequired();
               
            });

            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("grades");
                entity.HasKey(g => g.GradeId);
                entity.Property(g => g.GradeId).HasColumnName("grade_id").HasColumnType("int").ValueGeneratedOnAdd();

                entity.Property(g => g.EnrollmentId).HasColumnName("enrollment_id").HasColumnType("int").IsRequired();
                entity.Property(g => g.Title).HasColumnName("title").HasColumnType("varchar(50)").IsRequired();
                entity.Property(g => g.GradeValue).HasColumnName("grade").HasColumnType("decimal(5,2)").IsRequired();
                entity.Property(g => g.DateRecorded).HasColumnName("date_recorded").HasColumnType("date").IsRequired();

                entity.HasOne(g => g.Enrollment)
                      .WithMany(e => e.Grades)
                      .HasForeignKey(g => g.EnrollmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("enrollments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int").ValueGeneratedOnAdd();

                entity.Property(e => e.StudentId).HasColumnName("student_id").HasColumnType("varchar(50)").IsRequired();
                entity.Property(e => e.CourseId).HasColumnName("course_id").HasColumnType("int").IsRequired();
                entity.Property(e => e.TeacherId).HasColumnName("teacher_id").HasColumnType("varchar(50)").IsRequired();
                entity.Property(e => e.GroupId).HasColumnName("group_id").HasColumnType("int").IsRequired();
                entity.Property(e => e.EnrollmentYear).HasColumnName("enrollment_year").HasColumnType("varchar(10)").IsRequired();



                entity.HasOne(e => e.Student)
                      .WithMany(u => u.EnrollmentsAsStudent)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Teacher)
                      .WithMany(u => u.EnrollmentsAsTeacher)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Group)
                      .WithMany(g => g.Enrollments)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("groups");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).HasColumnName("id").HasColumnType("int").ValueGeneratedOnAdd();

                entity.Property(g => g.GroupNumber).HasColumnName("group_number").HasColumnType("varchar(50)").IsRequired();

            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("courses");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").HasColumnType("int").ValueGeneratedOnAdd();

                entity.Property(c => c.CourseName).HasColumnName("course_name").HasColumnType("varchar(100)").IsRequired();
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id).HasColumnName("id").HasColumnType("int").ValueGeneratedOnAdd();

                entity.Property(m => m.FromId).HasColumnName("from_id").HasColumnType("varchar(50)").IsRequired();
                entity.Property(m => m.To).HasColumnName("to").HasColumnType("varchar(50)").IsRequired();
                entity.Property(m => m.Content).HasColumnName("content").HasColumnType("varchar(100)").IsRequired();

                entity.HasOne(m => m.From)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.FromId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Recipient)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.To)
                      .OnDelete(DeleteBehavior.Restrict);


            });
        }

    }
}
