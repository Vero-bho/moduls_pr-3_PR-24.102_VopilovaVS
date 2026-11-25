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
using System.Windows.Threading;

namespace pr_3.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autho.xaml
    /// </summary>
    public partial class Autho : Page
    {
        private int failedAttempts = 0;

        private DispatcherTimer lockoutTimer;

        private DateTime lockoutEndTime = DateTime.MinValue;

        private const int MaxAttempts = 3;
        private const int LockoutSeconds = 30;
        public Autho()
        {
            InitializeComponent();
            InitializeLockoutTimer();
            RestoreLockoutState();
        }

        private void InitializeLockoutTimer()
        {
            lockoutTimer = new DispatcherTimer();
            lockoutTimer.Interval = TimeSpan.FromSeconds(1);
            lockoutTimer.Tick += LockoutTimer_Tick;
        }

        private void LockoutTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan remaining = lockoutEndTime - DateTime.Now;
            int secondsLeft = (int)Math.Ceiling(remaining.TotalSeconds);

            if (secondsLeft <= 0)
            {
                lockoutTimer.Stop();
                EndLockout();
                return;
            }

            txtLockoutInfo.Text =
                $"Превышено число попыток. Повторный ввод доступен через: {secondsLeft} сек.";
        }

        private void RestoreLockoutState()
        {
            string saved = Properties.Settings.Default.LockoutEndTime;
            if (!string.IsNullOrEmpty(saved) &&
                DateTime.TryParse(saved, out DateTime savedEnd) &&
                savedEnd > DateTime.Now)
            {
                lockoutEndTime = savedEnd;
                StartLockout(true);
            }
        }

        private void StartLockout(bool isRestored = false)
        {
            SetControlsEnabled(false);
            txtLockoutInfo.Visibility = Visibility.Visible;
            txtAttemptsInfo.Text = string.Empty;

            Properties.Settings.Default.LockoutEndTime = lockoutEndTime.ToString("O");
            Properties.Settings.Default.Save();

            lockoutTimer.Start();

            int secondsLeft = (int)Math.Ceiling((lockoutEndTime - DateTime.Now).TotalSeconds);
            txtLockoutInfo.Text =
                $"Превышено число попыток. Повторный ввод доступен через: {Math.Max(secondsLeft, 0)} сек.";
        }

        private void EndLockout()
        {
            Properties.Settings.Default.LockoutEndTime = string.Empty;
            Properties.Settings.Default.Save();

            lockoutEndTime = DateTime.MinValue;
            failedAttempts = 0;

            SetControlsEnabled(true);
            txtLockoutInfo.Visibility = Visibility.Collapsed;
            txtLockoutInfo.Text = string.Empty;
            txtAttemptsInfo.Text = string.Empty;

            HideCaptcha();
            txtBoxCaptcha.Clear();
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtLogin.IsEnabled = enabled;
            psbPassword.IsEnabled = enabled;
            txtPasswordVisible.IsEnabled = enabled;
            chkShowPassword.IsEnabled = enabled;
            txtBoxCaptcha.IsEnabled = enabled;
            btnEnter.IsEnabled = enabled;
        }

        private void GenerateCaptcha()
        {
            txtBlockCaptcha.Visibility = Visibility.Visible;
            gridCaptcha.Visibility = Visibility.Visible;

            string captchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = captchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }

        private void HideCaptcha()
        {
            txtBlockCaptcha.Visibility = Visibility.Collapsed;
            gridCaptcha.Visibility = Visibility.Collapsed;
            tblCaptcha.Text = string.Empty;
        }

        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null, "Гость"));
        }

        private void btnRegisterPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Register());
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (lockoutTimer.IsEnabled) return;

            string login = txtLogin.Text.Trim();
            string password = GetPassword();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните логин и пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (gridCaptcha.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(txtBoxCaptcha.Text) ||
                    txtBoxCaptcha.Text.Trim() != tblCaptcha.Text)
                {
                    RegisterFailedAttempt(login, "Неверная CAPTCHA", regenerateCaptcha: true);
                    return;
                }
            }

            string hashedPassword = PasswordHasher.GetHash(password);

            using (var context = new MiningMetallurgyPlantEntities1())
            {
                var user = context.Users.FirstOrDefault(x =>
                    x.Login == login && x.PasswordHash == hashedPassword);

                if (user != null)
                {
                    WriteLoginHistory(context, login, true, null);

                    failedAttempts = 0;
                    HideCaptcha();
                    txtAttemptsInfo.Text = string.Empty;

                    MessageBox.Show($"Вы вошли под: {user.FirstName} {user.LastName}\n" +
                                    $"Ваша роль: {user.Roles.RoleName}");
                    LoadPage("Клиент", user);
                }
                else
                {
                    WriteLoginHistory(context, login, false, "Неверный логин или пароль");
                    RegisterFailedAttempt(login, "Неверный логин или пароль", regenerateCaptcha: false);
                }
            }
        }

        private void RegisterFailedAttempt(string login, string reason, bool regenerateCaptcha)
        {
            failedAttempts++;

            if (failedAttempts >= MaxAttempts)
            {
                lockoutEndTime = DateTime.Now.AddSeconds(LockoutSeconds);
                StartLockout();
                return;
            }

            if (regenerateCaptcha || gridCaptcha.Visibility != Visibility.Visible)
                GenerateCaptcha();
            else
                GenerateCaptcha(); 
            txtBoxCaptcha.Clear();
            psbPassword.Clear();
            txtPasswordVisible.Clear();

            int left = MaxAttempts - failedAttempts;
            txtAttemptsInfo.Text = $"Осталось попыток: {left}";
        }

        private void WriteLoginHistory(MiningMetallurgyPlantEntities1 context,
                                       string login, bool isSuccess, string failReason)
        {
            try
            {
                var record = new LoginHistory
                {
                    UserLogin = login,
                    AttemptTime = DateTime.Now,
                    IsSuccess = isSuccess,
                    FailReason = failReason
                };
                context.LoginHistory.Add(record);
                context.SaveChanges();
            }
            catch
            {
            }
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = psbPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            psbPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Focus();
            txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            psbPassword.Password = txtPasswordVisible.Text;
            psbPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
        }

        private void psbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
                txtPasswordVisible.Text = psbPassword.Password;
        }

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
                psbPassword.Password = txtPasswordVisible.Text;
        }

        private string GetPassword()
        {
            return chkShowPassword.IsChecked == true
                ? txtPasswordVisible.Text.Trim()
                : psbPassword.Password.Trim();
        }

        private void LoadPage(string role, Users user)
        {
            switch (role)
            {
                case "Клиент":
                    NavigationService.Navigate(new Client(user, role));
                    break;
            }
        }
    }
}
