using Newtonsoft.Json;
using Sklad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Sklad.MainWindow;

namespace sklad
{
    /// <summary>
    /// Логика взаимодействия для auth.xaml
    /// </summary>
    public partial class auth : Window
    {
        public class User : INotifyPropertyChanged
        {
            private string login;
            private string pass;

            public string Login
            {
                get { return login; }
                set
                {
                    login = value;
                    NotifyPropertyChanged();
                }
            }

            public string Pass
            {
                get { return pass; }
                set
                {
                    pass = value;
                    NotifyPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<User> Users { get; set; } = new List<User>();
        private const string dataFilePath = "../../data/auth.json";
        private const string readme = "../../data/readme";

        public auth()
        {
            InitializeComponent();
            DataContext = this;
            LoadData();

            if (File.Exists(readme)) // проверка на первый старт
            {
                string readmestr = File.ReadAllText(readme);
                MessageBox.Show(readmestr, "рид ми");
                string oldFilePath = "../../data/readme";
                string newFileName = "../../data/readme.json";
                File.Move(oldFilePath, newFileName);
            }
        }
        private void SaveData()
        {
            string jsonData = JsonConvert.SerializeObject(Users);
            File.WriteAllText(dataFilePath, jsonData);
        }
        private void LoadData()
        {
            if (File.Exists(dataFilePath))
            {
                string jsonData = File.ReadAllText(dataFilePath);
                Users = JsonConvert.DeserializeObject<List<User>>(jsonData);
            }
            else
            {
                SaveData();
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string passHashed = CalculateSHA256(passwordTextbox.Text);
            User user = Users.FirstOrDefault(u => u.Login == loginTextbox.Text && u.Pass == passHashed);

            if (user != null)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Неправильный логин/пароль", "Ошибка авторизации");
                
            }
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            if (passwordTextbox.Text != "Пароль" && loginTextbox.Text != "Логин")
            {
                string passHashed = CalculateSHA256(passwordTextbox.Text);
                if (Users.Any(u => u.Login == loginTextbox.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка регистрации");
                    return;
                }
                User user = new User()
                {
                    Login = loginTextbox.Text,
                    Pass = passHashed
                };
                MessageBox.Show("Пользователь добавлен");
                Users.Add(user);
                SaveData();
            }
            else
            {
                MessageBox.Show("Введите логин и пароль для регистрации", "Ошибка регистрации");
            }
            
        }
        private string CalculateSHA256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] hashValue;
            UTF8Encoding objUtf8 = new UTF8Encoding();
            hashValue = sha256.ComputeHash(objUtf8.GetBytes(str));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hashValue)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        private void loginTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (loginTextbox.Text == "Логин")
            {
                loginTextbox.Text = "";
                loginTextbox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }
        private void loginTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (loginTextbox.Text == "")
            {
                loginTextbox.Text = "Логин";
                loginTextbox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void passwordTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (passwordTextbox.Text == "Пароль")
            {
                passwordTextbox.Text = "";
                passwordTextbox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void passwordTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (passwordTextbox.Text == "")
            {
                passwordTextbox.Text = "Пароль";
                passwordTextbox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void loginTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loginButton_Click(sender, e);
            }
        }

        private void passwordTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loginButton_Click(sender, e);
            }
        }
    }
}
