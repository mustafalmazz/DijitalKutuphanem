using System;
using System.Linq;
using BookManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BookManagementApp.Areas.Admin.Models;

var options = new DbContextOptionsBuilder<MyDbContext>()
    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BookManagementApp;Trusted_Connection=True;MultipleActiveResultSets=true")
    .Options;

using var context = new MyDbContext(options);
var follows = context.Follows.ToList();
foreach (var f in follows) {
    Console.WriteLine("Follower: " + f.FollowerId + " Following: " + f.FollowingId + " Accepted: " + f.IsAccepted);
}
