using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace vohaduy_ltdd_api.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<EmployeeLiveLocation> EmployeeLiveLocations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<VideoMeetingRoom> VideoMeetingRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("identity");

            builder.Entity<User>().Property(u => u.Initials).HasMaxLength(50);
            builder.Entity<User>().Property(u => u.FullName).HasMaxLength(100);

            // Employee relations
            builder.Entity<Employee>()
                .HasIndex(e => e.EmployeeCode)
                .IsUnique();

            builder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // AttendanceRecord relations
            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Shift)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.ShiftId)
                .OnDelete(DeleteBehavior.SetNull);

            // LeaveRequest relations
            builder.Entity<LeaveRequest>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage relations (Use Restrict/NoAction to avoid multiple cascade paths)
            builder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Department)
                .WithMany()
                .HasForeignKey(m => m.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // VideoMeetingRoom relations
            builder.Entity<VideoMeetingRoom>()
                .HasIndex(v => v.RoomCode)
                .IsUnique();

            builder.Entity<VideoMeetingRoom>()
                .HasOne(v => v.HostEmployee)
                .WithMany()
                .HasForeignKey(v => v.HostEmployeeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<VideoMeetingRoom>()
                .HasOne(v => v.Department)
                .WithMany()
                .HasForeignKey(v => v.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}