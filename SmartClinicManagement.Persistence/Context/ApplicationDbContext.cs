

using Microsoft.EntityFrameworkCore;

using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Department> Departments { get; set; }
    public DbSet<PasswordDetails> PasswordDetails { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Activity> Activities { get; set; }

    public DbSet<RoleActivities> RoleActivities { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Billing> Billings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasKey(x => x.UserID);

        modelBuilder.Entity<Role>()
            .HasKey(x => x.RoleID);

        modelBuilder.Entity<Department>()
            .HasKey(x => x.DepartmentID);
        modelBuilder.Entity<Appointment>()
    .HasKey(x => x.AppointmentID);
        modelBuilder.Entity<MedicalRecord>()
    .HasKey(x => x.RecordID);
        modelBuilder.Entity<Prescription>()
    .HasKey(x => x.PrescriptionID);


        modelBuilder.Entity<Billing>(entity =>
        {
            entity.ToTable("billings");

            entity.HasKey(e => e.BillID);
        });
        modelBuilder.Entity<Payment>()
    .HasKey(x => x.PaymentID);
        modelBuilder.Entity<User>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleID);
        modelBuilder.Entity<AuditLog>()
.HasKey(x => x.AuditID);
        modelBuilder.Entity<User>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.DepartmentID);
        modelBuilder.Entity<PasswordDetails>()
    .HasKey(x => x.PasswordID);
        modelBuilder.Entity<Doctor>()
    .HasKey(x => x.DoctorID);

        modelBuilder.Entity<Doctor>()
            .HasOne(x => x.User)
            .WithOne(x => x.Doctor)
            .HasForeignKey<Doctor>(x => x.UserID);
        modelBuilder.Entity<Patient>()
    .HasKey(x => x.PatientID);

        modelBuilder.Entity<Patient>()
            .HasOne(x => x.User)
            .WithOne(x => x.Patient)
            .HasForeignKey<Patient>(x => x.UserID);

        modelBuilder.Entity<Patient>()
            .HasOne(x => x.PrimaryDoctor)
            .WithMany()
            .HasForeignKey(x => x.PrimaryDoctorID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PasswordDetails>()
            .HasOne(x => x.User)
            .WithMany(x => x.PasswordDetails)
            .HasForeignKey(x => x.UserID);
        modelBuilder.Entity<RoleActivities>()
    .HasKey(x => x.RoleActivityID);
        modelBuilder.Entity<RoleActivities>()
    .HasOne(x => x.Role)
    .WithMany(x => x.RoleActivities)
    .HasForeignKey(x => x.RoleID);

        modelBuilder.Entity<RoleActivities>()
            .HasOne(x => x.Activity)
            .WithMany(x => x.RoleActivities)
            .HasForeignKey(x => x.ActivityID);

        modelBuilder.Entity<Permission>()
            .HasOne(x => x.RoleActivity)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.RoleActivityID)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
