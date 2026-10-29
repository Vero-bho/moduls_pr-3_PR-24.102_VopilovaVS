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
        public Autho()
        {
            InitializeComponent();
        }
        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null));
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(psbPassword.Password))
            {
                MessageBox.Show("Заполните логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string inputHash = PasswordHasher.GetHash(psbPassword.Password);

            using (var context = new pr_3.Models.MiningMetallurgyPlantEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Login == txtLogin.Text && u.PasswordHash == inputHash);

                if (user != null)
                {
                    MessageBox.Show("Авторизация успешна!");
                    NavigationService.Navigate(new Client(user.FirstName));
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRegisterPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Register());
        }

    }
}
