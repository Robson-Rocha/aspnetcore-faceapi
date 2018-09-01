using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace RobsonAraujo.Demos.ASPNetCoreFaceAPI
{
    public class User
    {
        public Guid PersonId { get; set; }
        public string Name { get; }
        public byte[] ImageBytes { get; }
        public string ImageDataUrl {get;}

        public User(string name, string imagePath)
        {
            Name = name;
            ImageBytes = File.ReadAllBytes(imagePath);
            ImageDataUrl = $"data:image/{Path.GetExtension(imagePath).Replace(".","")};base64,{Convert.ToBase64String(ImageBytes)}";
        }
    }

    public interface IUserService
    {
        IEnumerable<User> Users { get; }
    }

    public class UserService : IUserService
    {
        public IEnumerable<User> Users { get; }

        public UserService(IHostingEnvironment env)
        {
            #region Usuarios
            Users = new[] {
                new User ("Robson Araújo", Path.Combine(env.WebRootPath, "users", "robson.png")),
                new User ("Nicely Soares", Path.Combine(env.WebRootPath, "users", "ni.png"))
            }; 
            #endregion
        }
    }
}