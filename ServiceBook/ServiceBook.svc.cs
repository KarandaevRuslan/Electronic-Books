
using Databases;
using Databases.Models;
using Databases.SQLiteClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;

namespace WcfServiceBook
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class ServiceBook : IServiceBook
    {
        private StorageDBcontext context = new StorageDBcontext();

        public void DeleteAccount(int userId)
        {
            Directory.Delete(AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
            {
        (object) userId
            }).Login, true);
            this.context.Users.Remove(this.context.Users.Find(new object[1]
            {
        (object) userId
            }));
            this.context.Books.RemoveRange((IEnumerable<Book>)this.context.Books.Where<Book>((Expression<Func<Book, bool>>)(x => x.UserId == userId)));
            this.context.SaveChanges();
        }

        public int EditUserData(int userId, string login, string password, string userInformation)
        {
            int num = this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Login.Equals(login))).Count<User>();
            if (num == 0)
            {
                try
                {
                    Directory.Move(AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
                    {
            (object) userId
                    }).Login, AppSettings.PathStorage + "\\" + login);
                    this.context.Users.Find(new object[1]
                    {
            (object) userId
                    }).Login = login;
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }
            else if (num != 0)
            {
                if (!login.Equals(this.context.Users.Find(new object[1]
                {
          (object) userId
                }).Login))
                    return 0;
            }
            this.context.Users.Find(new object[1]
            {
        (object) userId
            }).Password = password;
            this.context.Users.Find(new object[1]
            {
        (object) userId
            }).UserInformation = userInformation;
            this.context.SaveChanges();
            return 1;
        }

        public string GetDescription(int userId)
        {
            try
            {
                return this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Id == userId)).First<User>().UserInformation;
            }
            catch (Exception ex)
            {
                return "Пользователь не существует";
            }
        }

        public string GetName(int userId)
        {
            try
            {
                return this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Id == userId)).First<User>().Login;
            }
            catch (Exception ex)
            {
                return "Пользователь не существует";
            }
        }

        public string GetPassword(int userId)
        {
            try
            {
                return this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Id == userId)).First<User>().Password;
            }
            catch (Exception ex)
            {
                return "Пользователь не существует";
            }
        }

        public int RegisterUser(string login, string password)
        {
            if (this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Login == login)).Count<User>() != 0)
                return 0;
            try
            {
                Directory.CreateDirectory(AppSettings.PathStorage + "\\" + login);
            }
            catch (Exception ex)
            {
                return -1;
            }
            int num = this.context.NewUserId();
            this.context.Users.Add(new User()
            {
                Id = num,
                Login = login,
                Password = password,
                UserInformation = ""
            });
            this.context.SaveChanges();
            return num;
        }

        public List<int> SearchUsers(int idIgnore, string searchText)
        {
            if (searchText.Equals(""))
                return this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Id != idIgnore)).Select<User, int>((Expression<Func<User, int>>)(x => x.Id)).ToList<int>();
            return this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Id != idIgnore && x.Login.Contains(searchText))).Select<User, int>((Expression<Func<User, int>>)(x => x.Id)).ToList<int>();
        }

        public int SignIn(string login, string password)
        {
            int num = 0;
            try
            {
                num = this.context.Users.Where<User>((Expression<Func<User, bool>>)(x => x.Login.Equals(login) && x.Password.Equals(password))).First<User>().Id;
            }
            catch (Exception ex)
            {
            }
            return num;
        }

        private void SaveFileStream(string filePath, Stream stream)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                FileStream destination = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                stream.CopyTo((Stream)destination);
                destination.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Put(FileTransferRequest[] filesToPush, string name, string description, int userId)
        {
            while (true)
            {
                if (Directory.Exists(AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
                {
          (object) userId
                }).Login + "\\" + name))
                    name += "_";
                else
                    break;
            }
            int num = this.context.NewBookId();
            this.context.Books.Add(new Book()
            {
                Id = num,
                Name = name,
                Description = description,
                UserId = userId,
                CanEdit = 1,
                Access = 1
            });
            this.context.SaveChanges();
            string path = AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
            {
        (object) userId
            }).Login + "\\" + name;
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\data");
            Directory.CreateDirectory(path + "\\database");
            foreach (FileTransferRequest fileTransferRequest in filesToPush)
                this.SaveFileStream(path + "\\database\\" + fileTransferRequest.FileName, (Stream)new MemoryStream(fileTransferRequest.Content));
            return num;
        }

        public string[] GetBook(int bookId)
        {
            return new List<string>()
      {
        this.context.Books.Find(new object[1]
        {
          (object) bookId
        }).Name,
        this.context.Books.Find(new object[1]
        {
          (object) bookId
        }).Description,
        this.context.Users.Find(new object[1]
        {
          (object) this.context.Books.Find(new object[1]
          {
            (object) bookId
          }).UserId
        }).Login,
        this.context.Books.Find(new object[1]
        {
          (object) bookId
        }).CanEdit.ToString(),
        this.context.Books.Find(new object[1]
        {
          (object) bookId
        }).Access.ToString()
      }.ToArray();
        }

        public List<int> GetBooksId(int userId)
        {
            return this.context.Books.Where<Book>((Expression<Func<Book, bool>>)(x => x.UserId == userId)).Select<Book, int>((Expression<Func<Book, int>>)(x => x.Id)).ToList<int>();
        }

        public void Put1(FileTransferRequest fileToPush, int bookId, int userId, string path)
        {
            string path1 = AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
            {
        (object) userId
            }).Login + "\\" + this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).Name + "\\" + path;
            Directory.CreateDirectory(path1);
            this.SaveFileStream(path1 + "\\" + fileToPush.FileName, (Stream)new MemoryStream(fileToPush.Content));
        }

        public void DeleteBook(int bookId)
        {
            int userId = this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).UserId;
            Directory.Delete(AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
            {
        (object) userId
            }).Login + "\\" + this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).Name, true);
            this.context.Books.Remove(this.context.Books.Find(new object[1]
            {
        (object) bookId
            }));
            this.context.SaveChanges();
        }

        public int EditBookName(int bookId, string name)
        {
            try
            {
                int userId = this.context.Books.Find(new object[1]
                {
          (object) bookId
                }).UserId;
                if (this.context.Books.Where<Book>((Expression<Func<Book, bool>>)(x => x.Name.Equals(name) && x.UserId == userId)).Count<Book>() != 0)
                    return -1;
                Directory.Move(AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
                {
          (object) userId
                }).Login + "\\" + this.context.Books.Find(new object[1]
                {
          (object) bookId
                }).Name, AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
                {
          (object) userId
                }).Login + "\\" + name);
                this.context.Books.Find(new object[1]
                {
          (object) bookId
                }).Name = name;
                this.context.SaveChanges();
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public void EditBookDescription(int bookId, string description)
        {
            this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).Description = description;
            this.context.SaveChanges();
        }

        public void EditBookCanEdit(int bookId, int canEdit)
        {
            this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).CanEdit = canEdit;
            this.context.SaveChanges();
        }

        public void EditBookAccess(int bookId, int access)
        {
            this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).Access = access;
            this.context.SaveChanges();
        }

        public FileTransferRequest[] Get(int bookId, int userId)
        {
            List<FileTransferRequest> fileTransferRequestList = new List<FileTransferRequest>();
            string path = AppSettings.PathStorage + "\\" + this.context.Users.Find(new object[1]
            {
        (object) userId
            }).Login + "\\" + this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).Name;
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                string str = AnyConverter.Get_Directory_Name_Extension_FromFilePath(file.Substring(path.Length + 1))[0];
                FileTransferRequest fileTransferRequest = new FileTransferRequest()
                {
                    FileName = AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[1] + "." + AnyConverter.Get_Directory_Name_Extension_FromFilePath(file)[2],
                    Content = File.ReadAllBytes(file),
                    Path = str
                };
                fileTransferRequestList.Add(fileTransferRequest);
            }
            return fileTransferRequestList.ToArray();
        }

        public List<int> SearchBooks(int idIgnore, string searchText)
        {
            if (searchText.Equals(""))
                return this.context.Books.Where<Book>((Expression<Func<Book, bool>>)(x => x.UserId != idIgnore && x.Access == 1)).Select<Book, int>((Expression<Func<Book, int>>)(x => x.Id)).ToList<int>();
            return this.context.Books.Where<Book>((Expression<Func<Book, bool>>)(x => x.UserId != idIgnore && x.Name.Contains(searchText) && x.Access == 1)).Select<Book, int>((Expression<Func<Book, int>>)(x => x.Id)).ToList<int>();
        }

        public int GetUserId(int bookId)
        {
            return this.context.Books.Find(new object[1]
            {
        (object) bookId
            }).UserId;
        }
    }
}