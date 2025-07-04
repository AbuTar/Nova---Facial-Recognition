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
using System.Windows.Shapes;

namespace Nova
{
    /// <summary>
    /// Interaction logic for AdminConfirmationWindow.xaml
    /// </summary>
    public partial class AdminConfirmationWindow : Window
    {
        private readonly string correctUsername = "Admin";
        private readonly string correctPassword = "Admin123";

        public AdminConfirmationWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string enteredUsername = Username.Text.Trim();
            string enteredPassword = Password.Text.Trim();

            if (enteredUsername == correctUsername && enteredPassword == correctPassword)
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                AdminButton();

            }
            else if (enteredPassword.Length < 8)
            {
                MessageBox.Show("Password is too short, must be at least 8 characters long");
            }

            else if (string.IsNullOrEmpty(enteredUsername) || string.IsNullOrEmpty(enteredPassword))
            {
                MessageBox.Show("You must use a username or password");
            }

            else
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AdminButton()
        {
            AdminWindow adminconfirm = new AdminWindow();

            // Show the new window
            adminconfirm.Show();

            // Close the current StartupMenuView's parent window
            Window.GetWindow(this)?.Close();

        }
    }



}
