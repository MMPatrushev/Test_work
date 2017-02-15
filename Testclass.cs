using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

/// <summary>
/// Сводное описание для Testclass
/// </summary>
public static class Testclass
{
    public static string DBPath = ""; // path to the database
    public static SQLiteConnection DBConnection   // SQlite Connection
    {
        get
        {
            return new SQLiteConnection(new SQLitePlatformWinRT(), DBPath);
        }
    }

    public class Posts // класс постов
    {
        public int id_post { set; get; }
        public string post_name { set; get; }
        public string post_text { set; get; }
        public DateTime dt { set; get; }

        public Posts(int id_post, string post_name, string post_text)
        {
            this.id_post = id_post;
            this.post_name = post_name;
            this.post_text = post_text;
            this.dt = DateTime.Now;
        }

    }

    public class Categories  //класс Категорий
    {
        public int id_category { set; get; }
        public string category_name { set; get; }

        public Categories(int id_category, string category_name)
        {
            this.id_category = id_category;
            this.category_name = category_name;
        }
    }

    public class Users   //Класс пользователей
    {
        public int id_user { set; get; }
        public string login { set; get; }
        public string name { set; get; }
        public string surname { set; get; }
        public string email { set; get; }

        public Users(int id_user, string login, string name, string surname, string email)
        {
            this.id_user = id_user;
            this.login = login;
            this.name = name;
            this.surname = surname;
            this.email = email;
        }
    }
    public class Blogs
    {
        public int id_user;
        public int id_block;
        public Blogs()
        {

        }
    }
    public static void AddCategory(Categories c, Users u)           // Дбавление категории
    {
        using (var db = DBConnection)
        {

            if (db.Query<Categories>("SELECT * FROM Categories WHERE category_name = ?", c.category_name).Count == 0)
            {
                db.Table<Categories>().Insert(new Categories(c.id_category, c.category_name));
                SendCategoryMail(u.id_user);
            }
            else
            {
                Session("CurrentError") = "Such category already exists";
                Server.Transfer("ApplicationError.aspx");
            }

        }

    }
    public static void AddPost(Posts p)      // добавление поста
    {
        using (var db = DBConnection)
        {
            db.Table<Posts>.Insert(new Posts(p.id_post, p.post_name, p.post_text));
        }
    }


    public class Post_category
    {
        public int id_post;
        public int id_category;
        public int id_blog;

        public Post_category(Posts p, Categories c, Blogs b)
        {
            this.id_blog = b.id_block;
            this.id_post = p.id_post;
            this.id_category = c.id_category;
        }
    }
    public static void AddPost_category(Posts p, List<Categories> c, Blogs b)        // Добавление в базу Post_categories
    {
        using (var db = DBConnection)
        {

            foreach (Categories temp_cat in c)
            {
                db.Table<Post_category>().Insert(new Post_category(p, temp_cat, b));
            }
            Blogs bdb = db.Query<Users>("SELECT * FROM Blogs WHERE id = ?", b.id_block).First();
            SendCategoryMail(bdb.id_user);

        }
    }
    public static void SendCategoryMail(int id_user)
    {

        using (var db = DBConnection)
        {
            MailMessage message = new MailMessage();
            Users udb = db.Query<Users>("SELECT * FROM Users WHERE id = ?", id_user).First();
            message.To.Add(new MailAddress(udb.email)); // кому 
            message.Subject = "Category creation"; // тема письма  
            message.Body = "Category was successfully create";
            SmtpClient client = new SmtpClient("smtp.rambler.ru");
            client.Port = 25; // указываем порт  
            client.Credentials = new NetworkCredential("masikmaloj@rambler.ru", "ваш пароль");
            client.Send(message);  // отправить  
        }
    }

    public static void SendPostsMail(Users u)
    {

        using (var db = DBConnection)
        {
            MailMessage message = new MailMessage();
            Users udb = db.Query<Users>("SELECT * FROM Users WHERE id = ?", u.id_user).First();
            message.To.Add(new MailAddress(udb.email)); // кому отправлять  
            message.Subject = "Post creation"; // тема письма  
            message.Body = "Post was created successfully";
            SmtpClient client = new SmtpClient("smtp.rambler.ru");
            client.Port = 25; // указываем порт  

            client.Credentials =
                new NetworkCredential("masikmaloj@rambler.ru", "ваш пароль");

            client.Send(message);  // отправить  
        }

    }
}