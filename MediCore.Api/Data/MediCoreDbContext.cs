//this file the Context is used by tthe API controllers to read, add, update, and delete database data

using MediCore.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Data
{
    public class MediCoreDbContext : IdentityDbContext<ApplicationUser>
    {
        public MediCoreDbContext(
            DbContextOptions<MediCoreDbContext> options
        ) : base(options)
        {
        }

        public DbSet<MedicalDocument> MedicalDocuments { get; set; } = null!; //this  table manages docs
        //DbSet<T> is how EF Core knows that a class should be stored as a database table and queried from the database
        public DbSet<Medication> Medications => Set<Medication>();
        public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();
        public DbSet<Chat> Chats { get; set; } = null!; //null! means: do not warn me that this prop starts as null; EF Core will init it later
        //this table manages chats, one row means one conversation
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        //each message belongs to a chat through ChatId
        public DbSet<PatientProfile> PatientProfiles { get; set; } = null!;
        //extra patient-specific info connected to one Identity user, later can contain fields like: DateOfBirth, Gender, EmergencyContact, MedicalNotes, InsurenceNumber, etc.
        //we do not put all patient data directly inside AspNetUsers cuz users may be Patients, Doctors, or Admins
        public DbSet<DoctorProfile> DoctorProfiles { get; set; } = null!;
        //contains doctor-specific information
        public DbSet<PatientDoctorRelation> PatientDoctorRelations { get; set; } = null!;
        //this is the relation table that connects Patient user <-> Doctor user (this table is needed cuz the relationship is m to m)
        protected override void OnModelCreating(ModelBuilder builder) //this method tells EF Core how the database relationships should work; EF Core can guess many relationships from prp names, but for more complex models, it is better to configure them clearly
        { //this method is used when we need: relationships, foreign keys, unique indexes, delete behavior, table names, column rules, required fields, max length, etc.
            base.OnModelCreating(builder);// this should be 1st before adding custom relationship rules; means: IdentityDbContext, create and configure all Identity tables 1st

            builder.Entity<MedicalDocument>()
                .HasOne(document => document.OwnerUser)
                .WithMany()
                .HasForeignKey(document => document.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MedicalDocument>()
                .Property(document => document.OwnerUserId)
                .IsRequired();

            builder.Entity<MedicalDocument>()
                .HasIndex(document => document.OwnerUserId);

            // MEDICATION

            // One patient can own many medication records.
            builder.Entity<Medication>()
                .HasOne(medication => medication.Patient)
                .WithMany()
                .HasForeignKey(medication => medication.PatientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Medication>()
                .Property(medication => medication.PatientUserId)
                .IsRequired();

            builder.Entity<Medication>()
                .HasIndex(medication => medication.PatientUserId);

            // MEDICATION SCHEDULE

            // One medication can have multiple reminder/schedule times.
            builder.Entity<MedicationSchedule>()
                .HasOne(schedule => schedule.Medication)
                .WithMany(medication => medication.Schedules)
                .HasForeignKey(schedule => schedule.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MedicationSchedule>()
                .HasIndex(schedule => new
                {
                    schedule.MedicationId,
                    schedule.TimeOfDay
                })
                .IsUnique();

            builder.Entity<Chat>()
                .HasOne(chat => chat.OwnerUser)
                .WithMany()
                .HasForeignKey(chat => chat.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Chat>()
                .Property(chat => chat.OwnerUserId)
                .IsRequired();

            builder.Entity<Chat>()
                .HasIndex(chat => chat.OwnerUserId);

            builder.Entity<PatientProfile>() //configure the PatientProfile entity/table
                .HasOne(profile => profile.User) //one PatientProfile has one AplicationUser
                .WithOne() //one ApplicationUser can have one PatientProfile; so this is one-to-one relationship
                .HasForeignKey<PatientProfile>(profile => profile.UserId) //the UserId col inside PatientProfile is the foreign key; 
                //Foreign Key is a val that points to a record in another table
                .OnDelete(DeleteBehavior.Cascade); //if the user is permanently deleted, delete their ParentProfile automatically
            //ex. delete user abc-123 -> automatically delete PatientProfile where UserId = abc-23; this makes sense cuz a PacientProfile should not exist without its user account

            builder.Entity<DoctorProfile>()
                .HasOne(profile => profile.User)
                .WithOne()
                .HasForeignKey<DoctorProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PatientDoctorRelation>()
                .HasOne(relation => relation.Patient) //Each PatientDoctorRelation row has one Patient.
                .WithMany() //one patient can appear inn many PatientDoctorRelation rows
                .HasForeignKey(relation => relation.PatientUserId) //PatientUserId is the foreign key that points to AspNetUsers.Id
                .OnDelete(DeleteBehavior.Restrict); // do not automatically delete patient-doctor relations when a patient user is deleted

            builder.Entity<PatientDoctorRelation>()
                .HasOne(relation => relation.Doctor)
                .WithMany()
                .HasForeignKey(relation => relation.DoctorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PatientDoctorRelation>() //this prevents duplicate patient-doctor connections, meaning SQL Server will not allow this twice: Patient A -> Doctor B, Patient A -> Doctor B
                .HasIndex(relation => new
                {
                    relation.PatientUserId,
                    relation.DoctorUserId
                })
                .IsUnique();
        }
    }
}