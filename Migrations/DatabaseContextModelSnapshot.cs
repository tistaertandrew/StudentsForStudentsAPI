﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentsForStudentsAPI.Models;

#nullable disable

namespace StudentsForStudentsAPI.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CursusId")
                        .HasColumnType("int");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CursusId");

                    b.ToTable("Courses", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CursusId = 1,
                            Label = "UE1 - Programmation de base (B1)"
                        },
                        new
                        {
                            Id = 2,
                            CursusId = 1,
                            Label = "UE2 - Architecture des ordinateurs (B1)"
                        },
                        new
                        {
                            Id = 3,
                            CursusId = 1,
                            Label = "UE3 - Conception d'applications 1 (B1)"
                        },
                        new
                        {
                            Id = 4,
                            CursusId = 1,
                            Label = "UE35 - Communication écrite et orale en langue française (B1)"
                        },
                        new
                        {
                            Id = 5,
                            CursusId = 1,
                            Label = "UE4 - Mathématiques appliquées à l'informatique 1 (B1)"
                        },
                        new
                        {
                            Id = 6,
                            CursusId = 1,
                            Label = "UE5 - Base de données (B1)"
                        },
                        new
                        {
                            Id = 7,
                            CursusId = 1,
                            Label = "UE6 - Développement web (B1)"
                        },
                        new
                        {
                            Id = 8,
                            CursusId = 1,
                            Label = "UE7 - Anglais (B1)"
                        },
                        new
                        {
                            Id = 9,
                            CursusId = 1,
                            Label = "UE8 - E-Business (B1)"
                        },
                        new
                        {
                            Id = 10,
                            CursusId = 1,
                            Label = "UE9 - Programmation intermédiaire (B1)"
                        },
                        new
                        {
                            Id = 11,
                            CursusId = 1,
                            Label = "UE13 - Systèmes d'exploitation (B2)"
                        },
                        new
                        {
                            Id = 12,
                            CursusId = 1,
                            Label = "UE14 - Anglais (B2)"
                        },
                        new
                        {
                            Id = 13,
                            CursusId = 1,
                            Label = "UE15 - Droit et Ethique du monde numérique (B2)"
                        },
                        new
                        {
                            Id = 14,
                            CursusId = 1,
                            Label = "UE16 - Digitalisation et nouvelle économie (B2)"
                        },
                        new
                        {
                            Id = 15,
                            CursusId = 1,
                            Label = "UE17 - Mathématiques appliquées à l'informatique 2 (B2)"
                        },
                        new
                        {
                            Id = 16,
                            CursusId = 1,
                            Label = "UE18 - Développement mobile (B2)"
                        },
                        new
                        {
                            Id = 17,
                            CursusId = 1,
                            Label = "UE19 - Développement web avancé (B2)"
                        },
                        new
                        {
                            Id = 18,
                            CursusId = 1,
                            Label = "UE20 - Langage de scripts dynamiques (B2)"
                        },
                        new
                        {
                            Id = 19,
                            CursusId = 1,
                            Label = "UE21 - Réseaux informatiques (B2)"
                        },
                        new
                        {
                            Id = 20,
                            CursusId = 1,
                            Label = "UE36 - Programmation avancée (B2)"
                        },
                        new
                        {
                            Id = 21,
                            CursusId = 1,
                            Label = "UE10 - Conception d'applications 2 (B2)"
                        },
                        new
                        {
                            Id = 22,
                            CursusId = 1,
                            Label = "UE22 - Laboratoire pluridisciplinaire (B2)"
                        },
                        new
                        {
                            Id = 23,
                            CursusId = 1,
                            Label = "UE23 - SALTo (B2)"
                        },
                        new
                        {
                            Id = 24,
                            CursusId = 1,
                            Label = "UE25 - Architectures logicielles (B3)"
                        },
                        new
                        {
                            Id = 25,
                            CursusId = 1,
                            Label = "UE26 - Frameworks web (B3)"
                        },
                        new
                        {
                            Id = 26,
                            CursusId = 1,
                            Label = "UE27 - Entrepreneuriat (B3)"
                        },
                        new
                        {
                            Id = 27,
                            CursusId = 1,
                            Label = "UE28 - Savoir être, culture d'entreprise (B3)"
                        },
                        new
                        {
                            Id = 28,
                            CursusId = 1,
                            Label = "UE29 - Informatique managériale (B3)"
                        },
                        new
                        {
                            Id = 29,
                            CursusId = 1,
                            Label = "UE30 - Stage et travail de fin d'études (B3)"
                        },
                        new
                        {
                            Id = 30,
                            CursusId = 1,
                            Label = "UE31 - Mémoire (B3)"
                        },
                        new
                        {
                            Id = 31,
                            CursusId = 1,
                            Label = "UE32 - Administration réseau et système (LINUX) (B3)"
                        },
                        new
                        {
                            Id = 32,
                            CursusId = 1,
                            Label = "UE24 - Administration réseau et système (WINDOWS) (B3)"
                        },
                        new
                        {
                            Id = 33,
                            CursusId = 1,
                            Label = "UE33 - Conférences - Visites - Séminaires (B3)"
                        },
                        new
                        {
                            Id = 34,
                            CursusId = 1,
                            Label = "UE34 - SALTo (B3)"
                        });
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Cursus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SectionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SectionId");

                    b.ToTable("Cursus", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "Développement d'applications",
                            SectionId = 1
                        },
                        new
                        {
                            Id = 2,
                            Label = "Cybersécurité",
                            SectionId = 1
                        },
                        new
                        {
                            Id = 3,
                            Label = "Marketing",
                            SectionId = 2
                        },
                        new
                        {
                            Id = 4,
                            Label = "Droit",
                            SectionId = 2
                        },
                        new
                        {
                            Id = 5,
                            Label = "Commerce Extérieur",
                            SectionId = 2
                        });
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Extension")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OwnerId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Form", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("HandlerId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("HandlerId");

                    b.HasIndex("SenderId");

                    b.ToTable("Forms");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Place", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Locality")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PostalCode")
                        .HasColumnType("int");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Places");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HandlerId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PlaceId")
                        .HasColumnType("int");

                    b.Property<string>("SenderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("HandlerId");

                    b.HasIndex("PlaceId");

                    b.HasIndex("SenderId");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Section", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Sections", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "Technique"
                        },
                        new
                        {
                            Id = 2,
                            Label = "Economique"
                        });
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("CalendarLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CursusId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("CursusId");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StudentsForStudentsAPI.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Course", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.Cursus", "Cursus")
                        .WithMany()
                        .HasForeignKey("CursusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cursus");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Cursus", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.Section", "Section")
                        .WithMany()
                        .HasForeignKey("SectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Section");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.File", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StudentsForStudentsAPI.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.Navigation("Course");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Form", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.User", "Handler")
                        .WithMany()
                        .HasForeignKey("HandlerId");

                    b.HasOne("StudentsForStudentsAPI.Models.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId");

                    b.Navigation("Handler");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.Request", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StudentsForStudentsAPI.Models.User", "Handler")
                        .WithMany()
                        .HasForeignKey("HandlerId");

                    b.HasOne("StudentsForStudentsAPI.Models.Place", "Place")
                        .WithMany()
                        .HasForeignKey("PlaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StudentsForStudentsAPI.Models.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId");

                    b.Navigation("Course");

                    b.Navigation("Handler");

                    b.Navigation("Place");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("StudentsForStudentsAPI.Models.User", b =>
                {
                    b.HasOne("StudentsForStudentsAPI.Models.Cursus", "Cursus")
                        .WithMany()
                        .HasForeignKey("CursusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cursus");
                });
#pragma warning restore 612, 618
        }
    }
}
