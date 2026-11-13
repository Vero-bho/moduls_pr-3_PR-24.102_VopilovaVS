using pr_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pr_3.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autho.xaml
    /// </summary>
    public partial class Autho : Page
    {
        int click;

        public Autho()
        {
            InitializeComponent();
            click = 0;
        }
        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null, "Гость"));
        }

        private void GenerateCaptcha()
        {
            txtBlockCaptcha.Visibility = Visibility.Visible;
            gridCaptcha.Visibility = Visibility.Visible;

            string captchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = captchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            click += 1;
            string login = txtLogin.Text.Trim();
            string password = psbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedPassword = PasswordHasher.GetHash(password);

            using (var context = new MiningMetallurgyPlantEntities1())
            {
                var user = context.Users.FirstOrDefault(x => x.Login == login && x.PasswordHash == hashedPassword);

                if (click == 1)
                {
                    if (user != null)
                    {
                        MessageBox.Show($"Вы вошли под: {user.FirstName} {user.LastName}\nВаша роль: {user.Roles.RoleName}");
                    }
                    else
                    {
                        MessageBox.Show("Вы ввели логин или пароль неверно!");
                        GenerateCaptcha();
                        psbPassword.Clear();
                    }
                }
                else if (click > 1)
                {
                    if (user != null && txtBoxCaptcha.Text.Trim() == tblCaptcha.Text)
                    {
                        MessageBox.Show($"Вы вошли под: {user.FirstName} {user.LastName}");
                        LoadPage("Клиент", user);
                    }
                    else
                    {
                        MessageBox.Show("Введите данные заново!");
                        GenerateCaptcha();
                        txtBoxCaptcha.Clear();
                        psbPassword.Clear();
                    }
                }
            }
        }

        private void LoadPage(string role, Users user)
        {
            click = 0;
            switch (role)
            {
                case "Клиент":
                    NavigationService.Navigate(new Client(user, role));
                    break;
            }
        }

        private void btnRegisterPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Register());
        }

    }
}
