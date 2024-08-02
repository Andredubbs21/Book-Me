﻿using UserService.API.Entities;
using Microsoft.EntityFrameworkCore;
namespace UserService.API.Data;


public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

}