﻿// <auto-generated />
using System;
using MO.Model.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MO.Model.Migrations.MORecord
{
    [DbContext(typeof(MORecordContext))]
    partial class MORecordContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MO.Model.Entitys.LoginRecord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("LoginDevice")
                        .HasColumnType("varchar(20) CHARACTER SET utf8mb4")
                        .HasMaxLength(20);

                    b.Property<string>("LoginIP")
                        .HasColumnType("varchar(20) CHARACTER SET utf8mb4")
                        .HasMaxLength(20);

                    b.Property<int>("LoginType")
                        .HasColumnType("int")
                        .HasMaxLength(2);

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("LoginRecord");
                });
#pragma warning restore 612, 618
        }
    }
}